using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Services;
using NNLib;
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
        public LayersDisplayViewModel(ILayersDisplayController service)
        {
            Service = service;
            service.Initialize(this);
        }

        public ILayersDisplayController Service { get; }

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


        public void AddLayer(Layer layer, int ind)
        {
            var layers = Layers;
            var layerItem = CreateLayerModel(layer, ind);

            layers.RemoveAt(layers.Count - 1);
            layers.Add(layerItem);
            layers.Add(new LayerEditorItemModel()
            {
                IsAddLayerItem = true,
                IsOutputLayer = false,
                LayerIndex = layers.Count,
                TotalNeurons = 0,
                AddLayer = Service.AddLayerCommand
            });

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
                AddLayer = Service.AddLayerCommand
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
                RemoveLayer = Service.RemoveLayerCommand,
                EditLayer = Service.EditLayerCommand,
                InsertAfter = Service.InsertAfterCommand,
                InsertBefore = Service.InsertBeforeCommand
            };
        }
    }
}
