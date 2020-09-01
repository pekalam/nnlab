using System.ComponentModel;
using Common.Domain;
using Common.Framework;
using Data.Application.Services;
using MathNet.Numerics.Providers.LinearAlgebra;
using Unity;

namespace Data.Application.ViewModels.DataSource.Normalization
{
    public class NormalizationViewModel : ViewModelBase<NormalizationViewModel>
    {
        private bool _noneChecked;
        private bool _minMaxChecked;
        private bool _meanChecked;
        private bool _stdChecked;
        private INormalizationService? _service;

        public NormalizationViewModel()
        {
            
        }

        [InjectionConstructor]
        public NormalizationViewModel(INormalizationService service, AppState appState)
        {
            Service = service;

            var method = appState.ActiveSession!.TrainingData!.NormalizationMethod;
            SetCheckedFromMethod(method);

            appState.ActiveSessionChanged += (sender, args) =>
            {
                if (args.next.TrainingData != null)
                {
                    SetCheckedFromMethod(args.next.TrainingData.NormalizationMethod);
                }
                else
                {
                    args.next.PropertyChanged += OnActiveSessionTrainingDataChanged;
                }
            };
        }

        private void OnActiveSessionTrainingDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.TrainingData))
            {
                var session = (sender as Session)!;
                SetCheckedFromMethod(session.TrainingData!.NormalizationMethod);
                session.PropertyChanged -= OnActiveSessionTrainingDataChanged;
            }
        }

        private void SetCheckedFromMethod(NormalizationMethod method)
        {
            var temp = Service;
            Service = null;
            switch (method)
            {
                case NormalizationMethod.None:
                    NoneChecked = true;
                    break;
                case NormalizationMethod.Mean:
                    MeanChecked = true;
                    break;
                case NormalizationMethod.MinMax:
                    MinMaxChecked = true;
                    break;
                case NormalizationMethod.Std:
                    StdChecked = true;
                    break;
            }

            Service = temp;
        }

        public INormalizationService? Service
        {
            get => _service;
            set => SetProperty(ref _service, value);
        }

        public bool NoneChecked
        {
            get => _noneChecked;
            set => SetProperty(ref _noneChecked, value);
        }

        public bool MinMaxChecked
        {
            get => _minMaxChecked;
            set => SetProperty(ref _minMaxChecked, value);
        }

        public bool MeanChecked
        {
            get => _meanChecked;
            set => SetProperty(ref _meanChecked, value);
        }

        public bool StdChecked
        {
            get => _stdChecked;
            set => SetProperty(ref _stdChecked, value);
        }
    }
}
