using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Domain;
using NNLib;
using NNLibAdapter;
using Prism.Ioc;

namespace NeuralNetwork.Domain
{
    /// <summary>
    /// Service provides methods for editing neural network parameters
    /// </summary>
    public interface INeuralNetworkService
    {
        bool AddLayer();
        bool? RemoveLayer(int layerIndex);
        bool SetNeuronsCount(Layer layer, int neuronsCount);
        void SetActivationFunction(Layer layer, IActivationFunction activationFunction);
        bool SetInputsCount(int inputsCount);
        void ResetWeights(Layer layer);
        void ResetNeuralNetworkWeights();
        Layer InsertAfter(int layerIndex);
        Layer InsertBefore(int layerIndex);
        MLPNetwork CreateNeuralNetwork(TrainingData trainingData);
    }

    public class NeuralNetworkService : INeuralNetworkService
    {
        private readonly AppState _appState;

        public NeuralNetworkService(AppState appState)
        {
            _appState = appState;
        }

        private MLPNetwork NeuralNetwork => _appState.ActiveSession?.Network!;

        private bool Validate()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            if (NeuralNetwork.BaseLayers[0].InputsCount != trainingData.Variables.Indexes.InputVarIndexes.Length ||
                NeuralNetwork.BaseLayers[^1].NeuronsCount != trainingData.Variables.Indexes.TargetVarIndexes.Length)
            {
                return false;
            }

            return true;
        }

        public bool AddLayer()
        {
            var newLayer = new PerceptronLayer(NeuralNetwork.Layers[^1].NeuronsCount, 1, new LinearActivationFunction());
            NeuralNetwork.AddLayer(newLayer);
            _appState.ActiveSession?.RaiseNetworkStructureChanged();
            return Validate();
        }

        public bool? RemoveLayer(int layerIndex)
        {
            if (NeuralNetwork.TotalLayers >= 2)
            {
                var toRemove = NeuralNetwork.Layers[layerIndex];
                NeuralNetwork.RemoveLayer(toRemove);
                _appState.ActiveSession?.RaiseNetworkStructureChanged();
                return Validate();
            }

            return null;
        }

        public bool SetNeuronsCount(Layer layer, int neuronsCount)
        {
            layer.NeuronsCount = neuronsCount;
            _appState.ActiveSession?.RaiseNetworkStructureChanged();
            return Validate();
        }

        public void SetActivationFunction(Layer layer, IActivationFunction activationFunction)
        {
            ((PerceptronLayer)layer).ActivationFunction = activationFunction;
        }

        public bool SetInputsCount(int inputsCount)
        {
            NeuralNetwork.BaseLayers[0].InputsCount = inputsCount;
            _appState.ActiveSession?.RaiseNetworkStructureChanged();
            return Validate();
        }

        public void ResetWeights(Layer layer)
        {
            layer.RebuildMatrices();
        }

        public void ResetNeuralNetworkWeights()
        {
            NeuralNetwork.RebuildMatrices();
        }

        public Layer InsertAfter(int layerIndex)
        {
            var layer = NeuralNetwork.InsertAfter(layerIndex);
            _appState.ActiveSession?.RaiseNetworkStructureChanged();
            return layer;
        }

        public Layer InsertBefore(int layerIndex)
        {
            var layer = NeuralNetwork.InsertBefore(layerIndex);
            _appState.ActiveSession?.RaiseNetworkStructureChanged();
            return layer;
        }

        public MLPNetwork CreateNeuralNetwork(TrainingData trainingData)
        {
            var inputCount = trainingData.Variables.InputVariableNames.Length;
            var outputCount = trainingData.Variables.TargetVariableNames.Length;

            return new MLPNetwork(
                new PerceptronLayer(inputCount, inputCount, new LinearActivationFunction()),
                new PerceptronLayer(inputCount, 5, new SigmoidActivationFunction()),
                new PerceptronLayer(5, outputCount, new SigmoidActivationFunction()));
        }
    }

   

    public class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.Register<INeuralNetworkService, NeuralNetworkService>();
        }
    }
}
