using NNLib;
using Prism.Events;

namespace NeuralNetwork.Application.Messaging
{
    internal class IntNeuronClicked : PubSubEvent<(Layer Layer, int NeuronIndex)> { }

    internal class IntLayerClicked : PubSubEvent<(Layer layer, int layerIndex)> { }
}
