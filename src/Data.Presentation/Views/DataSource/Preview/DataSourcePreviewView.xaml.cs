using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Data.Application.ViewModels.DataSource.Preview;

namespace Data.Presentation.Views.DataSource.Preview
{
    /// <summary>
    /// Interaction logic for DataSourcePreviewView
    /// </summary>
    public partial class DataSourcePreviewView : UserControl
    {
        public DataSourcePreviewView()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                Resources.Remove(typeof(UserControl));
            }


            InitializeComponent();
        }

        private void PreviewGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as DataSourcePreviewViewModel)!.PreviewLoaded = true;
        }

        private void InstancePreviewGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as DataSourcePreviewViewModel)!.InstancePreviewLoaded = true;
        }
    }
}

