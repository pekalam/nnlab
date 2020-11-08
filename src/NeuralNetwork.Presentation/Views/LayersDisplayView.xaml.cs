using NeuralNetwork.Application.ViewModels;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace NeuralNetwork.Presentation.Views
{
    /// <summary>
    /// Interaction logic for LayersDisplayView.xaml
    /// </summary>
    public partial class LayersDisplayView : UserControl
    {
        public LayersDisplayView()
        {
            InitializeComponent();
            ((LayersDisplayViewModel) DataContext).Layers.CollectionChanged += LayersOnCollectionChanged;
            ((LayersDisplayViewModel)DataContext).PropertyChanged += LayerEditorView_PropertyChanged;
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
            if (e.PropertyName == nameof(LayersDisplayViewModel.SelectedLayer))
            {
                listBox.ScrollIntoView(((LayersDisplayViewModel)DataContext).SelectedLayer);
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
