using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Domain;
using Prism.Ioc;

namespace Common.Domain
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<AppState>();
        }
    }
}
