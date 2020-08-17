using System;
using Infrastructure.Domain;
using Moq;
using NNLib.Common;
using Prism.Regions;

namespace TestUtils
{
    public static class RegionTestExtensions
    {
        public static void VerifyNavigation(this Mock<IRegion> region, string viewName, Times times)
        {
            region.Verify(f => f.RequestNavigate(It.Is<Uri>(uri => uri.ToString().Contains(viewName)), It.IsAny<Action<NavigationResult>>()), times);
        }
    }


    public static class TrainingDataMocks
    {
        public static TrainingData ValidData1 = new TrainingData(
            new SupervisedTrainingSets(SupervisedSet.FromArrays(new[] { new[] { 0d } }, new[] { new[] { 0d } })),
            new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }),
                new[] { new VariableName("x"), new VariableName("y") }), TrainingDataSource.Memory);


        public static TrainingData ValidData2 = new TrainingData(
            new SupervisedTrainingSets(SupervisedSet.FromArrays(new[] { new[] { 0d }, new[] { 1d }, new[] { 2d }, new[] { 3d } }, new[] { new[] { 0d }, new[] { 1d }, new[] { 2d }, new[] { 3d } })),
            new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }),
                new[] { new VariableName("x"), new VariableName("y") }), TrainingDataSource.Memory);
    }
}