using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Controllers.DataSource;

using Data.Domain;
using Prism.Ioc;
using Prism.Unity;

namespace Data.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .RegisterSingleton<IDataSetDivisionController, DataSetDivisionController>()
                .RegisterSingleton<IFileDataSourceController, FileDataSourceController>();

                INormalizationController.Register(cr);
                IStatisticsController.Register(cr);
                ICustomDataSetController.Register(cr);
                IVariablesSelectionController.Register(cr);

                IFileController.Register(cr);
                IFileDataSourceController.Register(cr);
                IDataSetDivisionController.Register(cr);

                cr.Register<ISingleFileController, SingleFileSourceController>()

                    .Register<IMultiFileSourceController, MultiFileSourceController>();
        }
    }
}
