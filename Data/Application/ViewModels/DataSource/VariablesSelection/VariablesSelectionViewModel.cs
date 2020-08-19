﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Data.Domain;
using Infrastructure;

namespace Data.Application.ViewModels.DataSource.VariablesSelection
{
    public class VariablesSelectionViewModel : ViewModelBase<VariablesSelectionViewModel>
    {
        private VariableTableModel[] _variables;
        private string _error;


        public VariableTableModel[] Variables
        {
            get => _variables;
            set
            {
                SetProperty(ref _variables, value);
                foreach (var model in value)
                {
                    model.OnError = error => Error = error;
                }
            }
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }
    }
}