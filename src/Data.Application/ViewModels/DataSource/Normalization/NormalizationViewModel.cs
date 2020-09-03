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
        public NormalizationViewModel(INormalizationService service)
        {
            Service = service;
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
