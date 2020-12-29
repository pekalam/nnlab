using System;
using System.Reflection;
using System.Windows;
using MahApps.Metro.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Common.Presentation
{
    public class SubmitNumericUpDown : NumericUpDown
    {
        private RepeatButton? _repeatUp;
        private RepeatButton? _repeatDown;

        public SubmitNumericUpDown()
        {
            PreviewKeyDown += OnPreviewKeyDown;
            LostFocus += (sender, args) => UpdateValuePropertyBindingSource(this);

        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateValuePropertyBindingSource(this);
            }
        }

        public static void UpdateValuePropertyBindingSource(SubmitNumericUpDown submit)
        {
            BindingExpression be = submit.GetBindingExpression(NumericUpDown.ValueProperty);

            var prop = be!.ResolvedSource.GetType().GetProperty(be.ResolvedSourcePropertyName);

            if (prop != null)
            {
                var tValue = submit.GetValue(be.TargetProperty);
                if (tValue == null)
                {
                    be.UpdateSource();
                    return;
                }
                try
                {
                    tValue = Convert.ChangeType(tValue, prop.PropertyType);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (tValue == null || !tValue.Equals(prop.GetValue(be.ResolvedSource)))
                {
                    be.UpdateSource();
                }
            }
            else
            {
                be.UpdateSource();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _repeatUp = GetTemplateChild("PART_NumericUp") as RepeatButton;
            _repeatDown = GetTemplateChild("PART_NumericDown") as RepeatButton;

            _repeatUp!.Click += (sender, args) => UpdateValuePropertyBindingSource(this);
            _repeatDown!.Click += (sender, args) => UpdateValuePropertyBindingSource(this);
        }
    }
}
