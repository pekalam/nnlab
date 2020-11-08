using Common.Framework;

namespace Training.Application.Views
{
    public interface ITrainingParametersView : IView
    {
        bool HasErrors();
    }
}