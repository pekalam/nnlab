﻿using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels;
using FluentAssertions;
using Infrastructure.Domain;
using Moq;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Windows;
using Data.Application.ViewModels.CustomDataSet;
using Infrastructure;
using MahApps.Metro.Controls;
using TestUtils;
using Xunit;
using OxyPlot;

namespace Data.Tests.Application.CustomDataSet
{
    public class CustomDataSetCreationTests
    {
        private AutoMocker _mocker = new AutoMocker();

        private CustomDataSetService _dsService;
        private CustomDataSetController _ctrl;
        private CustomDataSetViewModel _vm;
        private AppState _appState;

        public CustomDataSetCreationTests()
        {
            _mocker.UseTestRm();
            _ctrl = _mocker.UseImpl<ITransientControllerBase<CustomDataSetService>,CustomDataSetController>();
            _dsService = _mocker.UseImpl<ICustomDataSetService, CustomDataSetService>();
            _appState = _mocker.UseMock<AppState>().Object;
            _vm = _mocker.CreateInstance<CustomDataSetViewModel>();
        }


        [Fact]
        public void New_session_is_created_after_viewmodel_is_created()
        {
            _appState.SessionManager.Sessions.Count.Should().Be(1);
            _appState.SessionManager.ActiveSession.Should().NotBeNull();
        }

        [Fact]
        public void MouseDownCommand_updates_session_training_data_when_3_points_are_created()
        {
            //act
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(0,0), ChangedButton = OxyMouseButton.Left,
            });
            //assert
            _appState.SessionManager.ActiveSession.TrainingData.Should().BeNull();

            //act
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(1, 0), ChangedButton = OxyMouseButton.Left,
            });
            //assert
            _appState.SessionManager.ActiveSession.TrainingData.Should().BeNull();


            //act
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(2, 0), ChangedButton = OxyMouseButton.Left,
            });
            //assert
            _appState.SessionManager.ActiveSession.TrainingData.Should().NotBeNull();

            _appState.SessionManager.ActiveSession.TrainingData.Sets.TrainingSet.Input.Count.Should().Be(3);
            _appState.SessionManager.ActiveSession.TrainingData.Sets.TrainingSet.Target.Count.Should().Be(3);
        }
    }
}