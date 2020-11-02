using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Data.Application.ViewModels.DataSource.Preview;
using NNLib.Common;

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

        private void InstancePreviewGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var vm = (DataContext as DataSourcePreviewViewModel);
            if ((VariableUses)vm!.DataSourceInstance!.ExtendedProperties[e.Column.SortMemberPath]! == VariableUses.Target)
            {
                e.Column.HeaderStyle = (Style)Resources["TargetHeader"];
            }
        }

        private void PreviewGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var vm = (DataContext as DataSourcePreviewViewModel);
            if ((VariableUses)vm!.FileDataPreview!.ExtendedProperties[e.Column.SortMemberPath]! == VariableUses.Target)
            {
                e.Column.HeaderStyle = (Style)Resources["TargetHeader"];
            }
        }
    }
}

