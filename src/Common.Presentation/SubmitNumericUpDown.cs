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
        private RepeatButton repeatUp;
        private RepeatButton repeatDown;

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
            this.repeatUp = this.GetTemplateChild("PART_NumericUp") as RepeatButton;
            this.repeatDown = this.GetTemplateChild("PART_NumericDown") as RepeatButton;

            this.repeatUp.Click += (sender, args) => UpdateSource();
            this.repeatDown.Click += (sender, args) => UpdateSource();
        }
    }
}
