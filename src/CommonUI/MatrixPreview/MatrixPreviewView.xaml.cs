using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CommonUI.MatrixPreview
{
    /// <summary>
    /// Interaction logic for MatrixPreviewView.xaml
    /// </summary>
    public partial class MatrixPreviewView : UserControl
    {
        public static readonly DependencyProperty EditableProperty = DependencyProperty.Register(
            nameof(Editable), typeof(bool), typeof(MatrixPreviewView), new PropertyMetadata(default(bool), OnEditableChanged));

        private static void OnEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (MatrixPreviewView)d;
            if (view.DataContext is MatrixPreviewViewModel vm)
            {
                vm.ReadOnly = !(bool)e.NewValue;
            }
        }

        public static readonly DependencyProperty LayerSelectPanelVisibilityProperty = DependencyProperty.Register(
            "LayerSelectPanelVisibility", typeof(Visibility), typeof(MatrixPreviewView), new PropertyMetadata(Visibility.Visible));

        public Visibility LayerSelectPanelVisibility
        {
            get => (Visibility) GetValue(LayerSelectPanelVisibilityProperty);
            set => SetValue(LayerSelectPanelVisibilityProperty, value);
        }

        private bool _columnSelected;
        private object? _selectedColumn;


        public MatrixPreviewView()
        {
            InitializeComponent();
        }

        public bool Editable
        {
            get => (bool)GetValue(EditableProperty);
            set => SetValue(EditableProperty, value);
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridColumnHeader columnHeader && sender != _selectedColumn)
            {
                grid.Focus();

                _selectedColumn = sender;
                grid.SelectedCells.Clear();
                foreach (var item in grid.Items)
                {
                    grid.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
                }

                ((MatrixPreviewViewModel)DataContext).ColumnClicked.Execute(columnHeader.DisplayIndex);

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
                grid.SelectedCells.Clear();
                _columnSelected = false;
                _selectedColumn = null;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(DataContext))
            {
                if (DataContext is MatrixPreviewViewModel vm)
                {
                    vm.UpdateColumns = columns =>
                    {
                        grid.Columns.Clear();
                        foreach (var col in columns)
                        {
                            grid.Columns.Add(col);
                        }
                    };
                    vm.RaiseGridInitialized();
                    vm.ReadOnly = !(bool)GetValue(EditableProperty);
                }
            }
        }
    }
}