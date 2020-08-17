using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure;

namespace Data.Application.ViewModels
{
    public class MultiFileSourceViewModel : ViewModelBase<MultiFileSourceViewModel>
    {
        private string _trainingSetFilePath;
        private string _validationSetFilePath;
        private string _testSetFilePath;

        public string TrainingSetFilePath
        {
            get => _trainingSetFilePath;
            set => SetProperty(ref _trainingSetFilePath, value);
        }

        public string ValidationSetFilePath
        {
            get => _validationSetFilePath;
            set => SetProperty(ref _validationSetFilePath, value);
        }

        public string TestSetFilePath
        {
            get => _testSetFilePath;
            set => SetProperty(ref _testSetFilePath, value);
        }
    }
}
