using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.ViewModels;
using NNLib;
using Prism.Commands;
using Prism.Ioc;

namespace NeuralNetwork.Application.Services
{
    public interface ILayersDisplayService : IService
    {
        DelegateCommand AddLayerCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> RemoveLayerCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> EditLayerCommand { get; set; }
        DelegateCommand<Layer> SelectLayerCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> LayerClickedCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> InsertAfterCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> InsertBeforeCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ITransientController<LayersDisplayService>, LayersDisplayController>()
                .Register<ILayersDisplayService, LayersDisplayService>();
        }
    }

    public class LayersDisplayService : ILayersDisplayService
    {
        private readonly IViewModelAccessor _accessor;

        public LayersDisplayService(ITransientController<LayersDisplayService> ctrl, IViewModelAccessor accessor)
        {
            _accessor = accessor;
            ctrl.Initialize(this);
        }

        public DelegateCommand AddLayerCommand { get; set; } = null!;
        public DelegateCommand<LayerEditorItemModel> RemoveLayerCommand { get; set; } = null!;
        public DelegateCommand<LayerEditorItemModel> EditLayerCommand { get; set; } = null!;
        public DelegateCommand<Layer> SelectLayerCommand { get; set; } = null!;
        public DelegateCommand<LayerEditorItemModel> LayerClickedCommand { get; set; } = null!;
        public DelegateCommand<LayerEditorItemModel> InsertAfterCommand { get; set; } = null!;
        public DelegateCommand<LayerEditorItemModel> InsertBeforeCommand { get; set; } = null!;


        public void AddLayer(Layer layer, int ind)
        {
            var layers = _accessor.Get<LayersDisplayViewModel>()!.Layers;
            var layerItem = CreateLayerModel(layer, ind);

            layers.RemoveAt(layers.Count - 1);
            layers.Add(layerItem);
            layers.Add(new LayerEditorItemModel()
            {
                IsAddLayerItem = true,
                IsInputLayer = false,
                IsOutputLayer = false,
                LayerIndex = layers.Count,
                TotalNeurons = 0,
                AddLayer = AddLayerCommand
            });

        }

        public void CreateLayers(IEnumerable<Layer> lauers)
        {
             var collection = new ObservableCollection<LayerEditorItemModel>(lauers.Select(CreateLayerModel));
             collection.Add(new LayerEditorItemModel()
             {
                 IsAddLayerItem = true,
                 IsInputLayer = false,
                 IsOutputLayer = false,
                 LayerIndex = collection.Count,
                 TotalNeurons = 0,
                 AddLayer = AddLayerCommand
             });
             _accessor.Get<LayersDisplayViewModel>()!.Layers = collection;
        }

        private LayerEditorItemModel CreateLayerModel(Layer layer, int ind)
        {
            return new LayerEditorItemModel()
            {
                IsInputLayer = layer.IsInputLayer,
                IsOutputLayer = layer.IsOutputLayer,
                LayerIndex = ind,
                TotalNeurons = layer.NeuronsCount,
                RemoveLayer = RemoveLayerCommand,
                EditLayer = EditLayerCommand,
                InsertAfter = InsertAfterCommand,
                InsertBefore = InsertBeforeCommand
            };
        }
    }
}