using Common.Framework;
using System.Windows;

namespace Data.Application.Views
{
    public interface ISingleFileSourceView : IView
    {
        event RoutedEventHandler Loaded;
    }
}
