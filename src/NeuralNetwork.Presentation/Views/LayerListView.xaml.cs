using NeuralNetwork.Application.ViewModels;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace NeuralNetwork.Presentation.Views
{
    /// <summary>
    /// Interaction logic for LayerListView.xaml
    /// </summary>
    public partial class LayerListView : UserControl
    {
        public LayerListView()
        {
            InitializeComponent();
            ((LayerListViewModel) DataContext).Layers.CollectionChanged += LayersOnCollectionChanged;
            ((LayerListViewModel)DataContext).PropertyChanged += LayerEditorView_PropertyChanged;
        }

        private void LayersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                listBox.ScrollIntoView(listBox.Items[^1]);
            }
        }

        private void LayerEditorView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerListViewModel.SelectedLayer))
            {
                listBox.ScrollIntoView(((LayerListViewModel)DataContext).SelectedLayer);
            }
        }

        private void LayerEditorView_LayerAdded()
        {
            listBox.ScrollIntoView(listBox.Items[^1]);
        }

        private void Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            listBox.SelectedItem = ((Button)sender).Tag;
        }
    }
}
