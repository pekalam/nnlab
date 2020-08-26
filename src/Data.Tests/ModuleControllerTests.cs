using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Domain;
using Data.Application.Controllers.DataSource;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using Shell.Interface;
using TestUtils;
using Xunit;

namespace Data.Application.Tests
{
    public class ModuleControllerTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private Mock<IRegionManager> _rm;
        private TestEa _ea;
        private ModuleController _ctrl;
        private AppState _appState;

        public ModuleControllerTests()
        {
            _ea = _mocker.UseTestEa();
            (_rm, _) = _mocker.UseTestRm();
            _appState = _mocker.UseImpl<AppState>();
            _mocker.UseTestVmAccessor();

            _ctrl = _mocker.CreateInstance<ModuleController>();

            _ctrl.Run();
        }

    }
}
