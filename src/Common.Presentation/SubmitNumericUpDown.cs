using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace Common.Presentation
{
    public class SubmitNumericUpDown : NumericUpDown
    {
        private RepeatButton? _repeatUp;
        private RepeatButton? _repeatDown;

        public SubmitNumericUpDown()
        {
            PreviewKeyDown += OnPreviewKeyDown;
            LostFocus += (sender, args) => UpdateSource();

        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = GetBindingExpression(NumericUpDown.ValueProperty);
                be?.UpdateSource();
            }
        }

        private void UpdateSource()
        {
            BindingExpression be = GetBindingExpression(NumericUpDown.ValueProperty);
            be?.UpdateSource();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _repeatUp = GetTemplateChild("PART_NumericUp") as RepeatButton;
            _repeatDown = GetTemplateChild("PART_NumericDown") as RepeatButton;

            _repeatUp!.Click += (sender, args) => UpdateSource();
            _repeatDown!.Click += (sender, args) => UpdateSource();
        }
    }
}
