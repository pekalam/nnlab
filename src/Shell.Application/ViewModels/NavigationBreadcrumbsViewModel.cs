using Common.Framework;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace Shell.Application.ViewModels
{
    public class NavigationBreadcrumbsViewModel : ViewModelBase<NavigationBreadcrumbsViewModel>
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private readonly Dictionary<int, BreadcrumbModel[]> _previousBreadcrumbs =
            new Dictionary<int, BreadcrumbModel[]>();


#pragma warning disable 8618
        public NavigationBreadcrumbsViewModel() { }
#pragma warning restore 8618

        [InjectionConstructor]
        public NavigationBreadcrumbsViewModel(IEventAggregator ea, IRegionManager rm)
        {
            _ea = ea;
            _rm = rm;
            _ea.GetEvent<ContentRegionViewChanged>().Subscribe(OnContentRegionViewChanged);

        }

        internal IReadOnlyDictionary<int, BreadcrumbModel[]> PreviousBreadcrumbs => _previousBreadcrumbs;
        public ObservableCollection<BreadcrumbModel> Breadcrumbs { get; } = new ObservableCollection<BreadcrumbModel>();

        private void RemoveUntilExisting(string breadcrumb)
        {
            var count = Breadcrumbs.Count;
            int ind = -1;
            for (int i = 0; i < Breadcrumbs.Count; i++)
            {
                if (breadcrumb == Breadcrumbs[i].Breadcrumb)
                {
                    ind = i;
                    break;
                }
            }

            if (ind == -1)
            {
                throw new ArgumentException($"Cannot find breadcrumb {breadcrumb}");
            }

            for (int i = count - 1; i > ind; i--)
            {
                Breadcrumbs.RemoveAt(i);
            }
        }

        private void OnContentRegionViewChanged()
        {
            var view = _rm.Regions[AppRegions.ContentRegion].ActiveViews.FirstOrDefault();
            Debug.Assert(view != null);

            var isModal = (bool)(view as DependencyObject)!.GetValue(BreadcrumbsHelper.IsModalProperty);
            var breadcrumb = (view as DependencyObject)!.GetValue(BreadcrumbsHelper.BreadcrumbProperty) as string;

            Debug.Assert(breadcrumb != null);

            if (isModal)
            {
                if (Breadcrumbs.FirstOrDefault(b => b.Breadcrumb == breadcrumb) != null)
                {
                    RemoveUntilExisting(breadcrumb);
                }
                else
                {
                    if (Breadcrumbs.Count < 3)
                    {
                        Breadcrumbs.Add(new BreadcrumbModel()
                            { Breadcrumb = breadcrumb });
                    }
                    else
                    {
                        throw new InvalidOperationException("NavigationBreadcrumbs cannot have more than 4 breadcrumbs");
                    }
                }
            }
            else
            {
                Breadcrumbs.Clear();
                Breadcrumbs.Add(new BreadcrumbModel()
                {
                    Breadcrumb = breadcrumb,
                });
            }

            RaisePropertyChanged(nameof(Breadcrumbs));
        }

        public void ClearPreviousBreadcrumbsExcept(int moduleNavId)
        {
            foreach (var key in _previousBreadcrumbs.Keys)
            {
                if (key != moduleNavId)
                {
                    _previousBreadcrumbs.Remove(key);
                }
            }
        }

        public void SaveBreadcrumbsForModule(int moduleNavId)
        {
            _previousBreadcrumbs[moduleNavId] = Breadcrumbs.Select(b => b.Clone()).ToArray();
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
            }
            RaisePropertyChanged(nameof(Breadcrumbs));
        }
    }

    public class BreadcrumbConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (value as ObservableCollection<BreadcrumbModel>)!;
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
            var val = (value as ObservableCollection<BreadcrumbModel>)!;
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
        public string Breadcrumb { get; set; } = null!;

        public BreadcrumbModel Clone()
        {
            return new BreadcrumbModel()
            {
                Breadcrumb = Breadcrumb
            };
        }
    }
}

