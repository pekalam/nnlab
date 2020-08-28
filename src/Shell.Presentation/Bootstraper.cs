﻿using Prism.Ioc;
using Shell.Application;
using Shell.Application.Interfaces;
using Shell.Application.ViewModels;
using Shell.Interface;
using Shell.Presentation.Views;

namespace Shell.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .Register<IContentRegionHistoryService, ContentRegionHistoryService>();


            cr.RegisterDialog<DuplicateSessionDialogView, DuplicateSessionDialogViewModel>();
            cr.RegisterDialog<CreateSessionDialogView, CreateSessionDialogViewModel>();

        }
    }
}