using Prism.Ioc;
using Prism.Modularity;
using Training.Application;

namespace Training
{
    public class TrainingModule : IModule
    {
        public const int NavIdentifier = 3;


        public void OnInitialized(IContainerProvider cp)
        {
            cp.Resolve<ModuleController>().Run();
        }

        public void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleController>();

            Training.Domain.Bootstraper.RegisterTypes(cr);
            Training.Application.Bootstraper.RegisterTypes(cr);
            Training.Presentation.Bootstraper.RegisterTypes(cr);
        }
    }
}