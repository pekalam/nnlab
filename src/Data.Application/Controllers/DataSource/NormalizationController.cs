using Data.Application.Services;
using Data.Domain.Services;
using Prism.Commands;
using System;
using Common.Framework;
using Data.Application.Controllers.DataSource;
using Prism.Ioc;

namespace Data.Application.Services
{
    public interface INormalizationService
    {
        DelegateCommand NoNormalizationCommand { get; set; }
        DelegateCommand MinMaxNormalizationCommand { get; set; }
        DelegateCommand MeanNormalizationCommand { get; set; }
        DelegateCommand StdNormalizationCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<INormalizationService, NormalizationController>();
        }
    }
}

namespace Data.Application.Controllers.DataSource
{
    internal class NormalizationController : INormalizationService
    {
        private INormalizationDomainService _normalizationService;

        public NormalizationController(INormalizationDomainService normalizationService)
        {
            _normalizationService = normalizationService;

            NoNormalizationCommand = new DelegateCommand(NoNormalization);
            MinMaxNormalizationCommand = new DelegateCommand(MinMaxNormalization);
            MeanNormalizationCommand = new DelegateCommand(MeanNormalization);
            StdNormalizationCommand = new DelegateCommand(StdNormalization);
        }

        public DelegateCommand NoNormalizationCommand { get; set; }
        public DelegateCommand MinMaxNormalizationCommand { get; set; }
        public DelegateCommand MeanNormalizationCommand { get; set; }
        public DelegateCommand StdNormalizationCommand { get; set; }

        private void StdNormalization()
        {
            throw new NotImplementedException();
        }

        private void MeanNormalization()
        {
            throw new NotImplementedException();
        }

        private void MinMaxNormalization()
        {
            throw new NotImplementedException();
        }

        private void NoNormalization()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
