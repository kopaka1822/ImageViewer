using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.ImageLoader;

namespace ImageFramework.Model.Progress
{
    public class TaskCompletedEventArgs : EventArgs
    {
        public TaskCompletedEventArgs(bool success)
        {
            Success = success;
        }

        // task completed succesfully
        public bool Success { get; }
    }

    public delegate void TaskCompletedEventHandler(object sender, TaskCompletedEventArgs args);

    /// <summary>
    /// gives information about image pipelines in progress
    ///
    /// Usage for adding a task:
    /// 1. create a cancellation token: CancellationToken ct = new CancellationToken();
    /// 2. obtain the progress interface: progIf = ProgressModel.GetProgressInterface(ct);
    /// 3. obtain task from an async method: task = GetAsync(progIf,...)
    /// 4. enqueue the async task and supply the progress interface: ProgressModel.AddTask(task, ct)
    /// (5.) wait for your task: ProgressModel.WaitForTask() or ProgressModel.WaitForTaskAsync()
    /// </summary>
    public class ProgressModel : INotifyPropertyChanged, IDisposable
    {
        private float progress = 0.0f;
        private readonly Dll.ProgressDelegate onDllProgress;
        private Task currentTask = null;
        private CancellationTokenSource currentTaskCancellation = null;

        // helper class that forwards progress information
        private class ProgressInterface : IProgress
        {
            private readonly ProgressModel parent;

            public ProgressInterface(ProgressModel parent, CancellationToken token)
            {
                this.parent = parent;
                Token = token;
                Progress = 0.0f;
                What = "";
            }

            public void Report(float value) => Progress = value;

            public float Progress
            {
                get => parent.Progress;
                set => parent.Progress = value;
            }
            public string What
            {
                get => parent.What;
                set => parent.What = value;
            }
            public CancellationToken Token { get; }
            public IProgress CreateSubProgress(float maxProgress)
            {
                return new SubProgress(this, maxProgress);
            }
        }

        public ProgressModel()
        {
            // set dll progress
            onDllProgress = OnDllProgress;
            Dll.set_progress_callback(onDllProgress);
        }

        internal IProgress GetProgressInterface(CancellationToken ct)
        {
            return new ProgressInterface(this, ct);
        }

        private uint OnDllProgress(float prog, string description)
        {
            if (!IsProcessing) return 0; // ignore for now => progress when opening files without task

            if (enableDllProgress)
            {
                Progress = prog;
                if (What != description)
                    What = description;
            }

            if (currentTaskCancellation.IsCancellationRequested)
                return 1;

            return 0;
        }

        /// <summary>
        /// progress between 0.0 and 1.0
        /// </summary>
        public float Progress
        {
            get => progress;
            private set
            {
                float clamped = Math.Min(Math.Max(value, 0.0f), 1.0f);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (clamped == progress) return;
                progress = clamped;
                OnPropertyChanged(nameof(Progress));
            }
        }

        /// <summary>
        /// progress reports from the ImageLoader dll will be used to set progress and what
        /// </summary>
        private bool enableDllProgress = false;

        private string what = "";

        /// <summary>
        /// description of the thing being processed
        /// </summary>
        public string What
        {
            get => what;
            private set
            {
                var val = value ?? "";
                //if (val.Equals(what)) return;
                what = val;
                OnPropertyChanged(nameof(What));
            }
        }

        private string lastError = "";

        public string LastError
        {
            get => lastError;
            private set
            {
                if (value == lastError) return;
                lastError = value;
                OnPropertyChanged(nameof(LastError));
            }
        }

        // indicates if the last task that completed was cancelled by the user
        public bool LastTaskCancelledByUser { get; private set; } = false;

        // will be invoked if the last task failed
        public event TaskCompletedEventHandler TaskCompleted;

        /// <summary>
        /// indicates if anything is being processed
        /// </summary>
        public bool IsProcessing => currentTask != null;

        /// <summary>
        /// adds a new task. If a previous task exist, it will be a blocking wait first.
        /// </summary>
        /// <param name="t">new task</param>
        /// <param name="cts">cancellation token that corresponds to the task</param>
        /// <param name="enableDllProgress">if true, the Progress and What property will be set automatically by calls from the DxImageLoader.dll (this should be true when loading or saving single images, it should be false when loading or saving multiple images)</param>
        internal void AddTask(Task t, CancellationTokenSource cts, bool enableDllProgress)
        {
            if(currentTask != null)
                WaitForTask();

            Debug.Assert(cts != null);
            Debug.Assert(t != null);
            currentTask = t;
            currentTaskCancellation = cts;
            LastError = "";
            Progress = 0.0f;
            What = "";
            this.enableDllProgress = enableDllProgress;
            // automatically unregister task when finished or failed
            t.ContinueWith(OnTaskFinished);
            OnPropertyChanged(nameof(IsProcessing));
        }

        /// waits until the active task has finished
        public void WaitForTask()
        {
            try
            {
                currentTask?.Wait();
            }
            catch (Exception)
            {
                // this is handled by OnTaskFinished
            }
        }

        /// returns a task that waits for the active task to finish.
        /// This is guaranteed to never throw an exception (information about exceptions are in LastError)
        public async Task WaitForTaskAsync()
        {
            if (currentTask == null) return;
            try
            {
                await currentTask;
            }
            catch (Exception)
            {
                // this is handled by OnTaskFinished
            }
        }

        // callback that will be executed after the active task finished
        private void OnTaskFinished(Task prevTask)
        {

            LastTaskCancelledByUser = false;
            if (currentTaskCancellation != null)
            {
                LastTaskCancelledByUser = currentTaskCancellation.IsCancellationRequested;
                currentTaskCancellation.Dispose();
                currentTaskCancellation = null;
            }
            
            // don't set error if cancellation was requested by the user
            if (prevTask.Exception != null && !LastTaskCancelledByUser)
            {
                if (prevTask.Exception.InnerException != null) LastError = prevTask.Exception.InnerException.Message;
            }

            currentTask = null;
            OnPropertyChanged(nameof(IsProcessing));
            OnTaskCompleted(new TaskCompletedEventArgs(prevTask.IsCompleted));
        }

        // cancels the current task
        public void Cancel()
        {
            currentTaskCancellation?.Cancel();
            WaitForTask();
        }

        public async void CancelAsync()
        {
            currentTaskCancellation?.Cancel();
            await WaitForTaskAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Cancel();
            Debug.Assert(currentTask == null);
            Debug.Assert(currentTaskCancellation == null);
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs args)
        {
            TaskCompleted?.Invoke(this, args);
        }
    }
}
