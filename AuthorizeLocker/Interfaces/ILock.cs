using System;

namespace AuthorizeLocker.Interfaces
{
    public interface ILock : IAuthorizeEvent
    {
        int LockNumber { get; }
        DateTime TimeLockedTo { get; }
    }
}