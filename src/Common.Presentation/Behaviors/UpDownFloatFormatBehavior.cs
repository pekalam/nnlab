using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace Common.Presentation.Behaviors
{
    public class UpDownFloatFormatBehavior : Behavior<NumericUpDown>
    {
        protected override void OnAttached()
        {
            AssociatedObject.ValueChanged += AssociatedObjectOnValueChanged;
        }

        private void AssociatedObjectOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (e.NewValue.HasValue && Math.Abs(e.NewValue.Value % 1) <= (Double.Epsilon * 100))
            {
                AssociatedObject.StringFormat = "G";
            }
            else
            {
                AssociatedObject.StringFormat = "###0.###################";
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ValueChanged -= AssociatedObjectOnValueChanged;
        }
    }
}
