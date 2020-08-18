using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Prism.Regions;

namespace Infrastructure
{
    public interface IActionMenuNavigationService
    {
        void SetLeftMenu<T>() where T : UserControl, new();
        void SetRightMenu<T>() where T : UserControl, new();
    }

    internal class ActionMenuNavigationService : IActionMenuNavigationService
    {
        private IRegionManager _rm;

        public ActionMenuNavigationService(IRegionManager rm)
        {
            _rm = rm;
        }

        public void SetLeftMenu<T>() where T : UserControl, new()
        {
            var content = new T();
            _rm.Regions[AppRegions.ActionMenuLeftRegion].Add(content);
        }

        public void SetRightMenu<T>() where T : UserControl, new()
        {
            var content = new T();
            _rm.Regions[AppRegions.ActionMenuRightRegion].Add(content);
        }
    }
}
