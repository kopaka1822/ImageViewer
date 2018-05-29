using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class StatisticViewModel : INotifyPropertyChanged
    {
        private readonly int index;
        private readonly Models.Models models;
        private readonly StatisticsViewModel viewModel;
        // indicates if the data needs to be recomputed due to changes
        private bool refreshData = true;

        public StatisticViewModel(int index, Models.Models models, StatisticsViewModel viewModel)
        {
            this.index = index;
            this.models = models;
            this.viewModel = viewModel;
            this.models.Statistics.StatisticChanged += ModelOnStatisticChanged;
            this.models.Statistics.PropertyChanged += ModelOnPropertyChanged;
            this.models.Equations.Get(index).PropertyChanged += OnEquationPropertyChanged;
            this.models.FinalImages.Get(index).PropertyChanged += OnFinalImagePropertyChanged;
            
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(StatisticsViewModel.IsVisible):
                    if (viewModel.IsVisible)
                    {
                        TryUpdate();
                    }
                    break;
            }
        }

        private void OnFinalImagePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FinalImageModel.StatisticsTexture):
                    refreshData = true;
                    TryUpdate();
                    break;
            }
        }

        private void OnEquationPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationModel.Visible):
                    OnPropertyChanged(nameof(Visibility));
                    if (viewModel.IsVisible)
                    {
                        TryUpdate();
                    }
                    break;
            }
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(StatisticsModel.Channel):
                case nameof(StatisticsModel.ColorSpace):
                    OnPropertyChanged(nameof(Average));
                    OnPropertyChanged(nameof(Max));
                    OnPropertyChanged(nameof(Min));
                    break;
            }
        }

        private void ModelOnStatisticChanged(object sender, ChangedStatisticEvent e)
        {
            if (e.Index == index)
            {
                OnPropertyChanged(nameof(Average));
                OnPropertyChanged(nameof(Max));
                OnPropertyChanged(nameof(Min));
            }
        }

        public Visibility Visibility => models.Equations.Get(index).Visible ? Visibility.Visible : Visibility.Collapsed;

        public string Average
        {
            get => models.Statistics.Get(index).Avg.Get(models.Statistics.ColorSpace).Get(models.Statistics.Channel).ToString(App.GetCulture());
            set { }
        }

        public string Min
        {
            get => models.Statistics.Get(index).Min.Get(models.Statistics.ColorSpace).Get(models.Statistics.Channel).ToString(App.GetCulture());
            set { }
        }

        public string Max
        {
            get => models.Statistics.Get(index).Max.Get(models.Statistics.ColorSpace).Get(models.Statistics.Channel).ToString(App.GetCulture());
            set { }
        }

        /// <summary>
        /// tries to update the data if anythong changed and the current results are visible to the user
        /// </summary>
        private void TryUpdate()
        {
            // nothing to refresh
            if (!refreshData) return;
            // control not visible
            if (!viewModel.IsVisible) return;
            // equation not visible
            if (!models.Equations.Get(index).Visible) return;

            var statTex = models.FinalImages.Get(index).StatisticsTexture;
            if (statTex == null) return;

            // update the data
            var disableGl = models.GlContext.Enable();
            try
            {
                var linAvg = models.GlData.LinearAvgStatistics.Run(statTex, models);
                var linMin = models.GlData.LinearMinStatistics.Run(statTex, models);
                var linMax = models.GlData.LinearMaxStatistics.Run(statTex, models);

                var srgbAvg = models.GlData.SrgbAvgStatistics.Run(statTex, models);
                var srgbMin = models.GlData.SrgbMinStatistics.Run(statTex, models);
                var srgbMax = models.GlData.SrgbMaxStatistics.Run(statTex, models);

                var avgSpace = new StatisticModel.ColorSpace(
                    new StatisticModel.Channel(linAvg.Alpha, linAvg.Red, linAvg.Green, linAvg.Blue),
                    new StatisticModel.Channel(srgbAvg.Alpha, srgbAvg.Red, srgbAvg.Green, srgbAvg.Blue));

                var minSpace = new StatisticModel.ColorSpace(
                    new StatisticModel.Channel(linMin.Alpha, linMin.Red, linMin.Green, linMin.Blue),
                    new StatisticModel.Channel(srgbMin.Alpha, srgbMin.Red, srgbMin.Green, srgbMin.Blue));

                var maxSpace = new StatisticModel.ColorSpace(
                    new StatisticModel.Channel(linMax.Alpha, linMax.Red, linMax.Green, linMax.Blue),
                    new StatisticModel.Channel(srgbMax.Alpha, srgbMax.Red, srgbMax.Green, srgbMax.Blue));

                models.Statistics.UpdateModel(new StatisticModel(avgSpace, minSpace, maxSpace), index);
            }
            catch (Exception e)
            {
                App.ShowErrorDialog(models.App.Window, e.Message);
            }
            finally
            {
                if(disableGl)
                    models.GlContext.Disable();
            }

            refreshData = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
