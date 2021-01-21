using Common.Domain;
using NNLib;
using NNLib.ActivationFunction;
using NNLib.MLP;

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
        bool InsertAfter(int layerIndex);
        bool InsertBefore(int layerIndex);
        MLPNetwork CreateNeuralNetwork(TrainingData trainingData);
        void ChangeParamsInitMethod<T>(Layer layer, WeightsInitMethod newMethod, bool reset, T? options = null) where T : class;

        void AdjustNetworkToData(TrainingData data);
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
            var newLayer =
                new PerceptronLayer(NeuralNetwork.Layers[^1].NeuronsCount, 1, new LinearActivationFunction());
            NeuralNetwork.AddLayer(newLayer);
            if (Validate())
            {
                _appState.ActiveSession?.RaiseNetworkStructureChanged();
                return true;
            }
            return false;
        }

        public bool? RemoveLayer(int layerIndex)
        {
            if (NeuralNetwork.TotalLayers >= 2)
            {
                var toRemove = NeuralNetwork.Layers[layerIndex];
                NeuralNetwork.RemoveLayer(toRemove);
                if (Validate())
                {
                    _appState.ActiveSession?.RaiseNetworkStructureChanged();
                    return true;
                }
                return false;
            }

            return null;
        }

        public bool SetNeuronsCount(Layer layer, int neuronsCount)
        {
            layer.NeuronsCount = neuronsCount;
            if (Validate())
            {
                _appState.ActiveSession?.RaiseNetworkStructureChanged();
                return true;
            }
            return false;
        }

        public void SetActivationFunction(Layer layer, IActivationFunction activationFunction)
        {
            ((PerceptronLayer) layer).ActivationFunction = activationFunction;
            _appState.ActiveSession?.RaiseNetworkParametersChanged();
        }

        public bool SetInputsCount(int inputsCount)
        {
            NeuralNetwork.BaseLayers[0].InputsCount = inputsCount;
            if (Validate())
            {
                _appState.ActiveSession?.RaiseNetworkStructureChanged();
                return true;
            }
            return false;
        }

        public void ResetWeights(Layer layer)
        {
            layer.ResetParameters();
            _appState.ActiveSession?.RaiseNetworkParametersChanged();
        }

        public void ResetNeuralNetworkWeights()
        {
            NeuralNetwork.ResetParameters();
            _appState.ActiveSession?.RaiseNetworkParametersChanged();
        }

        public bool InsertAfter(int layerIndex)
        {
            var layer = NeuralNetwork.InsertAfter(layerIndex);
            if (Validate())
            {
                _appState.ActiveSession?.RaiseNetworkStructureChanged();
                return true;
            }
            return false;
        }

        public bool InsertBefore(int layerIndex)
        {
            var layer = NeuralNetwork.InsertBefore(layerIndex);
            if (Validate())
            {
                _appState.ActiveSession?.RaiseNetworkStructureChanged();
                return true;
            }
            return false;
        }

        public MLPNetwork CreateNeuralNetwork(TrainingData trainingData)
        {
            var inputCount = trainingData.Variables.InputVariableNames.Length;
            var outputCount = trainingData.Variables.TargetVariableNames.Length;

            return new MLPNetwork(
                new PerceptronLayer(inputCount, 5, new SigmoidActivationFunction()),
                new PerceptronLayer(5, outputCount, new LinearActivationFunction()));
        }

        public void ChangeParamsInitMethod<T>(Layer layer, WeightsInitMethod newMethod, bool reset, T? options = null) where T : class
        {
            if (_appState.ActiveSession?.Network == null) return;
            layer.MatrixBuilder = WeightsInitMethodAssembler.FromWeightsInitMethod(newMethod, options);

            if (reset)
            {
                layer.ResetParameters();
            }

            _appState.ActiveSession?.RaiseNetworkParametersChanged();
        }

        public void AdjustNetworkToData(TrainingData data)
        {
            _appState.ActiveSession!.Network!.Layers[0].InputsCount = data.Variables.InputVariableNames.Length;
            _appState.ActiveSession!.Network!.Layers[^1].NeuronsCount = data.Variables.TargetVariableNames.Length;
            _appState.ActiveSession?.RaiseNetworkStructureChanged();
        }
    }
}