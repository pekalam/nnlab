using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NNLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
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

    public class LayerListViewModel : ViewModelBase<LayerListViewModel>
    {
        private ObservableCollection<LayerEditorItemModel> _layers = new ObservableCollection<LayerEditorItemModel>();
        private LayerEditorItemModel? _selectedLayer;

                
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public LayerListViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        [InjectionConstructor]
        public LayerListViewModel(ILayerListController controller)
        {
            Controller = controller;
            controller.Initialize(this);
        }

        public ILayerListController Controller { get; }

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

        public void CreateLayers(IEnumerable<Layer> lauers)
        {
            var collection = new ObservableCollection<LayerEditorItemModel>(lauers.Select(CreateLayerModel));
            collection.Add(new LayerEditorItemModel()
            {
                IsAddLayerItem = true,
                IsOutputLayer = false,
                LayerIndex = collection.Count,
                TotalNeurons = 0,
                AddLayer = Controller.AddLayerCommand
            });
            Layers = collection;
        }

        private LayerEditorItemModel CreateLayerModel(Layer layer, int ind)
        {
            return new LayerEditorItemModel()
            {
                IsOutputLayer = layer.IsOutputLayer,
                LayerIndex = ind,
                TotalNeurons = layer.NeuronsCount,
                RemoveLayer = Controller.RemoveLayerCommand,
                EditLayer = Controller.EditLayerCommand,
                InsertAfter = Controller.InsertAfterCommand,
                InsertBefore = Controller.InsertBeforeCommand
            };
        }
    }
}
