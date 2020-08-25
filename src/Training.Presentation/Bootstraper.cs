﻿using Prism.Ioc;
using Training.Application.ViewModels;
using Training.Presentation.Views;
using Training.Presentation.Views.PanelLayout;
using Training.Presentation.Views.PanelLayout.Layouts;

namespace Training.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterForNavigation<TrainingView>();
            cr.RegisterForNavigation<TrainingInfoView>();
            cr.RegisterForNavigation<TrainingReportView>();

            cr.RegisterForNavigation<PanelLayoutView>();
            cr.RegisterForNavigation<SingleLayoutView>();
            cr.RegisterForNavigation<Horizontal2LayoutView>();
            cr.RegisterForNavigation<Part3LayoutView>();
            cr.RegisterForNavigation<Part4LayoutView>();

            cr.RegisterForNavigation<MatrixTrainingPreviewView>();
            cr.RegisterForNavigation<ErrorPlotView>();
            cr.RegisterForNavigation<TrainingNetworkPreviewView>();
            cr.RegisterForNavigation<OutputPlotView>();
            cr.RegisterForNavigation<TrainingParametersView>();
            cr.RegisterForNavigation<ParametersEditView>();

            cr.RegisterDialog<PanelSelectView, PanelSelectViewModel>();
        }
    }
}