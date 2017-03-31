using System;

namespace AuthorizeLocker.Interfaces
{
    public interface IDBAuthorizeManager
    {
        int GetFailedAttempts(DateTime lookFrom);
        IUnlock GetLastUnlocker();
        ILock GetLastLocker();
        void SaveAttempt(string binData);
        void CreateLock(int number);
        void CreateUnlock(int duration);
    }
}