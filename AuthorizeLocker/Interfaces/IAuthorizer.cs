using System;

namespace AuthorizeLocker.Interfaces
{
    public interface IAuthorizer
    {
        event EventHandler LockStarted;
        event EventHandler LockReleased;

        bool IsBlocked { get; }
        bool IsUnlocked { get; }
        bool Login(Func<bool> action);
        DateTime BlockedTo { get; }
        int FailedAttempts { get; }

    }
}