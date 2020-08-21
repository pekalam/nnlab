using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Windows.Controls;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ArchTests
{
    public class ModuleTests
    {
        private ITestOutputHelper _output;
        private string[] _packages;
        private static string[] DefaultModules = new[] {".Domain", ".Application", ".Presentation"};

        public ModuleTests(ITestOutputHelper output)
        {
            _output = output;
            _packages = new[] { "Shell", "Data", "NeuralNetwork", "Training" };
        }

        private IEnumerable<Assembly> GetAssembliesFromPackage(string package, string[] modules = null)
        {
            var assemblies = new List<Assembly>();
            var files = Directory.EnumerateFiles(".").ToList();


            foreach (var str in (modules ?? DefaultModules))
            {
                var name = $"{package}{str}";
                if (files.Any(v => v.Contains(name)))
                {
                    assemblies.Add(Assembly.Load(name));
                }
            }

            return assemblies;
        }

        private void AssertArchTest(TestResult result, ITestOutputHelper output)
        {
            if (!result.IsSuccessful)
            {
                _output.WriteLine("Failing type names:");
                _output.WriteLine(result.FailingTypeNames.Aggregate(((s, s1) => s + "\n" + s1)));
            }
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void Modules_does_not_have_dependency_to_other_modules()
        {
            foreach (var package in _packages)
            {
                foreach (var other in _packages.Where(v => v != package))
                {
                    foreach (var sub in new []{".Domain", ".Application", ".Presentation"})
                    {
                        var result = Types.InAssemblies(GetAssembliesFromPackage(package))
                            .That()
                            .ResideInNamespace(package)
                            .Should()
                            .NotHaveDependencyOn(other + sub)
                            .GetResult();

                        AssertArchTest(result, _output);
                    }
                    
                }
            }
        }


        [Fact]
        public void Modules_have_module_controller_in_application_namespace()
        {
            foreach (var package in _packages)
            {
                var name = $"{package}.Application";
                var result = Types.InAssembly(Assembly.Load(name))
                    .That()
                    .ResideInNamespace(package)
                    .And()
                    .AreClasses()
                    .And()
                    .AreNotPublic()
                    .And()
                    .HaveName("ModuleController")
                    .GetTypes();

                if (result.Count() != 1)
                {
                    _output.WriteLine("Cannot find ModuleController for " + name);
                    Assert.False(true);
                }


            }
        }

        [Fact]
        public void Modules_have_bootstraper_class()
        {
            foreach (var package in _packages)
            {
                var assemblies = GetAssembliesFromPackage(package).ToList();
                var result = Types.InAssemblies(assemblies)
                    .That()
                    .ResideInNamespace(package)
                    .And()
                    .AreClasses()
                    .And()
                    .ArePublic()
                    .And()
                    .HaveName("Bootstraper")
                    .GetTypes();

                if (result.Count() != assemblies.Count())
                {
                    _output.WriteLine("Cannot find Bootstraper for " + package);
                    Assert.False(true);
                }


            }
        }


        [Fact]
        public void Views_from_presentation_modules_have_vm_in_corresponding_namespace_in_application_module()
        {
            bool fail = false;
            foreach (var package in _packages)
            {
                var views = Types
                    .InAssemblies(GetAssembliesFromPackage(package, new []{".Presentation"}))
                    .That()
                    .ResideInNamespaceContaining("Views")
                    .And()
                    .Inherit(typeof(UserControl))
                    .GetTypes().ToList();


                var vms = Types
                    .InAssemblies(GetAssembliesFromPackage(package, new[] { ".Application" }))
                    .That()
                    .ResideInNamespaceContaining("ViewModels")
                    .GetTypes().ToList();


                foreach (var view in views)
                {
                    if (!vms.Any(vm => vm.FullName == view.FullName.Replace("Views", "ViewModels").Replace("Presentation","Application") + "Model"))
                    {
                        _output.WriteLine($"View {view.Name} in package {package} doesn't contain vm");
                        fail = true;
                    }
                }

            }


            Assert.False(fail);
        }
    }
}
