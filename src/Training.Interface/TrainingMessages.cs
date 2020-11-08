using Prism.Events;

namespace Training.Interface
{
    public class TrainingSessionStarted : PubSubEvent
    {
    }

    public class TrainingSessionStopped : PubSubEvent
    {
    }

    public class TrainingSessionPaused : PubSubEvent
    {
    }
}
