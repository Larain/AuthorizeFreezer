using System;

namespace Interfaces.Authorizer
{
    public interface ILock : IAuthorizeEvent
    {
        int LockNumber { get; }
        DateTime TimeLockedTo { get; }
    }
}