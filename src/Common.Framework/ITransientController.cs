namespace Common.Framework
{
    /// <summary>
    /// Controllers that implement this interface are created by mediating service when view model is created.
    /// </summary>
    /// <typeparam name="T">Type of service that is responsible for creating controller.</typeparam>
    public interface ITransientController<T> where T : IService
    {
        /// <summary>
        /// This method must be called by service.
        /// </summary>
        /// <param name="service"></param>
        void Initialize(T service);
    }

    /// <summary>
    /// Marker interface
    /// </summary>
    public interface ITransientController
    {
    }
}