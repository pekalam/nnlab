using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Data.Application.Services;
using Data.Application.Views;
using Infrastructure;
using Prism.Commands;
using Prism.Mvvm;

namespace Data.Application.ViewModels
{
    public class SelectDataSourceViewModel : ViewModelBase<SelectDataSourceViewModel>
    {
        public SelectDataSourceViewModel(IFileService fileService)
        {
            FileService = fileService;

        }

        public IFileService FileService { get; set; }
    }
}
