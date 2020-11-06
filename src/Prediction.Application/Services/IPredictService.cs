using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using NNLib.Common;
using NNLib.Data;
using NNLib.MLP;
using NNLibAdapter;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prediction.Application.Controllers;
using Prediction.Application.ViewModels;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using SharedUI.MatrixPreview;

namespace Prediction.Application.Services
{
    public interface IPredictService : ITransientController
    {
        DelegateCommand PredictCommand { get; }
        Action<NavigationContext> Navigated { get; set; }

        DelegateCommand<DataSetType?> PredictPlotCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IPredictService, PredictController>();

        }
    }
}
