using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infrastructure.PrismDecorators;
using Moq;
using Moq.AutoMock;
using Prism.Events;
using Prism.Regions;

namespace TestUtils
{
    public static class RegionManagerTestUtils
    {
        private static void SetupEvents(Mock<IRegionManager> rm, Dictionary<string, Mock<IRegion>> regions)
        {
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
        }

        public static (Mock<IRegionManager> regionManager, Dictionary<string, Mock<IRegion>> regions)
            CreateTestRegionManager()
        {
            var rm = new Mock<IRegionManager>();
            var regions = new Dictionary<string, Mock<IRegion>>();

            SetupEvents(rm, regions);

            return (rm, regions);
        }

        public static (Mock<RegionManagerNavigationDecorator> regionManager, Dictionary<string, Mock<IRegion>> regions, TestEa ea)
            CreateDecoratedTestRegionManager(AutoMocker mocker)
        {
            var ea = mocker.UseTestEa();
            var rm = new Mock<IRegionManager>();
            var regions = new Dictionary<string, Mock<IRegion>>();

            SetupEvents(rm, regions);

            var decrm = new Mock<RegionManagerNavigationDecorator>(() => new RegionManagerNavigationDecorator(rm.Object, ea));

            return (decrm, regions, ea);
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

        public static TestEa UseTestEa(this AutoMocker mocker)
        {
            var ea = new TestEa(new EventAggregator());
            mocker.Use<IEventAggregator>(ea);
            return ea;
        }

        public static (Mock<IRegionManager> rm, Dictionary<string, Mock<IRegion>> regions) UseTestRm(
            this AutoMocker mocker)
        {
            var (rm, regions) = RegionManagerTestUtils.CreateTestRegionManager();

            mocker.Use(rm.Object);

            return (rm, regions);
        }

        public static (Mock<RegionManagerNavigationDecorator> rm, Dictionary<string, Mock<IRegion>> regions, TestEa ea) UseDecoratedTestRm(this AutoMocker mocker)
        {
            var (rm, regions, ea) = RegionManagerTestUtils.CreateDecoratedTestRegionManager(mocker);

            mocker.Use(rm.Object);

            return (rm, regions, ea);
        }
    }
}