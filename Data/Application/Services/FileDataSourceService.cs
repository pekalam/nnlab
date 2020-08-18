using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Application.Services
{
    public interface IFileDataSourceService
    {
        Action Initialized { get; set; }
    }

    internal class FileDataSourceService : IFileDataSourceService
    {
        public Action Initialized { get; set; }
    }
}
