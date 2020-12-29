using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Common.Domain;
using Common.Presentation;
using MahApps.Metro.Controls;
using Prism.Mvvm;
using Training.Application.ViewModels;
using Training.Application.Views;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TrainingParametersView
    /// </summary>
    public partial class TrainingParametersView : UserControl, ITrainingParametersView
    {
        public TrainingParametersView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(DataContext))
            {
                if (DataContext is TrainingParametersViewModel vm)
                {
                    vm.ParametersReseted += () =>
                    {
                        LearningRate.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        Momentum.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        BatchSize.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        DampingParameterDec.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        DampingParameterInc.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        ValidationEpochThreshold.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        ValidationTargetError.GetBindingExpression(SubmitNumericUpDown.ValueProperty).UpdateTarget();
                        MaxLearningTime.GetBindingExpression(TimePicker.SelectedDateTimeProperty).UpdateTarget();
                    };

                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(TrainingParametersViewModel.IsMaxLearningTimeChecked) &&
                            vm.IsMaxLearningTimeChecked)
                        {
                            MaxLearningTime.GetBindingExpression(TimePicker.SelectedDateTimeProperty).UpdateTarget();
                        }
                    };
                }

            }
        }

        public bool HasErrors()
        {
            bool err;
            if ((TrainingAlgorithm) Algorithm.SelectedItem == TrainingAlgorithm.GradientDescent)
            {
                 err=  Validation.GetHasError(LearningRate) || Validation.GetHasError(Momentum) ||
                       Validation.GetHasError(BatchSize);
            }
            else
            {
                err =  Validation.GetHasError(DampingParameterInc) || Validation.GetHasError(DampingParameterDec);
            }

            return err || Validation.GetHasError(ValidationEpochThreshold) ||
                   Validation.GetHasError(ValidationTargetError) || Validation.GetHasError(MaxLearningTime);
        }

        public event EventHandler<ValidationErrorEventArgs> ValidationError;

        private void Validation_OnError(object? sender, ValidationErrorEventArgs e)
        {
            ValidationError?.Invoke(sender, e);
        }
    }
}
