﻿using System;
using Castle.DynamicProxy;
using Infrastructure;
using Infrastructure.Domain;
using Infrastructure.Messaging;
using Moq;
using NNLib.Common;
using Prism.Regions;

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
    }
}