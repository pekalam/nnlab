using NNLib.Data;
using NNLib.MLP;
using Prism.Mvvm;
using System;
using System.Diagnostics;

namespace Common.Domain
{
    public class Session : BindableBase
    {
        private TrainingData? _trainingData;
        private MLPNetwork? _network;
        private MLPNetwork? _initialNetwork;
        private TrainingParameters? _trainingParameters;
        private string? _singleDataFile;
        private string? _trainingDataFile;
        private string? _validationDataFile;
        private string? _testDataFile;
        private string _name = null!;

        public event Action<MLPNetwork>? NetworkStructureChanged;
        public event Action<MLPNetwork>? NetworkParametersChanged;
        public event Action<TrainingData>? TrainingDataUpdated;

        public Session(string name)
        {
            Name = name;
        }

        private Session(string name, TrainingData? trainingData, MLPNetwork? network,
            TrainingParameters? trainingParameters, string? singleDataFile, string? trainingDataFile,
            string? validationDataFile, string? testDataFile)
        {
            Name = name;
            _trainingData = trainingData;
            _network = network;
            _trainingParameters = trainingParameters;
            _singleDataFile = singleDataFile;
            _trainingDataFile = trainingDataFile;
            _validationDataFile = validationDataFile;
            _testDataFile = testDataFile;
            _initialNetwork = network?.Clone();

            if (_trainingParameters == null && trainingData != null)
            {
                _trainingParameters = new TrainingParameters(trainingData?.GetSet(DataSetType.Validation) != null);
            }
        }

        internal Session CloneWithName(string name, DuplicateOptions opt)
        {
            return new Session(name,
                !opt.HasFlag(DuplicateOptions.NoData) ? TrainingData?.Clone() : null,
                !opt.HasFlag(DuplicateOptions.NoNetwork) ? Network?.Clone() : null,
                !opt.HasFlag(DuplicateOptions.NoTrainingParams) ? TrainingParameters?.Clone() : null,
                SingleDataFile, TrainingDataFile, ValidationDataFile, TestDataFile
            );
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Name cannot be null or contain whitespace characters");
                }

                SetProperty(ref _name, value);
            }
        }

        public string? SingleDataFile
        {
            get => _singleDataFile;
            set => SetProperty(ref _singleDataFile, value);
        }

        public string? TrainingDataFile
        {
            get => _trainingDataFile;
            set => SetProperty(ref _trainingDataFile, value);
        }

        public string? ValidationDataFile
        {
            get => _validationDataFile;
            set => SetProperty(ref _validationDataFile, value);
        }

        public string? TestDataFile
        {
            get => _testDataFile;
            set => SetProperty(ref _testDataFile, value);
        }

        public TrainingData? TrainingData
        {
            get => _trainingData;
            set
            {
                SetProperty(ref _trainingData, value);
                if (value == null || _network == null) return;

                if (TrainingParameters == null)
                    TrainingParameters = new TrainingParameters(TrainingData?.GetSet(DataSetType.Validation) != null);
            }
        }

        public MLPNetwork? Network
        {
            get => _network;
            set
            {
                SetProperty(ref _network, value);

                if (value == null || _trainingData == null) return;

                _initialNetwork = value.Clone();

                if (TrainingParameters == null)
                    TrainingParameters = new TrainingParameters(TrainingData?.GetSet(DataSetType.Validation) != null);
            }
        }

        public void RaiseNetworkStructureChanged()
        {
            Debug.Assert(Network != null);
            _initialNetwork = Network.Clone();
            NetworkStructureChanged?.Invoke(Network);
        }

        public void RaiseNetworkParametersChanged()
        {
            Debug.Assert(Network != null);
            _initialNetwork = Network.Clone();
            NetworkParametersChanged?.Invoke(Network);
        }

        public void ResetNetworkToInitialAndClearReports()
        {
            Network = _initialNetwork;
            TrainingReports.Reset();
        }

        public void RaiseTrainingDataUpdated()
        {
            TrainingDataUpdated?.Invoke(_trainingData!);
            if (TrainingParameters != null)
            {
                TrainingParameters.CanRunValidation = TrainingData?.GetSet(DataSetType.Validation) != null;
                TrainingParameters.RunValidation =
                    TrainingParameters.CanRunValidation && TrainingParameters.RunValidation;
            }
        }

        public TrainingReportsCollection TrainingReports { get; private set; } = new TrainingReportsCollection();

        public TrainingParameters? TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }
    }
}