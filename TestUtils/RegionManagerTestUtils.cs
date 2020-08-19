using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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
        public static Mock<T> UseMock<T>(this AutoMocker mocker) where T : class
        {
            var mock = mocker.GetMock<T>();
            mocker.Use(mock.Object);
            return mock;
        }

        public static Mock<TImlp> UseMock<TI, TImlp>(this AutoMocker mocker) where TI : class where TImlp : class, TI
        {
            var mock = mocker.GetMock<TImlp>();
            mocker.Use(mock.Object);
            mocker.Use<TI>(mock.Object);

            return mock;
        }

        public static TImlp UseImpl<TImlp>(this AutoMocker mocker) where TImlp : class
        {
            var impl = mocker.CreateInstance<TImlp>();
            mocker.Use<TImlp>(impl);

            return impl;
        }

        public static TImlp UseImpl<TI, TImlp>(this AutoMocker mocker) where TI : class where TImlp : class, TI
        {
            var impl = mocker.CreateInstance<TImlp>();
            mocker.Use<TI>(impl);

            return impl;
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