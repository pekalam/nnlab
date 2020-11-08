using Prism.Ioc;

namespace Common.Domain
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<AppState>();
        }
    }
}
