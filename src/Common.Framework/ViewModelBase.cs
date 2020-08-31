using Prism;
using Prism.Mvvm;
using Prism.Regions;
using System;
using Microsoft.VisualBasic;

namespace Common.Framework
{
    /// <summary>
    /// Base class of view models.
    /// </summary>
    /// <typeparam name="T">Inheriting class type</typeparam>
    public class ViewModelBase<T> : BindableBase, INavigationAware, IRegionMemberLifetime, IActiveAware, IJournalAware
        where T : ViewModelBase<T>
    {
        private bool _isActive;
        public static T? Instance { get; private set; }
        public static event Action Created;

        public ViewModelBase()
        {
            Instance = this as T;
            Created?.Invoke();
        }

        public bool KeepAlive { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                // if (!value)
                // {
                //     Instance = null;
                // }
                // else
                // {
                //     Instance = this as T;
                // }
                IsActiveChanged?.Invoke(this, null!);
            }
        }

        public event EventHandler IsActiveChanged;

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public bool PersistInHistory()
        {
            return false;
        }
    }


    /// <summary>
    /// Base class of view models that interact with view by the use of view abstraction that implements <see cref="IView"/> interface
    /// </summary>
    /// <typeparam name="T">Inheriting class type</typeparam>
    /// <typeparam name="TV">View abstraction type</typeparam>
    public class ViewModelBase<T, TV> : ViewModelBase<T> where T : ViewModelBase<T> where TV : class, IView
    {
        /// <summary>
        /// Provides view abstraction for view model.
        /// </summary>
        /// <remarks>
        /// This property is set in <see cref="OnNavigatedTo"/> method.
        /// </remarks>
        public TV View { get; private set; }

        /// <summary>
        /// Called after <see cref="View"/> property is set.
        /// </summary>
        /// <param name="view">View abstraction</param>
        protected virtual void ViewChanged(TV view)
        {
        }

        public void SetView(TV view)
        {
            View = view;
            ViewChanged(view);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            View = navigationContext.Parameters["view"] as TV;
            ViewChanged(View);
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }


    public interface IViewModelAccessor 
    {
        T? Get<T>() where T : ViewModelBase<T>;
        void OnCreated<T>(Action action) where T : ViewModelBase<T>;
    }

    internal class DefaultViewModelAccessor : IViewModelAccessor
    {
        public T? Get<T>() where T : ViewModelBase<T>
        {
            return ViewModelBase<T>.Instance;
        }

        public void OnCreated<T>(Action action) where T : ViewModelBase<T>
        {
            ViewModelBase<T>.Created += action;
        }
    }
}