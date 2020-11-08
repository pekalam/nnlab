using Prism.Regions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Training.Application.ViewModels.PanelLayout.Layouts
{
    public class SingleLayoutViewModel : LayoutViewModelBase
    {
        private PanelSelectModel _selected1 = null!;

        public SingleLayoutViewModel(IRegionManager rm) : base(rm)
        {
        }

        public PanelSelectModel Selected1
        {
            get => _selected1;
            set => SetProperty(ref _selected1, value);
        }

        protected override void OnPanelsSelected(string[] views, List<PanelSelectModel> selected, NavigationParameters navParams)
        {
            var viewName = views.First();
            Selected1 = selected.First();
            ClearAndNavgate(SingleLayoutRegions.SingleLayoutMainRegion, viewName, navParams);
            PropertyChanged += ViewModelOnPropertyChanged;
        }

        protected override void OnInactive()
        {
            Rm.Regions[SingleLayoutRegions.SingleLayoutMainRegion].RemoveAll();
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Selected1))
            {
                var newView = PanelToViewHelper.GetView(Selected1);
                ClearAndNavgate(SingleLayoutRegions.SingleLayoutMainRegion, newView, InitialNavParams);
            }
        }

    }

    public class Horizontal2LayoutRegions
    {
        public const string Horizontal2LayoutRegion1 = nameof(Horizontal2LayoutRegion1);
        public const string Horizontal2LayoutRegion2 = nameof(Horizontal2LayoutRegion2);
    }

    class Horizontal2LayoutViewModel : LayoutViewModelBase
    {
        private PanelSelectModel _selected1 = null!;
        private PanelSelectModel _selected2 = null!;

        public Horizontal2LayoutViewModel(IRegionManager rm) : base(rm)
        {
        }

        public PanelSelectModel Selected1
        {
            get => _selected1;
            set => SetProperty(ref _selected1, value);
        }

        public PanelSelectModel Selected2
        {
            get => _selected2;
            set => SetProperty(ref _selected2, value);
        }

        protected override void OnPanelsSelected(string[] views, List<PanelSelectModel> selected, NavigationParameters navParams)
        {
            var first = views.First();
            var second = views.ElementAt(1);
            Selected1 = selected[0];
            Selected2 = selected[1];
            Rm.Regions[Horizontal2LayoutRegions.Horizontal2LayoutRegion1].RequestNavigate(first, navParams);
            Rm.Regions[Horizontal2LayoutRegions.Horizontal2LayoutRegion2].RequestNavigate(second, navParams);
            PropertyChanged += vmOnPropertyChanged;
        }

        protected override void OnInactive()
        {
            Rm.Regions[Horizontal2LayoutRegions.Horizontal2LayoutRegion1].RemoveAll();
            Rm.Regions[Horizontal2LayoutRegions.Horizontal2LayoutRegion2].RemoveAll();
        }

        private void vmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Horizontal2LayoutViewModel.Selected1))
            {
                var newView = PanelToViewHelper.GetView(Selected1);
                ClearAndNavgate(Horizontal2LayoutRegions.Horizontal2LayoutRegion1, newView, InitialNavParams);
            }

            if (e.PropertyName == nameof(Horizontal2LayoutViewModel.Selected2))
            {
                var newView = PanelToViewHelper.GetView(Selected2);
                ClearAndNavgate(Horizontal2LayoutRegions.Horizontal2LayoutRegion2, newView, InitialNavParams);
            }
        }

    }

    public class Part3LayoutRegions
    {
        public const string Part3LayoutRegion1 = nameof(Part3LayoutRegion1);
        public const string Part3LayoutRegion2 = nameof(Part3LayoutRegion2);
        public const string Part3LayoutRegion3 = nameof(Part3LayoutRegion3);
    }

    class Part3LayoutViewModel : LayoutViewModelBase
    {
        private PanelSelectModel _selected1 = null!;
        private PanelSelectModel _selected2 = null!;
        private PanelSelectModel _selected3 = null!;

        public Part3LayoutViewModel(IRegionManager rm) : base(rm)
        {
        }

        public PanelSelectModel Selected1
        {
            get => _selected1;
            set => SetProperty(ref _selected1, value);
        }

        public PanelSelectModel Selected2
        {
            get => _selected2;
            set => SetProperty(ref _selected2, value);
        }

        public PanelSelectModel Selected3
        {
            get => _selected3;
            set => SetProperty(ref _selected3, value);
        }

        protected override void OnPanelsSelected(string[] views, List<PanelSelectModel> selected, NavigationParameters navParams)
        {
            var first = views.First();
            var second = views.ElementAt(1);
            var third = views.ElementAt(2);
            Selected1 = selected[0];
            Selected2 = selected[1];
            Selected3 = selected[2];
            Rm.Regions[Part3LayoutRegions.Part3LayoutRegion1].RequestNavigate(first, navParams);
            Rm.Regions[Part3LayoutRegions.Part3LayoutRegion2].RequestNavigate(second, navParams);
            Rm.Regions[Part3LayoutRegions.Part3LayoutRegion3].RequestNavigate(third, navParams);
            PropertyChanged += vmOnPropertyChanged;
        }

        protected override void OnInactive()
        {
            Rm.Regions[Part3LayoutRegions.Part3LayoutRegion1].RemoveAll();
            Rm.Regions[Part3LayoutRegions.Part3LayoutRegion2].RemoveAll();
            Rm.Regions[Part3LayoutRegions.Part3LayoutRegion3].RemoveAll();
        }

        private void vmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Part3LayoutViewModel.Selected1))
            {
                var newView = PanelToViewHelper.GetView(Selected1);
                ClearAndNavgate(Part3LayoutRegions.Part3LayoutRegion1, newView, InitialNavParams);
            }

            if (e.PropertyName == nameof(Part3LayoutViewModel.Selected2))
            {
                var newView = PanelToViewHelper.GetView(Selected2);
                ClearAndNavgate(Part3LayoutRegions.Part3LayoutRegion2, newView, InitialNavParams);
            }

            if (e.PropertyName == nameof(Part3LayoutViewModel.Selected3))
            {
                var newView = PanelToViewHelper.GetView(Selected3);
                ClearAndNavgate(Part3LayoutRegions.Part3LayoutRegion3, newView, InitialNavParams);
            }
        }


    }

    public class Part4LayoutRegions
    {
        public const string Part4LayoutRegion1 = nameof(Part4LayoutRegion1);
        public const string Part4LayoutRegion2 = nameof(Part4LayoutRegion2);
        public const string Part4LayoutRegion3 = nameof(Part4LayoutRegion3);
        public const string Part4LayoutRegion4 = nameof(Part4LayoutRegion4);
    }

    class Part4LayoutViewModel : LayoutViewModelBase
    {
        private PanelSelectModel _selected1 = null!;
        private PanelSelectModel _selected2 = null!;
        private PanelSelectModel _selected3 = null!;
        private PanelSelectModel _selected4 = null!;

        public Part4LayoutViewModel(IRegionManager rm) : base(rm)
        {
        }

        public PanelSelectModel Selected1
        {
            get => _selected1;
            set => SetProperty(ref _selected1, value);
        }

        public PanelSelectModel Selected2
        {
            get => _selected2;
            set => SetProperty(ref _selected2, value);
        }

        public PanelSelectModel Selected3
        {
            get => _selected3;
            set => SetProperty(ref _selected3, value);
        }

        public PanelSelectModel Selected4
        {
            get => _selected4;
            set => SetProperty(ref _selected4, value);
        }

        protected override void OnPanelsSelected(string[] views, List<PanelSelectModel> selected, NavigationParameters navParams)
        {
            var first = views[0];
            var second = views[1];
            var third = views[2];
            var fourth = views[3];
            Selected1 = selected[0];
            Selected2 = selected[1];
            Selected3 = selected[2];
            Selected4 = selected[3];
            Rm.RequestNavigate(Part4LayoutRegions.Part4LayoutRegion1, first, navParams);
            Rm.RequestNavigate(Part4LayoutRegions.Part4LayoutRegion2, second, navParams);
            Rm.RequestNavigate(Part4LayoutRegions.Part4LayoutRegion3, third, navParams);
            Rm.RequestNavigate(Part4LayoutRegions.Part4LayoutRegion4, fourth, navParams);
            PropertyChanged += vmOnPropertyChanged;
        }

        protected override void OnInactive()
        {
            Rm.Regions[Part4LayoutRegions.Part4LayoutRegion1].RemoveAll();
            Rm.Regions[Part4LayoutRegions.Part4LayoutRegion2].RemoveAll();
            Rm.Regions[Part4LayoutRegions.Part4LayoutRegion3].RemoveAll();
            Rm.Regions[Part4LayoutRegions.Part4LayoutRegion4].RemoveAll();
        }

        private void vmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Part4LayoutViewModel.Selected1))
            {
                var newView = PanelToViewHelper.GetView(Selected1);
                ClearAndNavgate(Part4LayoutRegions.Part4LayoutRegion1, newView, InitialNavParams);
            }

            if (e.PropertyName == nameof(Part4LayoutViewModel.Selected2))
            {
                var newView = PanelToViewHelper.GetView(Selected2);
                ClearAndNavgate(Part4LayoutRegions.Part4LayoutRegion2, newView, InitialNavParams);
            }

            if (e.PropertyName == nameof(Part4LayoutViewModel.Selected3))
            {
                var newView = PanelToViewHelper.GetView(Selected3);
                ClearAndNavgate(Part4LayoutRegions.Part4LayoutRegion3, newView, InitialNavParams);
            }

            if (e.PropertyName == nameof(Part4LayoutViewModel.Selected4))
            {
                var newView = PanelToViewHelper.GetView(Selected4);
                ClearAndNavgate(Part4LayoutRegions.Part4LayoutRegion4, newView, InitialNavParams);
            }
        }
    }
}
