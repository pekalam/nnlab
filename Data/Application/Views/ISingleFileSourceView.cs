using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infrastructure;

namespace Data.Application.Views
{
    public interface ISingleFileSourceView : IView
    {
        event RoutedEventHandler Loaded;
    }
}
