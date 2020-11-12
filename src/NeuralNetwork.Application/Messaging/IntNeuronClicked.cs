using NNLib;
using Prism.Events;

namespace NeuralNetwork.Application.Messaging
{
    internal class IntNeuronClicked : PubSubEvent<Layer> { }

    internal class IntLayerClicked : PubSubEvent<(Layer layer, int layerIndex)> { }

    internal class IntNetDisplayAreaClicked : PubSubEvent {}

    internal class IntLayerListChanged : PubSubEvent{}
}
