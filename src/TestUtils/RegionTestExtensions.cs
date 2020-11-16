using Common.Domain;
using Moq;
using NNLib.ActivationFunction;
using NNLib.Common;
using NNLib.Data;
using NNLib.MLP;
using Prism.Regions;
using Shell.Interface;
using System;
using System.Linq;

namespace TestUtils
{


    public static class RegionTestExtensions
    {
        public static void VerifyNavigation(this Mock<IRegion> region, string viewName, Times times)
        {
            region.Verify(
                f => f.RequestNavigate(It.Is<Uri>(uri => uri.ToString().Contains(viewName)),
                    It.IsAny<Action<NavigationResult>>()), times);
        }


        public static void VerifyContentNavigation(this Mock<IRegionManager> rm, string viewName,
            Times times)
        {
            try
            {
                rm.Verify(f => f.RequestNavigate(AppRegions.ContentRegion, viewName), times);
            }
            catch (MockException)
            {
                rm.Verify(f => f.RequestNavigate(AppRegions.ContentRegion, viewName,It.IsAny<NavigationParameters>()), times);
            }
        }
    }


    public static class MLPMocks
    {
        public static MLPNetwork ValidNet1 => new MLPNetwork(new PerceptronLayer(1, 10, new SigmoidActivationFunction()),
            new PerceptronLayer(10, 1, new ArcTanActivationFunction()));

        public static MLPNetwork ValidNet2 => new MLPNetwork(new PerceptronLayer(1, 10, new SigmoidActivationFunction()),
            new PerceptronLayer(10, 1, new ArcTanActivationFunction()),
                new PerceptronLayer(1, 2, new ArcTanActivationFunction()));

        public static MLPNetwork AndGateNet => new MLPNetwork(new PerceptronLayer(2, 2, new LinearActivationFunction()),
        new PerceptronLayer(2, 1, new SigmoidActivationFunction()));
    }

    public static class TrainingDataMocks
    {
        public static TrainingData ValidData1 = new TrainingData(
            new SupervisedTrainingData(SupervisedTrainingSamples.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d}})),
            new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                new[] {new VariableName("x"), new VariableName("y")}), TrainingDataSource.Memory, NormalizationMethod.None);


        public static TrainingData ValidData2 = new TrainingData(
            new SupervisedTrainingData(SupervisedTrainingSamples.FromArrays(new[] {new[] {0d}, new[] {1d}, new[] {2d}, new[] {3d}},
                new[] {new[] {0d}, new[] {1d}, new[] {2d}, new[] {3d}})),
            new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                new[] {new VariableName("x"), new VariableName("y")}), TrainingDataSource.Memory, NormalizationMethod.None);


        public static TrainingData ValidData3 = new TrainingData(
            new SupervisedTrainingData(SupervisedTrainingSamples.FromArrays(new[] {new[] {0d}, new[] {0d}},
                new[] {new[] {0d}, new[] {0d}}))
            {
                TestSet = SupervisedTrainingSamples.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d}}),
                ValidationSet = SupervisedTrainingSamples.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d}}),
            },
            new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                new[] {new VariableName("x"), new VariableName("y")}), TrainingDataSource.Memory, NormalizationMethod.None);


        public static TrainingData ValidData4 = new TrainingData(
            new SupervisedTrainingData(SupervisedTrainingSamples.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d, 1d}})),
            new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1, 2}),
                new[] {new VariableName("x"), new VariableName("y"), new VariableName("z"),}),
            TrainingDataSource.Memory, NormalizationMethod.None);

        public static TrainingData ValidFileData4 = new TrainingData(
            new SupervisedTrainingData(SupervisedTrainingSamples.FromArrays(new[] { new[] { 0d } }, new[] { new[] { 0d, 1d } })),
            new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1, 2 }),
                new[] { new VariableName("x"), new VariableName("y"), new VariableName("z"), }),
            TrainingDataSource.Memory, NormalizationMethod.None);


        public static SupervisedTrainingSamples AndGateSet()
        {
            var input = new[]
            {
                new[] {0d, 0d},
                new[] {0d, 1d},
                new[] {1d, 0d},
                new[] {1d, 1d},
            };

            var expected = new[]
            {
                new[] {0d},
                new[] {0d},
                new[] {0d},
                new[] {1d},
            };

            return SupervisedTrainingSamples.FromArrays(input, expected);
        }

        public static SupervisedTrainingSamples RandomSamples(int inputSz = 2, int targetSz = 1)
        {
            var rnd = new Random();
            var total = rnd.Next(4000);
            return SupervisedTrainingSamples.FromArrays(
                Enumerable.Repeat(Enumerable.Range(0, inputSz).Select(_ => rnd.NextDouble()).ToArray(), total).ToArray(),
                Enumerable.Repeat(Enumerable.Range(0, targetSz).Select(_ => rnd.NextDouble()).ToArray(), total).ToArray()
            );
        }

        public static SupervisedTrainingData RandomTrainingData(int inputSz = 2, int targetSz = 1)
        {
            return new SupervisedTrainingData(RandomSamples(inputSz,targetSz));
        }

        public static TrainingData AndGateTrainingData()
        {
            var set = AndGateSet();
            var data = new TrainingData(new SupervisedTrainingData(set),
                new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }),
                    new VariableName[] { "x", "y" }), TrainingDataSource.Memory, NormalizationMethod.None);
            return data;
        }
    }
}