using Prism.Regions;

namespace Common.Framework.Extensions
{
    public static class NavigationParametersExtensions
    {
        public static T GetOrDefault<T>(this NavigationParameters parameters, string key, T? defValue = null) where T : class
        {
            if (parameters.TryGetValue(key, out T value))
            {
                return value;
            }

            return defValue ?? value;
        }

        public static T GetOrDefault<T>(this NavigationParameters parameters, string key, T? defValue = null) where T : struct
        {
            if (parameters.TryGetValue(key, out T value))
            {
                return value;
            }

            return defValue ?? value;
        }
    }
}