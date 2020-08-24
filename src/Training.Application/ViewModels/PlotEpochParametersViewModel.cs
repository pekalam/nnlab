using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;

namespace Training.Application.ViewModels
{
    public class PlotEpochParametersViewModel : ViewModelBase<PlotEpochParametersViewModel>
    {
        private int _epochDelay;
        private bool _onlineMode;
        private bool _bufferingMode;

        public int EpochDelay
        {
            get => _epochDelay;
            set => SetProperty(ref _epochDelay, value);
        }

        public bool OnlineMode
        {
            get => _onlineMode;
            set => SetProperty(ref _onlineMode, value);
        }

        public bool BufferingMode
        {
            get => _bufferingMode;
            set => SetProperty(ref _bufferingMode, value);
        }
    }
}
