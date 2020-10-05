using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Framework;
using Moq;
using Moq.AutoMock;
using Prism.Events;
using Prism.Regions;

namespace TestUtils
{
    public static class RegionManagerTestUtils
    {
        public static void SetupEvents(Mock<IRegionManager> rm, Dictionary<string, Mock<IRegion>> regions)
        {
            var packages = new[] {"Shell", "Data", "NeuralNetwork", "Training"};
            var files = Directory.GetFiles(".");

            var assemblies = new List<Assembly>();
            foreach (var package in packages)
            {
                foreach (var file in files.Where(f => f.Contains(package) && f.EndsWith("dll")))
                {
                    assemblies.Add(Assembly.Load(file.Replace(".dll", "").Replace(".\\", "")));
                }
            }

            _ = assemblies.SelectMany(v => v.GetTypes())
                .Where(t => t.Name.Contains("Regions"))
                .SelectMany(t => t.GetMembers())
                .Select(m =>
                {
                    if (m.MemberType == MemberTypes.Field && ((FieldInfo)(m)).IsStatic)
                    {
                        var reg = new Mock<IRegion>();
                        regions.Add((string)(m as FieldInfo)!.GetValue(null)!, reg);
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

    }

    public class TestViewModelAccessor : IViewModelAccessor
    {
        private readonly Dictionary<Type,object> _vms = new Dictionary<Type, object>();
        private readonly Dictionary<Type,Action> _onCreated = new Dictionary<Type, Action>();

        public TestViewModelAccessor()
        {
            
        }

        public virtual T Get<T>() where T : ViewModelBase<T>
        {
            _vms.TryGetValue(typeof(T), out var vm);
            return (vm as T)!;
        }

        public void OnCreated<T>(Action action) where T : ViewModelBase<T>
        {
            _onCreated[typeof(T)] = action;
        }

        public virtual void Register<T>(T vm) where T : ViewModelBase<T>
        {
            _vms[typeof(T)] = vm; 
            _onCreated.TryGetValue(typeof(T), out var action);
            action?.Invoke();
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
            mocker.Use<TImlp>(impl);

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

        public static void UseTestVmAccessor(this AutoMocker mocker)
        {
            var accessor = new TestViewModelAccessor();
            mocker.Use<IViewModelAccessor>(accessor);
            mocker.Use(accessor);
        }

        public static T UseVm<T>(this AutoMocker mocker) where T : ViewModelBase<T>
        {
            var accessor = mocker.Get<TestViewModelAccessor>();
            var vm = mocker.UseImpl<T>();
            accessor.Register(vm);

            return vm;
        }
    }
}