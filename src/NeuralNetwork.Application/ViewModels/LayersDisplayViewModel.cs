using System.Collections.ObjectModel;
using System.Windows.Input;
using Common.Framework;
using NeuralNetwork.Application.Services;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class LayerEditorItemModel
    {
        public enum InsertModes
        {
            InsertBefore, InsertAfter
        }

        public int LayerIndex { get; set; }
        public int TotalNeurons { get; set; }
        public bool IsOutputLayer { get; set; }
        public bool IsAddLayerItem { get; set; }
        public ICommand? AddLayer { get; set; }
        public ICommand? RemoveLayer { get; set; }
        public ICommand? EditLayer { get; set; }
        public ICommand? InsertBefore { get; set; }
        public ICommand? InsertAfter { get; set; }
        public string LayerText => $"Layer {LayerIndex + 1}";

        public string LayerSecondText
        {
            get
            {
                if (IsOutputLayer)
                {
                    return "(output)";
                }

                return "";
            }
        }
    }

    public class LayersDisplayViewModel : ViewModelBase<LayersDisplayViewModel>
    {
        private ObservableCollection<LayerEditorItemModel> _layers = new ObservableCollection<LayerEditorItemModel>();
        private LayerEditorItemModel? _selectedLayer;

                
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public LayersDisplayViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        [InjectionConstructor]
        public LayersDisplayViewModel(ILayersDisplayService service)
        {
            Service = service;
        }

        public ILayersDisplayService Service { get; }

        public ObservableCollection<LayerEditorItemModel> Layers
        {
            get => _layers;
            set => SetProperty(ref _layers, value);
        }

        public LayerEditorItemModel? SelectedLayer
        {
            get => _selectedLayer;
            set => SetProperty(ref _selectedLayer, value);
        }
    }
}
