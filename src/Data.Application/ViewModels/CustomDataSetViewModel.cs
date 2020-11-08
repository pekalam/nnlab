using Common.Framework;
using Data.Application.Controllers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.MatrixPreview;
using Unity;

namespace Data.Application.ViewModels
{
    public class CustomDataSetViewModel : ViewModelBase<CustomDataSetViewModel>
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public CustomDataSetViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.        
        }

        [InjectionConstructor]
        public CustomDataSetViewModel(ICustomDataSetController customDataSetController, MatrixPreviewViewModel matrixVm)
        {
            KeepAlive = false;
            Controller = customDataSetController;
            MatrixVm = matrixVm;
            matrixVm.CanRemoveItem = true;

            PlotModel.MouseDown += (sender, args) => Controller.PlotMouseDownCommand.Execute(args);

            PlotModel.Series.Add(Scatter);
            PlotModel.Series.Add(Line);

            customDataSetController.Initialize(this);
        }

        public ICustomDataSetController Controller { get; set; }


        public ScatterSeries Scatter { get; } = new ScatterSeries()
        {
            MarkerType = MarkerType.Circle,
            MarkerFill = OxyColor.FromRgb(0, 0, 255),
        };
        public LineSeries Line { get; } = new LineSeries()
        {
            Color = OxyColor.FromRgb(0, 255, 0),
        };

        public PlotModel PlotModel { get; set; } = new PlotModel()
        {
            Axes =
            {
                new LinearAxis()
                {
                    Title = "x",
                    Position = AxisPosition.Bottom,
                    MajorGridlineStyle = LineStyle.Solid,
                },
                new LinearAxis()
                {
                    Title = "y",
                    Position = AxisPosition.Left,
                    MajorGridlineStyle = LineStyle.Solid,
                }
            }
        };

        public PlotController PlotController { get; } = new PlotController();


        public MatrixPreviewViewModel MatrixVm { get; }


        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Controller.Navigated(navigationContext);
        }
    }
}