using System;
using System.Collections.Generic;
using System.Text;

namespace Shell.Application
{
    class ModuleController
    {
        public void Run() { }
    }

    public interface IContentRegionHistoryService
    {
        void SaveContentForModule(int moduleNavId);
        void TryRestoreContentForModule(int moduleNavId);
    }
}
