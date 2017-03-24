using System;
using System.Linq;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.Entities
{
    public static class DbManager
    {
        private static MemoryDataBase Db => MemoryDataBase.Instance;

        public static int GetFailedAuthorizeAttempts(DateTime lookFromDate)
        {
            return Db.AttemptsStorage.Count(a => a.TimeOccured >= lookFromDate);
        }

        public static ILock GetLastLocker(DateTime lookFromDate)
        {
            ILock result = null;
            if (Db.UnlocksStorage.Any(l => l.TimeOccurred >= lookFromDate))
                result = Db.LocksStorage.OrderByDescending(u => u.TimeOccurred).First();
            return result;
        }

        public static IUnlock GetLastUnlocker()
        {
            IUnlock result = null;
            if (Db.UnlocksStorage.Any())
                result = Db.UnlocksStorage.OrderByDescending(u => u.TimeOccurred).First();
            return result;
        }

        public static void AddFailedAuthorizeAttempts(string binData)
        {
            Db.AttemptsStorage.Add(new FailedAttempt {BinData = binData, TimeOccured = DateTime.Now});
        }
        public static void AddLock(int number)
        {
            Db.LocksStorage.Add(new ServiceMenuLocker(number, DateTime.Now));
        }
        public static void AddUnlcok(int duration)
        {
            Db.UnlocksStorage.Add(new ServiceMenuUnlocker(DateTime.Now, duration));
        }
    }
}