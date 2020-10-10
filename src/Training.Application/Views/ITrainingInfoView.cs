using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;

namespace Training.Application.Views
{
    public interface ITrainingInfoView : IView
    {
        void UpdateTimer(in TimeSpan timeSpan);
        void UpdateTraining(double error, int epochs, int iterations, double? validationError);
        void ResetProgress();
    }
}
