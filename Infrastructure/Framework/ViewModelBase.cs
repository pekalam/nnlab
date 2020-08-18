using System;
using Prism;
using Prism.Mvvm;
using Prism.Regions;

namespace Infrastructure
{
    /// <summary>
    /// Base class of view models.
    /// </summary>
    /// <typeparam name="T">Inheriting class type</typeparam>
    public class ViewModelBase<T> : BindableBase, INavigationAware, IRegionMemberLifetime, IActiveAware where T : ViewModelBase<T>
    {
        public static T Instance { get; private set; }
        public static event Action Created;

        public ViewModelBase()
        {
            Instance = this as T;
            Created?.Invoke();
        }

        public bool KeepAlive { get; set; }
        public bool IsActive { get; set; }
        public event EventHandler IsActiveChanged;

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }


    /// <summary>
    /// Base class of view models that interact with view by the use of view abstraction that implements <see cref="IView"/> interface
    /// </summary>
    /// <typeparam name="T">Inheriting class type</typeparam>
    /// <typeparam name="TV">View abstraction type</typeparam>
    public class ViewModelBase<T, TV> : ViewModelBase<T> where T : ViewModelBase<T> where TV : class,IView
    {
        /// <summary>
        /// Provides view abstraction for view model.
        /// </summary>
        /// <remarks>
        /// This property is set in <see cref="OnNavigatedTo"/> method.
        /// </remarks>
        protected TV View { get; private set; }

        /// <summary>
        /// Called after <see cref="View"/> property is set.
        /// </summary>
        /// <param name="view">View abstraction</param>
        protected virtual void ViewChanged(TV view) { }

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

    /// <summary>
    /// Marker interface for service that is mediating between vm and controller
    /// </summary>
    public interface IService { }

    /// <summary>
    /// Controllers that implement this interface are created by mediating service when view model is created.
    /// </summary>
    /// <typeparam name="T">Type of service that is responsible for creating controller.</typeparam>
    public interface ITransientControllerBase<T> where T : IService
    {
        /// <summary>
        /// This method must be called by service.
        /// </summary>
        /// <param name="service"></param>
        void Initialize(T service);
    }

    /// <summary>
    /// Controllers that implement this interface are created during module startup.
    /// </summary>
    public interface ISingletonController
    {
        /// <summary>
        /// This method is called when module starts.
        /// </summary>
        void Initialize();
    }
}