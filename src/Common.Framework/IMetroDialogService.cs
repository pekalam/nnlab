using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Framework
{
    public interface IMetroDialogService
    {
        bool ShowModalConfirmationDialog(string title,string message);
    }
}
