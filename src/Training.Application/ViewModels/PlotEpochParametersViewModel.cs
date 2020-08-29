﻿using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Prism.Regions;

namespace Training.Application.ViewModels
{
    internal class PlotEpochParametersNavParams : NavigationParameters
    {
        public PlotEpochParametersNavParams(PlotEpochEndConsumer epochEndConsumer)
        {
            Add(nameof(RecNavParams.PlotEpochEndConsumer), epochEndConsumer);
        }

        public class RecNavParams
        {
            private readonly NavigationParameters _parameters;

            public RecNavParams(NavigationParameters parameters)
            {
                _parameters = parameters;
            }

            public PlotEpochEndConsumer PlotEpochEndConsumer => _parameters[nameof(PlotEpochEndConsumer)] as PlotEpochEndConsumer;
        }
    }

    public class PlotEpochParametersViewModel : ViewModelBase<PlotEpochParametersViewModel>
    {
        private int _epochDelay = PlotEpochEndConsumer.BUFFERING_BUFFER_SIZE;
        private bool _onlineMode;
        private bool _bufferingMode;

        private PlotEpochEndConsumer _epochEndConsumer;

        public int EpochDelay
        {
            get => _epochDelay;
            set
            {
                SetProperty(ref _epochDelay, value);
                _epochEndConsumer.BufferSize = value;
            }
        }

        public bool OnlineMode
        {
            get => _onlineMode;
            set
            {
                SetProperty(ref _onlineMode, value);
                if (value && _epochEndConsumer.ConsumerType != PlotEpochEndConsumerType.Online)
                {
                    _epochEndConsumer.ConsumerType = PlotEpochEndConsumerType.Online;
                }
                else if(value && _epochEndConsumer.ConsumerType != PlotEpochEndConsumerType.Buffering)
                {
                    _epochEndConsumer.ConsumerType = PlotEpochEndConsumerType.Buffering;
                    _epochEndConsumer.BufferSize = EpochDelay;
                }
            }
        }

        public bool BufferingMode
        {
            get => _bufferingMode;
            set => SetProperty(ref _bufferingMode, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters =
                new PlotEpochParametersNavParams.RecNavParams(navigationContext.Parameters);

            _epochEndConsumer = parameters.PlotEpochEndConsumer;
            OnlineMode = _epochEndConsumer.ConsumerType == PlotEpochEndConsumerType.Online;
            BufferingMode = _epochEndConsumer.ConsumerType == PlotEpochEndConsumerType.Buffering;
        }
    }
}