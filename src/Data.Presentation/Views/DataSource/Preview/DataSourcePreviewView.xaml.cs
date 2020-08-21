using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Data.Application.ViewModels.DataSource.Preview;

namespace Data.Presentation.Views.DataSource.Preview
{
    /// <summary>
    /// Interaction logic for DataSourcePreviewView
    /// </summary>
    public partial class DataSourcePreviewView : UserControl
    {
        private bool _columnSelected;
        private object? _selectedColumn;

        public DataSourcePreviewView()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                Resources.Remove(typeof(UserControl));
            }


            InitializeComponent();
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridColumnHeader columnHeader && sender != _selectedColumn)
            {
                PreviewGrid.Focus();

                _selectedColumn = sender;
                PreviewGrid.SelectedCells.Clear();
                foreach (var item in PreviewGrid.Items)
                {
                    PreviewGrid.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
                }

                ((DataSourcePreviewViewModel)DataContext).PreviewColumnClicked.Execute(columnHeader.Content as string);

                _columnSelected = true;
                e.Handled = true;
            }
            else if (sender is DataGridColumnHeader && sender == _selectedColumn)
            {
                DeselectColumn();
            }
        }

        private void DeselectColumn()
        {
            if (_columnSelected)
            {
                PreviewGrid.SelectedCells.Clear();
                _columnSelected = false;
                _selectedColumn = null;
            }
        }

        private void DataGridCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DeselectColumn();
        }

        private void PreviewGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as DataSourcePreviewViewModel).PreviewLoaded = true;
        }

        private void InstancePreviewGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as DataSourcePreviewViewModel).InstancePreviewLoaded = true;
        }
    }
}

