using System;

namespace AuthorizeLocker.Interfaces {
    public interface ILock {
        int LockNumber { get; }
        DateTime TimeOccurred { get; }
        DateTime TimeLockedTo { get; }
        bool IsActive { get; }
    }
}