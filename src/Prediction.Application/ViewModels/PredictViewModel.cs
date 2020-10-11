using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;
using NNLib.Common;
using NNLibAdapter;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prediction.Application.Services;
using Prism.Regions;
using SharedUI.BasicPlot;
using SharedUI.MatrixPreview;

namespace Prediction.Application.ViewModels
{
    public class PredictViewModel : ViewModelBase<PredictViewModel>
    {
        private NNLibModelAdapter? _modelAdapter;
        private bool _showPlotPrediction;
        private double _startValue;
        private double _endValue;
        private double _interval = 0.25;
        private DataSetType[] _plotSetTypes;
        private DataSetType _selectedPlotSetType;
        private MatrixPreviewViewModel _outputMatrixVm = new MatrixPreviewViewModel();

#pragma warning disable 8618
        public PredictViewModel()
#pragma warning restore 8618
        {
            
        }

        public PredictViewModel(IPredictService service)
        {
            Service = service;
            PlotModel = new BasicPlotModel()
            {
                Wpf = true,
            };
            PlotModel.Model.Series.Add(DataPredictionLineSeries);
            PlotModel.Model.Series.Add(DataScatterSeries);
            PlotModel.Model.Series.Add(PredictionLineSeries);
            PlotModel.Model.Series.Add(PredictionScatterSeries);

            var l = new Legend
            {
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendPosition = LegendPosition.RightTop,
                LegendPlacement = LegendPlacement.Inside,
            };

            PlotModel.Model.Legends.Add(l);
        }

        public NNLibModelAdapter? ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }

        public IPredictService Service { get; }

        public MatrixPreviewViewModel InputMatrixVm { get; set; } = new MatrixPreviewViewModel();

        public MatrixPreviewViewModel OutputMatrixVm
        {
            get => _outputMatrixVm;
            set => SetProperty(ref _outputMatrixVm, value);
        }

        public BasicPlotModel PlotModel { get; set; }

        public LineSeries DataPredictionLineSeries { get; set; } = new LineSeries()
        {
            Title = "Prediction"
        };

        public ScatterSeries DataScatterSeries { get; set; } = new ScatterSeries()
        {
            Title = "Original data",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(0, 0, 255),
        };

        public LineSeries PredictionLineSeries { get; set; } = new LineSeries()
        {
            Title = "Prediction"
        };


        public ScatterSeries PredictionScatterSeries { get; set; } = new ScatterSeries()
        {
            Title = "Prediction",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(0, 255, 0),
        };

        public DataSetType[] PlotSetTypes
        {
            get => _plotSetTypes;
            set => SetProperty(ref _plotSetTypes,value);
        }

        public DataSetType SelectedPlotSetType
        {
            get => _selectedPlotSetType;
            set => SetProperty(ref _selectedPlotSetType, value);
        }

        public bool ShowPlotPrediction
        {
            get => _showPlotPrediction;
            set => SetProperty(ref _showPlotPrediction, value);
        }

        public double StartValue
        {
            get => _startValue;
            set => SetProperty(ref _startValue, value);
        }

        public double EndValue
        {
            get => _endValue;
            set => SetProperty(ref _endValue, value);
        }

        public double Interval
        {
            get => _interval;
            set
            {
                if(value <= 0) throw new Exception("Interval must be greater than 0");
                SetProperty(ref _interval, value);
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated?.Invoke(navigationContext);
        }
    }
}
