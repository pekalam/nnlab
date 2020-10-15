using NNLib.Common;
using OxyPlot;
using OxyPlot.Series;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Common.Domain;
using NNLib.Data;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class HistogramController
    {
        private readonly HistogramViewModel _vm;
        private int _varIndex;
        private IVectorSet? _vectorSet;
        private AppStateHelper _helper;
        private readonly AppState _appState;

        public HistogramController(HistogramViewModel vm, AppState appState)
        {
            _vm = vm;
            _appState = appState;
            _helper = new AppStateHelper(appState);
            vm.PropertyChanged += VmOnPropertyChanged;

            _helper.OnTrainingDataChanged(data =>
                {
                    _vm.Variables = data.Variables.InputVariableNames.Union(data.Variables.TargetVariableNames).ToArray();
                    _vm.SelectedVariable = _vm.Variables[0];
                });

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                _vm.Variables = data.Variables.InputVariableNames.Union(data.Variables.TargetVariableNames).ToArray();
                _vm.SelectedVariable = _vm.Variables[0];
            }, s => s switch
            {
                nameof(TrainingData.Variables) => true,
                _ => false,
            });
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(HistogramViewModel.BinWidth):
                    UpdateBinWidth();
                    break;
                case nameof(HistogramViewModel.SelectedVariable):
                    if(_vm.SelectedVariable == null) return;
                    PlotColumnDataOnHistogram(_vm.SelectedVariable);
                    break;
            }
        }

        public void UpdateBinWidth()
        {
            if (_vectorSet != null)
            {
                var items = CollectHistogramItems(_vectorSet, _varIndex);
                ((HistogramSeries)_vm.HistogramModel.Model.Series[0]).ItemsSource = items;
                _vm.HistogramModel.Model.InvalidatePlot(true);
            }
        }

        private IList<HistogramItem> CollectHistogramItems(IVectorSet vectorSet, int varIndex)
        {
            _varIndex = varIndex;
            _vectorSet = vectorSet;

            var vectorSetValues = Enumerable.Range(0, vectorSet.Count).Select(i => vectorSet[i][varIndex, 0]).ToList();

            var max = vectorSetValues.Max();
            var min = vectorSetValues.Min();

            var binCount = (int)((max - min) / _vm.BinWidth);

            if (binCount == 0)
            {
                binCount = 1;
            }
            var bins = HistogramHelpers.CreateUniformBins(min, max, binCount);
            var items = HistogramHelpers.Collect(vectorSetValues, bins,
                new BinningOptions(BinningOutlierMode.CountOutliers, BinningIntervalType.InclusiveLowerBound,
                    BinningExtremeValueMode.IncludeExtremeValues));
            foreach (var item in items)
            {
                item.Area = item.Width * item.Count;
            }
            return items;
        }

        private void LoadHistogram(IVectorSet vectorSet, string columnName, int varIndex)
        {
            _vm.HistogramModel.Model.Series.Clear();

            _vectorSet = vectorSet;
            _varIndex = varIndex;

            if (vectorSet.Count > 100_000)
            {
                Log.Logger.Debug("Ignoring loading histogram for more than 100_000 items (actual count: {count})", vectorSet.Count);
                return;
            }

            _vm.HistogramModel.Model.Axes[1].Title = columnName.ToLower();

            columnName = char.ToUpper(columnName[0]) + columnName.Substring(1);
            _vm.HistogramModel.Model.Title = $"\"{columnName}\" variable histogram";

            var hs = new HistogramSeries()
            {
                Title = $"{columnName}",
            };

            var items = CollectHistogramItems(vectorSet, varIndex);

            hs.ItemsSource = items;

            _vm.HistogramModel.Model.Series.Add(hs);
            _vm.HistogramModel.Model.InvalidatePlot(true);
        }

        public void PlotColumnDataOnHistogram(string columnName)
        {
            Debug.Assert(_appState.ActiveSession?.TrainingData != null);

            var trainingData = _appState.ActiveSession.TrainingData;
            columnName = columnName.Replace('_', '.');
            for (int i = 0; i < trainingData.Variables.Length; i++)
            {
                if (trainingData.Variables.Names[i] == columnName)
                {
                    IVectorSet? vectorSet = null;
                    var varInd = -1;
                    if ((varInd = trainingData.Variables.Indexes.InputVarIndexes.IndexOf(i)) != -1)
                    {
                        vectorSet = trainingData.Sets.TrainingSet.Input;
                    }
                    else if ((varInd = trainingData.Variables.Indexes.TargetVarIndexes.IndexOf(i)) != -1)
                    {
                        vectorSet = trainingData.Sets.TrainingSet.Target;
                    }
                    else
                    {
                        throw new Exception($"Cannot find variable index {i}");
                    }

                    LoadHistogram(vectorSet, columnName, varInd);
                    return;
                }
            }
            throw new Exception($"Cannot find column {columnName}");
        }
    }
}