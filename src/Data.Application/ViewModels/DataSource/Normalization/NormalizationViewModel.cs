using Common.Framework;
using Data.Application.Services;

namespace Data.Application.ViewModels.DataSource.Normalization
{
    public class NormalizationViewModel : ViewModelBase<NormalizationViewModel>
    {
        private bool _noneChecked = true;
        private bool _minMaxChecked;
        private bool _meanChecked;
        private bool _stdChecked;

        public NormalizationViewModel(INormalizationService service)
        {
            Service = service;
        }

        public INormalizationService Service { get; }

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
