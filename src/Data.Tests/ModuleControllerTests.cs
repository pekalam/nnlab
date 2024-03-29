﻿using Common.Domain;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using TestUtils;

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

            _ctrl = _mocker.CreateInstance<ModuleController>();

            _ctrl.Run();
        }

    }
}
