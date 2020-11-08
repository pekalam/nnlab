using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Prism.Commands;
using Prism.Ioc;

namespace Data.Application.Controllers.DataSource
{
    public interface INormalizationController : ITransientController
    {
        DelegateCommand NoNormalizationCommand { get; }
        DelegateCommand MinMaxNormalizationCommand { get; }
        DelegateCommand MeanNormalizationCommand { get; }
        DelegateCommand StdNormalizationCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<INormalizationController, NormalizationController>();
        }
    }

    internal class NormalizationController : ControllerBase<NormalizationViewModel>, INormalizationController
    {
        private readonly INormalizationDomainService _normalizationService;
        private readonly AppStateHelper _helper;
        private bool _ignoreCmd;
        private bool _cmdCall;

        public NormalizationController(INormalizationDomainService normalizationService, AppState appState)
        {
            _normalizationService = normalizationService;
            _helper = new AppStateHelper(appState);

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                if (_cmdCall)
                {
                    _cmdCall = false;
                    return;
                }

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
            }, s => s switch
            {
                nameof(TrainingData.Variables) => true,
                nameof(TrainingData.NormalizationMethod) => true,
                _ => false,
            });


            NoNormalizationCommand = new DelegateCommand(NoNormalization);
            MinMaxNormalizationCommand = new DelegateCommand(MinMaxNormalization);
            MeanNormalizationCommand = new DelegateCommand(MeanNormalization);
            StdNormalizationCommand = new DelegateCommand(StdNormalization);
        }

        protected override void VmCreated()
        {
            _helper.OnTrainingDataInSession(SetVmNormalizationMethod);
        }

        private void SetVmNormalizationMethod(TrainingData? data)
        {
            if(data == null) return;
            _ignoreCmd = true;
            switch (data.NormalizationMethod)
            {
                case NormalizationMethod.None:
                    Vm!.NoneChecked = true;
                    break;
                case NormalizationMethod.Mean:
                    Vm!.MeanChecked = true;
                    break;
                case NormalizationMethod.MinMax:
                    Vm!.MinMaxChecked = true;
                    break;
                case NormalizationMethod.Std:
                    Vm!.StdChecked = true;
                    break;
            }
            _ignoreCmd = false;
        }

        private void StdNormalization()
        {
            if(_ignoreCmd) return;
            _cmdCall = true;
            _normalizationService.StdNormalization();
        }

        private void MeanNormalization()
        {
            if (_ignoreCmd) return;
            _cmdCall = true;
            _normalizationService.MeanNormalization();
        }

        private void MinMaxNormalization()
        {
            if (_ignoreCmd) return;
            _cmdCall = true;
            _normalizationService.MinMaxNormalization();
        }

        private void NoNormalization()
        {
            if (_ignoreCmd) return;
            _cmdCall = true;
            _normalizationService.NoNormalization();
        }

        public DelegateCommand NoNormalizationCommand { get; }
        public DelegateCommand MinMaxNormalizationCommand { get; }
        public DelegateCommand MeanNormalizationCommand { get; }
        public DelegateCommand StdNormalizationCommand { get; }
    }
}
