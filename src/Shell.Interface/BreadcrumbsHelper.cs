using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Shell.Interface
{
    public class BreadcrumbsHelper
    {
        public static readonly DependencyProperty BreadcrumbProperty = DependencyProperty.RegisterAttached(
            "Breadcrumb", typeof(string), typeof(BreadcrumbsHelper), new PropertyMetadata(default(string)));

        public static void SetBreadcrumb(DependencyObject element, string value)
        {
            element.SetValue(BreadcrumbProperty, value);
        }

        public static string GetBreadcrumb(DependencyObject element)
        {
            return (string) element.GetValue(BreadcrumbProperty);
        }

        public static readonly DependencyProperty IsModalProperty = DependencyProperty.RegisterAttached(
            "IsModal", typeof(bool), typeof(BreadcrumbsHelper), new PropertyMetadata(default(bool)));

        public static void SetIsModal(DependencyObject element, bool value)
        {
            element.SetValue(IsModalProperty, value);
        }

        public static bool GetIsModal(DependencyObject element)
        {
            return (bool) element.GetValue(IsModalProperty);
        }
    }
}
