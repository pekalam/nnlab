using System;
using System.Windows.Controls;
using Common.Framework;

namespace Training.Application.Views
{
    public interface ITrainingParametersView : IView
    {
        bool HasErrors();

        event EventHandler<ValidationErrorEventArgs> ValidationError;
    }
}