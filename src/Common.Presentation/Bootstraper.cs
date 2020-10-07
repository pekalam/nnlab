using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Prism.Ioc;

namespace Common.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.Register<IMetroDialogService, MetroDialogService>();
        }
    }
}
