using Prism.Events;

namespace Training.Interface
{
    public class Class1
    {
    }

    public class TrainingValidationStarting : PubSubEvent { }
    public class TrainingValidationFinished : PubSubEvent<double> { }


    public class TrainingTestStarting : PubSubEvent { }
    public class TrainingTestFinished : PubSubEvent<double> { }
}
