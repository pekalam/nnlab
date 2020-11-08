using System.Windows;
using System.Windows.Controls;
using Common.Domain;
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
                   Validation.GetHasError(ValidationTargetError);
        }
    }
}
