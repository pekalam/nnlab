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
        void AdjustParametersToTrainingData(TrainingData trainingData);
        MLPNetwork CreateDefaultNetwork();
    }

    public class NeuralNetworkService : INeuralNetworkService
    {
        private AppState _appState;

        public NeuralNetworkService(AppState appState)
        {
            _appState = appState;
        }

        private MLPNetwork NeuralNetwork => _appState.ActiveSession?.Network;

        private bool Validate()
        {
            var trainingData = _appState.ActiveSession.TrainingData;
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
            return Validate();
        }

        public bool? RemoveLayer(int layerIndex)
        {
            if (NeuralNetwork.TotalLayers >= 2)
            {
                var toRemove = NeuralNetwork.Layers[layerIndex];
                NeuralNetwork.RemoveLayer(toRemove);
                return Validate();
            }

            return null;
        }

        public bool SetNeuronsCount(Layer layer, int neuronsCount)
        {
            layer.NeuronsCount = neuronsCount;
            return Validate();
        }

        public void SetActivationFunction(Layer layer, IActivationFunction activationFunction)
        {
            ((PerceptronLayer)layer).ActivationFunction = activationFunction;
        }

        public bool SetInputsCount(int inputsCount)
        {
            NeuralNetwork.BaseLayers[0].InputsCount = inputsCount;
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

        public void AdjustParametersToTrainingData(TrainingData trainingData)
        {
            var newInputCount = trainingData.Sets.TrainingSet.Input[0].RowCount;
            if (newInputCount != NeuralNetwork.Layers[0].InputsCount)
            {
                SetInputsCount(newInputCount);
            }

            var newOutputCount = trainingData.Sets.TrainingSet.Target[0].RowCount;
            if (newOutputCount != NeuralNetwork.Layers[^1].NeuronsCount)
            {
                SetNeuronsCount(NeuralNetwork.Layers[^1], newOutputCount);
            }
        }

        public MLPNetwork CreateDefaultNetwork()
        {
            return new MLPNetwork(new PerceptronLayer(1, 1, new LinearActivationFunction()),
                new PerceptronLayer(1, 5, new SigmoidActivationFunction()),
                new PerceptronLayer(5, 1, new SigmoidActivationFunction()));
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
