using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace Common.Presentation.Behaviors
{
    public class UpDownFloatFormatBehavior : Behavior<NumericUpDown>
    {
        protected override void OnAttached()
        {
            AssociatedObject.ValueChanged += AssociatedObjectOnValueChanged;

            if (AssociatedObject.Value.HasValue && Math.Abs(AssociatedObject.Value.Value % 1) <= (Double.Epsilon * 100))
            {
                AssociatedObject.StringFormat = "G";
            }
            else
            {
                AssociatedObject.StringFormat = "###0.###################";
            }
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

    public abstract class DebounceKeyDownBehaviorBase<T> : Behavior<T> where T : UIElement
    {
        private readonly int _debounceDelay;
        private bool _keyDown;
        private bool _taskStarted;

        protected DebounceKeyDownBehaviorBase(int debounceDelay)
        {
            _debounceDelay = debounceDelay;
        }

        protected override void OnAttached()
        {
            AssociatedObject.KeyUp += OnKeyUp;
            AssociatedObject.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _keyDown = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_taskStarted) return;

            _taskStarted = true;
            Task.Run(async () =>
            {
                while (true)
                {
                    _keyDown = false;
                    await Task.Delay(_debounceDelay);
                    if (_keyDown)
                    {
                        continue;
                    }

                    break;
                }

                Invoke();
                _taskStarted = false;
            });
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyUp -= OnKeyUp;
            AssociatedObject.KeyUp -= OnKeyDown;
        }

        protected abstract void Invoke();
    }

    public class TextBoxKeyDownValidationBehavior : DebounceKeyDownBehaviorBase<TextBox>
    {
        public TextBoxKeyDownValidationBehavior() : base(200)
        {
        }

        protected override void Invoke()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                AssociatedObject.GetBindingExpression(TextBox.TextProperty)!.ValidateWithoutUpdate();
            });
        }
    }
    
    public class SubmitNumericUpDownKeyDownSubmitBehavior : DebounceKeyDownBehaviorBase<SubmitNumericUpDown>
    {
        public SubmitNumericUpDownKeyDownSubmitBehavior() : base(100)
        {
        }

        protected override void Invoke()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                AssociatedObject.GetBindingExpression(NumericUpDown.ValueProperty)!.UpdateSource();
            });
        }
    }
}
