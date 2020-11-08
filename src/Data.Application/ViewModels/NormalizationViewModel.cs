using System.ComponentModel;
using Common.Domain;
using Common.Framework;
using Data.Application.Controllers.DataSource;
using MathNet.Numerics.Providers.LinearAlgebra;
using Unity;

namespace Data.Application.ViewModels
{
    public class NormalizationViewModel : ViewModelBase<NormalizationViewModel>
    {
        private bool _noneChecked;
        private bool _minMaxChecked;
        private bool _meanChecked;
        private bool _stdChecked;
        private INormalizationController? _controller;

        public NormalizationViewModel()
        {
            
        }

        [InjectionConstructor]
        public NormalizationViewModel(INormalizationController controller)
        {
            Controller = controller;

            controller.Initialize(this);
        }

        public INormalizationController? Controller
        {
            get => _controller;
            set => SetProperty(ref _controller, value);
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
