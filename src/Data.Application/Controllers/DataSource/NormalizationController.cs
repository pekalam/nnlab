using Data.Application.Services;
using Data.Domain.Services;
using Prism.Commands;
using System;

namespace Data.Application.Controllers.DataSource
{
    internal class NormalizationController
    {
        private NormalizationService _service;
        private INormalizationDomainService _normalizationService;

        public NormalizationController(NormalizationService service, INormalizationDomainService normalizationService)
        {
            _service = service;
            _normalizationService = normalizationService;

            _service.NoNormalizationCommand = new DelegateCommand(NoNormalization);
            _service.MinMaxNormalizationCommand = new DelegateCommand(MinMaxNormalization);
            _service.MeanNormalizationCommand = new DelegateCommand(MeanNormalization);
            _service.StdNormalizationCommand = new DelegateCommand(StdNormalization);
        }

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
    }
}
