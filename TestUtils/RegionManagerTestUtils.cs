using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using Moq.AutoMock;
using Prism.Regions;

namespace TestUtils
{
    public static class RegionManagerTestUtils
    {
        public static (Mock<IRegionManager> regionManager, Dictionary<string, Mock<IRegion>> regions)
            CreateTestRegionManager()
        {
            var rm = new Mock<IRegionManager>();
            var regions = new Dictionary<string, Mock<IRegion>>();

            _ = Assembly.Load("Infrastructure").GetTypes()
                .Union(Assembly.Load("Data").GetTypes())
                .Where(t => t.Name.Contains("Regions"))
                .SelectMany(t => t.GetMembers())
                .Select(m =>
                {
                    if (m.MemberType == MemberTypes.Field && ((FieldInfo)(m)).IsStatic)
                    {
                        var reg = new Mock<IRegion>();
                        regions.Add((m as FieldInfo).GetValue(null) as string, reg);
                        rm.SetupGet(p => p.Regions[m.Name]).Returns(reg.Object);
                    }

                    return m;
                }).ToList();


            return (rm, regions);
        }


    }

    public static class AutoMockerExtensions
    {
        public static Mock<T> UseInMocker<T>(this AutoMocker mocker) where T : class
        {
            var mock = mocker.GetMock<T>();
            mocker.Use(mock.Object);
            return mock;
        }

        public static Mock<TImlp> UseInMocker<TI, TImlp>(this AutoMocker mocker) where TI : class where TImlp : class, TI
        {
            var mock = mocker.GetMock<TImlp>();
            mocker.Use(mock.Object);
            mocker.Use<TI>(mock.Object);

            return mock;
        }

        public static (Mock<IRegionManager> rm, Dictionary<string, Mock<IRegion>> regions) UseTestRm(
            this AutoMocker mocker)
        {
            var (rm, regions) = RegionManagerTestUtils.CreateTestRegionManager();

            mocker.Use(rm.Object);

            return (rm, regions);
        }
    }
}