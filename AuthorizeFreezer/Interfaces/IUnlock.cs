using System;

namespace AuthorizeLocker.Interfaces {
    public interface IUnlock {
        bool IsActice { get; }
        int DurationInMinutes { get; }
        DateTime TimeUnlockedTo { get; }
        DateTime TimeOccurred { get; }
    }
}