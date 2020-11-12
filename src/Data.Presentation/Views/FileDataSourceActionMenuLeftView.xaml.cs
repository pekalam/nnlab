using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Data.Application.ViewModels;

namespace Data.Presentation.Views
{
    /// <summary>
    /// Interaction logic for FileDataSourceActionMenuLeftView
    /// </summary>
    public partial class FileDataSourceActionMenuLeftView : UserControl
    {
        public FileDataSourceActionMenuLeftView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            if (e.Property.Name == nameof(DataContext))
            {
                var vm = (DataContext as FileDataSourceViewModel)!;
                vm.PropertyChanged -= OnPropertyChanged;
                vm.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileDataSourceViewModel.IsDivideDataSetEnabled))
            {
                if ((sender as FileDataSourceViewModel)!.IsDivideDataSetEnabled)
                {
                    DivideDatasetButton.Visibility = Visibility.Visible;
                }
                else
                {
                    DivideDatasetButton.Visibility = Visibility.Collapsed;
                }
            }   
        }
    }
}
