using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageFramework.Model
{
    /// <summary>
    /// gives information about image pipelines in progress
    /// </summary>
    public class ProgressModel : INotifyPropertyChanged
    {
        private float progress = 0.0f;

        /// <summary>
        /// progress between 0.0 and 1.0
        /// </summary>
        public float Progress
        {
            get => progress;
            internal set
            {
                float clamped = Math.Min(Math.Max(value, 0.0f), 1.0f);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (clamped == progress) return;
                progress = clamped;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private string what = "";

        /// <summary>
        /// desription of the thing being processed
        /// </summary>
        public string What
        {
            get => what;
            internal set
            {
                var val = value ?? "";
                if (val.Equals(what)) return;
                what = val;
                OnPropertyChanged(nameof(What));
            }
        }

        private bool isProcessing = false;

        /// <summary>
        /// indicates if anything is being processed
        /// </summary>
        public bool IsProcessing
        {
            get => isProcessing;
            internal set
            {
                if (value == isProcessing) return;
                isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
