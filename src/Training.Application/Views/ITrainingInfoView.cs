using Common.Framework;
using System;

namespace Training.Application.Views
{
    public interface ITrainingInfoView : IView
    {
        void UpdateTimer(in TimeSpan timeSpan);
        void UpdateTraining(double error, int epochs, int iterations, double? validationError);
        void ResetProgress();
    }
}
