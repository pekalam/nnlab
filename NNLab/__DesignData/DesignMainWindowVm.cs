using System;
using System.Collections.Generic;
using System.Text;
using NNLab.ViewModels;
using Prism.Events;

namespace NNLab.__DesignData
{
    class DesignMainWindowVm : MainWindowViewModel
    {
    }

    class DesignNavigationBreadcrumbsVm : NavigationBreadcrumbsViewModel {
        public DesignNavigationBreadcrumbsVm() : base(null, null)
        {
        }
    }
}
