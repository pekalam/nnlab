using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Common.Framework;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace Shell.Application.ViewModels
{
    public class NavigationBreadcrumbsViewModel : ViewModelBase<NavigationBreadcrumbsViewModel>
    {
        private IEventAggregator _ea;
        private IRegionManager _rm;
        private bool _show = true;
        private readonly Dictionary<int, bool> _previousShow = new Dictionary<int, bool>();
        private readonly Dictionary<int, BreadcrumbModel[]> _previousBreadcrumbs =
            new Dictionary<int, BreadcrumbModel[]>();

        public NavigationBreadcrumbsViewModel(IEventAggregator ea, IRegionManager rm)
        {
            _ea = ea;
            _rm = rm;
            _ea.GetEvent<ContentRegionViewChanged>().Subscribe(OnContentRegionViewChanged);

            NavigateCommand = new DelegateCommand<BreadcrumbModel>(Navigate);
        }

        internal IReadOnlyDictionary<int, BreadcrumbModel[]> PreviousBreadcrumbs => _previousBreadcrumbs;
        public ObservableCollection<BreadcrumbModel> Breadcrumbs { get; } = new ObservableCollection<BreadcrumbModel>();

        public DelegateCommand<BreadcrumbModel> NavigateCommand { get; }

        public bool Show
        {
            get => _show;
            set => SetProperty(ref _show, value);
        }

        private void RemoveUntilExisting(string viewName)
        {
            var count = Breadcrumbs.Count;
            int viewInd = -1;
            for (int i = 0; i < Breadcrumbs.Count; i++)
            {
                if (viewName == Breadcrumbs[i].ViewName)
                {
                    viewInd = i;
                    break;
                }
            }

            if (viewInd == -1)
            {
                throw new ArgumentException($"Cannot find breadcrumb with viewName {viewName}");
            }

            for (int i = count - 1; i > viewInd; i--)
            {
                Breadcrumbs.RemoveAt(i);
            }
        }

        private void OnContentRegionViewChanged(ContentRegionViewChangedEventArgs args)
        {
            Show = args.NavigationParameters.ShowBreadcrumbs;
            if (Breadcrumbs.FirstOrDefault(b => b.ViewName == args.ViewName) != null)
            {
                RemoveUntilExisting(args.ViewName);
            }
            else
            {
                if (Breadcrumbs.Count < 3)
                {
                    Breadcrumbs.Add(new BreadcrumbModel()
                        { ViewName = args.ViewName, Breadcrumb = args.NavigationParameters.Breadcrumb, NavParams=args.NavigationParameters });
                }
                else
                {
                    throw new InvalidOperationException("NavigationBreadcrumbs cannot have more than 4 breadcrumbs");
                }
            }

            RaisePropertyChanged(nameof(Breadcrumbs));
        }


        public void SaveBreadcrumbsForModule(int moduleNavId)
        {
            _previousBreadcrumbs[moduleNavId] = Breadcrumbs.Select(b => b.Clone()).ToArray();
            _previousShow[moduleNavId] = Show;
            Breadcrumbs.Clear();
        }

        public void TryRestoreBreadrumbsForModule(int moduleNavId)
        {
            if (_previousBreadcrumbs.TryGetValue(moduleNavId, out var previous))
            {
                Breadcrumbs.Clear();
                foreach (var breadcrumb in previous)
                {
                    Breadcrumbs.Add(breadcrumb);
                }

                Show = _previousShow[moduleNavId];
            }
            RaisePropertyChanged(nameof(Breadcrumbs));
        }

        private void Navigate(BreadcrumbModel breadcrumb)
        {
            if (breadcrumb.IsCustom)
            {
                breadcrumb.CustomNavigationAction(breadcrumb);
            }
            else
            {
                _rm.NavigateContentRegion(breadcrumb.ViewName, breadcrumb.NavParams);
            }
        }
    }

    public class BreadcrumbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as ObservableCollection<BreadcrumbModel>;
            var ind = System.Convert.ToInt32(parameter);

            if (ind < val.Count)
            {
                return val[ind];
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BreadcrumbStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as ObservableCollection<BreadcrumbModel>;
            var ind = System.Convert.ToInt32(parameter);

            if (ind < val.Count)
            {
                return val[ind].Breadcrumb;
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BreadcrumbModel
    {
        public string Breadcrumb { get; set; }
        public string ViewName { get; set; }
        public ContentRegionNavigationParameters NavParams { get; set; }
        public Action<BreadcrumbModel> CustomNavigationAction { get; set; }
        public bool IsCustom => CustomNavigationAction != null;

        public BreadcrumbModel Clone()
        {
            return new BreadcrumbModel()
            {
                Breadcrumb = Breadcrumb, CustomNavigationAction = CustomNavigationAction,NavParams = NavParams,ViewName = ViewName,
            };
        }
    }
}

