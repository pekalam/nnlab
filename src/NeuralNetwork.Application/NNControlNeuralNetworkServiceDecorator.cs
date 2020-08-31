﻿using System.Linq;
using Common.Domain;
using NeuralNetwork.Domain;
using NNLib;
using NNLibAdapter;

namespace NeuralNetwork.Application
{
    internal class NNControlNeuralNetworkServiceDecorator : INeuralNetworkService
    {
        private readonly INeuralNetworkService _service;
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;

        public NNControlNeuralNetworkServiceDecorator(NeuralNetworkService service, AppState appState, ModuleState moduleState)
        {
            _service = service;
            _appState = appState;
            _moduleState = moduleState;
        }

        private NNLibModelAdapter ModelAdapter => _moduleState.ModelAdapter;

        private MLPNetwork NeuralNetwork => _appState.ActiveSession?.Network;

        public bool AddLayer()
        {
            var result = _service.AddLayer();
            ModelAdapter.AddLayer(NeuralNetwork.Layers[^1]);
            return result;
        }

        public bool? RemoveLayer(int layerIndex)
        {
            var result = _service.RemoveLayer(layerIndex);
            if (result != null)
            {
                ModelAdapter.RemoveLayer(layerIndex);
            }

            return result;
        }

        public bool SetNeuronsCount(Layer layer, int neuronsCount)
        {
            var layerModelAdapter = ModelAdapter.LayerModelAdapters.First(l => ((NNLibLayerAdapter)l).Layer == layer);
            layerModelAdapter.SetNeuronsCount(neuronsCount);
            return _service.SetNeuronsCount(layer, neuronsCount);
        }

        public void SetActivationFunction(Layer layer, IActivationFunction activationFunction)
        {
            _service.SetActivationFunction(layer, activationFunction);
        }

        public bool SetInputsCount(int inputsCount)
        {
            ModelAdapter.LayerModelAdapters[0].SetNeuronsCount(inputsCount);
            return _service.SetInputsCount(inputsCount);
        }

        public void ResetWeights(Layer layer)
        {
            _service.ResetWeights(layer);
        }

        public void ResetNeuralNetworkWeights()
        {
            _service.ResetNeuralNetworkWeights();
        }

        public Layer InsertAfter(int layerIndex)
        {
            var layer = _service.InsertAfter(layerIndex);
            ModelAdapter.InsertAfter(layerIndex, layer);
            return layer;
        }

        public Layer InsertBefore(int layerIndex)
        {
            var layer = _service.InsertBefore(layerIndex);
            ModelAdapter.InsertBefore(layerIndex, layer);
            return layer;
        }

        public MLPNetwork CreateNeuralNetwork(TrainingData trainingData)
        {
            return _service.CreateNeuralNetwork(trainingData);
        }
    }
}