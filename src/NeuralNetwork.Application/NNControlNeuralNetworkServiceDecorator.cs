using Common.Domain;
using NeuralNetwork.Domain;
using NNLib;
using NNLib.ActivationFunction;
using NNLib.MLP;
using NNLibAdapter;
using System.Linq;
using NeuralNetwork.Application.Messaging;
using Prism.Events;

namespace NeuralNetwork.Application
{
    internal class NNControlNeuralNetworkServiceDecorator : INeuralNetworkService
    {
        private readonly INeuralNetworkService _service;
        private readonly AppState _appState;
        private readonly IEventAggregator _ea;

        public NNControlNeuralNetworkServiceDecorator(NeuralNetworkService service, AppState appState, IEventAggregator ea)
        {
            _service = service;
            _appState = appState;
            _ea = ea;
        }


        private MLPNetwork NeuralNetwork => _appState.ActiveSession?.Network!;

        public bool AddLayer()
        {
            var result = _service.AddLayer();
            _ea.GetEvent<IntLayerAdded>().Publish((_appState.ActiveSession!.Network!.Layers[^1], _appState.ActiveSession!.Network!.Layers.Count));
            return result;
        }

        public bool? RemoveLayer(int layerIndex)
        {
            var result = _service.RemoveLayer(layerIndex);
            if (result != null)
            {
                _ea.GetEvent<IntLayerRemoved>().Publish(layerIndex);
            }
            return result;
        }

        public bool SetNeuronsCount(Layer layer, int neuronsCount)
        {
            var result= _service.SetNeuronsCount(layer, neuronsCount);

            int index = 1;
            foreach (var netLayer in _appState.ActiveSession!.Network!.BaseLayers)
            {
                if (netLayer == layer)
                {
                    break;
                }

                index++;
            }

            _ea.GetEvent<IntLayerModified>().Publish((index, layer.NeuronsCount));
            return result;
        }

        public void SetActivationFunction(Layer layer, IActivationFunction activationFunction)
        {
            _service.SetActivationFunction(layer, activationFunction);
        }

        public bool SetInputsCount(int inputsCount)
        {
            var result = _service.SetInputsCount(inputsCount);
            _ea.GetEvent<IntLayerModified>().Publish((0, inputsCount));
            return result;
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
            _ea.GetEvent<IntLayerAdded>().Publish((layer,layerIndex));
            return layer;
        }

        public Layer InsertBefore(int layerIndex)
        {
            var layer = _service.InsertBefore(layerIndex);
            _ea.GetEvent<IntLayerAdded>().Publish((layer,layerIndex));
            return layer;
        }

        public MLPNetwork CreateNeuralNetwork(TrainingData trainingData)
        {
            var network = _service.CreateNeuralNetwork(trainingData);
            return network;
        }

        public void ChangeParamsInitMethod<T>(Layer layer, ParamsInitMethod newMethod, bool reset, T? options = null) where T : class
        {
            _service.ChangeParamsInitMethod(layer,newMethod, reset, options);
        }

        public void AdjustNetworkToData(TrainingData data)
        {
            var outBefore = _appState.ActiveSession!.Network!.BaseLayers[^1].NeuronsCount;
            var inBefore = _appState.ActiveSession!.Network!.BaseLayers[0].InputsCount;

            _service.AdjustNetworkToData(data);

            var outAfter = _appState.ActiveSession!.Network!.BaseLayers[^1].NeuronsCount;
            var inAfter = _appState.ActiveSession!.Network!.BaseLayers[0].InputsCount;

            if (outBefore != outAfter)
            {
                _ea.GetEvent<IntLayerModified>().Publish((_appState.ActiveSession.Network!.BaseLayers.Count, outAfter));
            }

            if (inBefore != inAfter)
            {
                _ea.GetEvent<IntLayerModified>().Publish((0, inAfter));
            }
        }
    }
}