using System;

namespace Common.Domain
{
    [Flags]
    public enum DuplicateOptions
    {
        All = 1,
        NoData = 2,
        NoNetwork = 4,
        NoTrainingParams = 8
    }
}