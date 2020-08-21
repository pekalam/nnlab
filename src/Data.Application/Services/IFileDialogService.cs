using System;
using System.Collections.Generic;
using System.Text;
using Prism.Regions;
using Shell.Interface;

namespace Data.Application
{
    public interface IFileDialogService
    {
        (bool? result, string filePath) OpenCsv();
    }
}
