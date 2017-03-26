using System;

namespace AuthorizeLocker.Interfaces
{
    public interface IAuthorizeDbManager
    {
        int GetFailedAttempts(DateTime lookFrom);
        IUnlock GetLastUnlocker();
        ILock GetLastLocker(DateTime lookFrom);
        void SaveAttempt(string binData);
        void CreateLock(int number);
        void CreateUnlock(int duration);
    }
}