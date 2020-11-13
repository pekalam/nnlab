using NNLib;
using Prism.Events;

namespace NeuralNetwork.Application.Messaging
{
    internal class IntNeuronClicked : PubSubEvent<Layer> { }

    internal class IntLayerClicked : PubSubEvent<(Layer layer, int layerIndex)> { }

    internal class IntNetDisplayAreaClicked : PubSubEvent {}

    internal class IntLayerListChanged : PubSubEvent{}

    internal class IntLayerAdded : PubSubEvent<(Layer layer,int index)> {}
    internal class IntLayerRemoved : PubSubEvent<int> {}
    internal class IntLayerModified : PubSubEvent<(int index, int neuronsCount)> {}

    internal class IntLayerEditorClosed : PubSubEvent {}
}
