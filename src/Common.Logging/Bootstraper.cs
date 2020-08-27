using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Logging
{
    public static class Bootstraper
    {
        public static void Configure()
        {
            SerilogLoggingConfiguration.Configure();
        }
    }
}
