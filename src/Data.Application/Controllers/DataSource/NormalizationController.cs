using Data.Application.Services;
using Data.Domain.Services;
using Prism.Commands;
using System;
using System.ComponentModel;
using Common.Domain;
using Common.Framework;
using Data.Application.Controllers.DataSource;
using Data.Application.ViewModels.DataSource.Normalization;
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
    internal class NormalizationController : INormalizationService, ISingletonController
    {
        private readonly INormalizationDomainService _normalizationService;
        private readonly AppStateHelper _helper;
        private bool _ignoreCmd;
        private readonly IViewModelAccessor _accessor;

        public NormalizationController(INormalizationDomainService normalizationService, AppState appState, IViewModelAccessor accessor)
        {
            _normalizationService = normalizationService;
            _accessor = accessor;
            _helper = new AppStateHelper(appState);

            NoNormalizationCommand = new DelegateCommand(NoNormalization);
            MinMaxNormalizationCommand = new DelegateCommand(MinMaxNormalization);
            MeanNormalizationCommand = new DelegateCommand(MeanNormalization);
            StdNormalizationCommand = new DelegateCommand(StdNormalization);

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                if (data.Source == TrainingDataSource.Csv)
                {
                    switch (data.NormalizationMethod)
                    {
                        case NormalizationMethod.Mean:
                            MeanNormalization();
                            break;
                        case NormalizationMethod.Std:
                            StdNormalization();
                            break;
                        case NormalizationMethod.MinMax:
                            MinMaxNormalization();
                            break;
                    }
                }
            }, s => s == nameof(TrainingData.Variables));

            _helper.OnTrainingDataInSession(SetVmNormalizationMethod);
        }

        public DelegateCommand NoNormalizationCommand { get; set; }
        public DelegateCommand MinMaxNormalizationCommand { get; set; }
        public DelegateCommand MeanNormalizationCommand { get; set; }
        public DelegateCommand StdNormalizationCommand { get; set; }

        private void SetVmNormalizationMethod(TrainingData? data)
        {
            if(data == null) return;
            var vm = _accessor.Get<NormalizationViewModel>();
            if(vm == null) return;
            _ignoreCmd = true;
            switch (data.NormalizationMethod)
            {
                case NormalizationMethod.None:
                    vm.NoneChecked = true;
                    break;
                case NormalizationMethod.Mean:
                    vm.MeanChecked = true;
                    break;
                case NormalizationMethod.MinMax:
                    vm.MinMaxChecked = true;
                    break;
                case NormalizationMethod.Std:
                    vm.StdChecked = true;
                    break;
            }
            _ignoreCmd = false;
        }

        private void StdNormalization()
        {
            if(_ignoreCmd) return;
            _normalizationService.StdNormalization();
        }

        private void MeanNormalization()
        {
            if (_ignoreCmd) return;
            _normalizationService.MeanNormalization();
        }

        private void MinMaxNormalization()
        {
            if (_ignoreCmd) return;
            _normalizationService.MinMaxNormalization();
        }

        private void NoNormalization()
        {
            if (_ignoreCmd) return;
            _normalizationService.NoNormalization();
        }

        public void Initialize()
        {
        }
    }
}
