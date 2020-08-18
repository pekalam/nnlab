using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Infrastructure;
using Infrastructure.Messaging;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace NNLab.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private bool _isFlyoutOpen;
        private string _flyoutTitle;
        private string _title = "NNLab";

        public MainWindowViewModel(IEventAggregator ea, IRegionManager rm)
        {
            _ea = ea;
            _rm = rm;

            _ea.GetEvent<ShowFlyout>().Subscribe(args =>
            {
                FlyoutTitle = args.Title;
                IsFlyoutOpen = true;
            }, ThreadOption.UIThread, true);

            _ea.GetEvent<HideFlyout>()
                .Subscribe(() =>
                {
                    CloseFlyoutCommand.Execute(null);
                }, ThreadOption.UIThread, true);

            CloseFlyoutCommand = new DelegateCommand(() =>
            {
                IsFlyoutOpen = false;
                _rm.Regions[AppRegions.FlyoutRegion].RemoveAll();
            });
        }

        public ICommand CloseFlyoutCommand { get; set; }

        public bool IsFlyoutOpen
        {
            get => _isFlyoutOpen;
            set => SetProperty(ref _isFlyoutOpen, value);
        }

        public string FlyoutTitle
        {
            get => _flyoutTitle;
            set => SetProperty(ref _flyoutTitle, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}