﻿using Common.Framework;
using Data.Application.Services;

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
