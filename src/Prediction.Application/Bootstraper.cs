using Prediction.Application.Services;
using Prism.Ioc;

namespace Prediction.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            IPredictService.Register(cr);
        }
    }
}