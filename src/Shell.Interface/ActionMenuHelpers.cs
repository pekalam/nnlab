using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Prism;
using Prism.Regions;

namespace Shell.Interface
{
    public class ActionMenuHelpers
    {
        internal static IRegionManager? RegionManager { get; set; }

        public static readonly DependencyProperty LeftMenuViewProperty = DependencyProperty.RegisterAttached(
            "LeftMenuView", typeof(object), typeof(ActionMenuHelpers), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.AffectsRender, LeftMenuChangedCallback));


        private static void ChangeActionMenu(string regionName, DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(RegionManager != null);

            var rm = RegionManager;
            var ctx = (d as FrameworkElement)!.DataContext;
            var activeAware = ctx as IActiveAware;
            (e.NewValue as FrameworkElement)!.DataContext = ctx;

            activeAware!.IsActiveChanged += (sender, args) =>
            {
                if (!activeAware!.IsActive)
                {
                    rm.Regions[regionName].RemoveAll();
                }
                else
                {
                    if (!rm.Regions[regionName].Views.Contains(e.NewValue))
                    {
                        rm.Regions[regionName].Add(e.NewValue);
                    }
                }
            };
        }

        private static void LeftMenuChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChangeActionMenu(AppRegions.ActionMenuLeftRegion, d,e);
        }

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static void SetLeftMenuView(FrameworkElement element, object value)
        {
            element.SetValue(LeftMenuViewProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static object GetLeftMenuView(FrameworkElement element)
        {
            return (UserControl)element.GetValue(LeftMenuViewProperty);
        }


        public static readonly DependencyProperty RightMenuViewProperty = DependencyProperty.RegisterAttached(
            "RightMenuView", typeof(object), typeof(ActionMenuHelpers), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.AffectsRender, RightMenuChangedCallback));
        
        private static void RightMenuChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChangeActionMenu(AppRegions.ActionMenuRightRegion, d, e);
        }

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static void SetRightMenuView(FrameworkElement element, object value)
        {
            element.SetValue(RightMenuViewProperty, value);
        }
        
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static object GetRightMenuView(FrameworkElement element)
        {
            return (UserControl)element.GetValue(RightMenuViewProperty);
        }

    }
}