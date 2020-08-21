﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Prism.Regions;
using Shell.Application;
using Shell.Application.Interfaces;
using Shell.Interface;

namespace Shell.Presentation
{
    internal class ActionMenuNavigationService : IActionMenuNavigationService
    {
        private readonly IRegionManager _rm;

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

    internal class ContentRegionHistoryService : IContentRegionHistoryService
    {
        private IRegionManager _rm;
        //module identifier to previous content view
        private readonly Dictionary<int, object> _previousViews = new Dictionary<int, object>();

        public ContentRegionHistoryService(IRegionManager rm)
        {
            _rm = rm;
        }

        public void SaveContentForModule(int moduleNavId)
        {
            var currentView = _rm.Regions[AppRegions.ContentRegion].ActiveViews.FirstOrDefault();

            _previousViews[moduleNavId] = currentView ?? throw new NullReferenceException("Null content region active view");
        }

        public void TryRestoreContentForModule(int moduleNavId)
        {
            if (_previousViews.TryGetValue(moduleNavId, out var view))
            {
                if (_rm.Regions[AppRegions.ContentRegion].Views.Contains(view))
                {
                    _rm.Regions[AppRegions.ContentRegion].Activate(view);
                }
                else
                {
                    _rm.Regions[AppRegions.ContentRegion].Add(view);
                    _rm.Regions[AppRegions.ContentRegion].Activate(view);
                }
            }
        }
    }
}
