using Common.Framework;
using Prism.Ioc;

namespace Common.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.Register<IMetroDialogService, MetroDialogService>();
        }
    }
}
