using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ControlzEx.Standard;
using Prism.Commands;

namespace Infrastructure.Helpers
{
    public class TextboxHelpers
    {
        public static readonly DependencyProperty UpdateSourceOnReturnKey = DependencyProperty.RegisterAttached(
            "UpdateSourceOnReturnKey", typeof(bool), typeof(TextboxHelpers), new PropertyMetadata(default(bool)));


        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetUpdateSourceOnReturnKeyProperty(TextBox element)
        {
            return (bool) element.GetValue(UpdateSourceOnReturnKey);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetUpdateSourceOnReturnKeyProperty(TextBox element, bool value)
        {
            if (value)
            {
                var inputBinding = new InputBinding(new DelegateCommand(() =>
                {
                    BindingExpression binding = BindingOperations.GetBindingExpression(element, TextBox.TextProperty);
                    binding?.UpdateSource();
                }), new KeyGesture(Key.Return));
               
            }
            element.SetValue(UpdateSourceOnReturnKey, value);
        }
    }
}
