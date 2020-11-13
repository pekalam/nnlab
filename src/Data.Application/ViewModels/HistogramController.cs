using Common.Domain;
using NNLib.Data;
using OxyPlot;
using OxyPlot.Series;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Data.Application.ViewModels
{
    public class HistogramController
    {
        private readonly HistogramViewModel _vm;
        private readonly AppState _appState;

        public HistogramController(HistogramViewModel vm, AppState appState)
        {
            _vm = vm;
            _appState = appState;
        }

        public void UpdateBinWidth(DataSetType dataSetType)
        {
            var (varInd, vectorSet) = GetVarIndAndVectorSet(_vm.SelectedVariable!, dataSetType);

            var items = CollectHistogramItems(vectorSet, varInd);
            ((HistogramSeries)_vm.HistogramModel.Model.Series[0]).ItemsSource = items;
            _vm.HistogramModel.Model.InvalidatePlot(true);
        }

        private IList<HistogramItem> CollectHistogramItems(IVectorSet vectorSet, int varIndex)
        {
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
                StrokeThickness = 1,
                StrokeColor = OxyColors.Black,
                EdgeRenderingMode = EdgeRenderingMode.PreferGeometricAccuracy,
            };

            var items = CollectHistogramItems(vectorSet, varIndex);

            hs.ItemsSource = items;

            _vm.HistogramModel.Model.Series.Add(hs);
            _vm.HistogramModel.Model.InvalidatePlot(true);
        }

        private string GetOrgColumnName(string columnName) => columnName.Replace('_', '.');

        public void PlotColumnDataOnHistogram(string columnName, DataSetType dataSetType)
        {
            var (varInd, vectorSet) = GetVarIndAndVectorSet(columnName, dataSetType);

            LoadHistogram(vectorSet, columnName, varInd);
        }

        private (int ind, IVectorSet vectorSet) GetVarIndAndVectorSet(string columnName, DataSetType dataSetType)
        {
            Debug.Assert(_appState.ActiveSession?.TrainingData != null);

            columnName = GetOrgColumnName(columnName);

            var trainingData = _appState.ActiveSession.TrainingData;

            for (int i = 0; i < trainingData.Variables.Length; i++)
            {
                if (trainingData.Variables.Names[i] == columnName)
                {
                    IVectorSet? vectorSet = null;
                    var varInd = -1;
                    if ((varInd = trainingData.Variables.Indexes.InputVarIndexes.IndexOf(i)) != -1)
                    {
                        vectorSet = trainingData.GetSet(dataSetType)!.Input;
                    }
                    else if ((varInd = trainingData.Variables.Indexes.TargetVarIndexes.IndexOf(i)) != -1)
                    {
                        vectorSet = trainingData.GetSet(dataSetType)!.Target;
                    }
                    else
                    {
                        throw new Exception($"Cannot find variable index {i}");
                    }

                    return (varInd, vectorSet);
                }
            }

            throw new Exception($"Cannot find variable with name {columnName}");
        }
    }
}