using System;

namespace AuthorizeLocker.Interfaces {
    public interface IAuthorizer {
        bool IsBlocked { get; }
        bool Login(Func<bool> action);
        DateTime BlockedTo { get; }
        void CreateLock(int number);
        void CreateUnlock(int duration);
    }
}