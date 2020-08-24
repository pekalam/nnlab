using System;
using Castle.DynamicProxy;
using Common.Domain;
using Moq;
using NNLib;
using NNLib.Common;
using Prism.Regions;
using Shell.Interface;

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
            rm.Verify(f => f.RequestNavigate(AppRegions.ContentRegion, viewName, It.IsAny<ContentRegionNavigationParameters>()), times);
        }
    }


    public static class MLPMocks
    {
        public static MLPNetwork ValidNet1 = new MLPNetwork(new PerceptronLayer(1, 10, new SigmoidActivationFunction()),
            new PerceptronLayer(10, 1, new ArcTanActivationFunction()));

        public static MLPNetwork ValidNet2 = new MLPNetwork(new PerceptronLayer(1, 10, new SigmoidActivationFunction()),
            new PerceptronLayer(10, 1, new ArcTanActivationFunction()),
                new PerceptronLayer(1, 2, new ArcTanActivationFunction()));
    }

    public static class TrainingDataMocks
    {
        public static TrainingData ValidData1 = new TrainingData(
            new SupervisedTrainingSets(SupervisedSet.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d}})),
            new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                new[] {new VariableName("x"), new VariableName("y")}), TrainingDataSource.Memory);


        public static TrainingData ValidData2 = new TrainingData(
            new SupervisedTrainingSets(SupervisedSet.FromArrays(new[] {new[] {0d}, new[] {1d}, new[] {2d}, new[] {3d}},
                new[] {new[] {0d}, new[] {1d}, new[] {2d}, new[] {3d}})),
            new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                new[] {new VariableName("x"), new VariableName("y")}), TrainingDataSource.Memory);


        public static TrainingData ValidData3 = new TrainingData(
            new SupervisedTrainingSets(SupervisedSet.FromArrays(new[] {new[] {0d}, new[] {0d}},
                new[] {new[] {0d}, new[] {0d}}))
            {
                TestSet = SupervisedSet.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d}}),
                ValidationSet = SupervisedSet.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d}}),
            },
            new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                new[] {new VariableName("x"), new VariableName("y")}), TrainingDataSource.Memory);


        public static TrainingData ValidData4 = new TrainingData(
            new SupervisedTrainingSets(SupervisedSet.FromArrays(new[] {new[] {0d}}, new[] {new[] {0d, 1d}})),
            new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1, 2}),
                new[] {new VariableName("x"), new VariableName("y"), new VariableName("z"),}),
            TrainingDataSource.Memory);


        public static SupervisedSet AndGateSet()
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

            return SupervisedSet.FromArrays(input, expected);
        }

        public static TrainingData AndGateTrainingData()
        {
            var set = AndGateSet();
            var data = new TrainingData(new SupervisedTrainingSets(set),
                new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }),
                    new VariableName[] { "x", "y" }), TrainingDataSource.Memory);
            return data;
        }
    }
}