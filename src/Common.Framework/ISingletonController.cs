namespace Common.Framework
{
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