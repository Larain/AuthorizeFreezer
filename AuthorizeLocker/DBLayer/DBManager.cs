﻿using System;
using System.Linq;
using AuthorizeLocker.Authorizer.ServiceMenu;
using AuthorizeLocker.Entities;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.DBLayer
{
    public class DbManager : IDBAuthorizeManager
    {
        private MemoryDataBase Db => MemoryDataBase.Instance;

        public int GetFailedAttempts(DateTime lookFromDate)
        {
            return Db.AttemptsStorage.Count(a => a.TimeOccured > lookFromDate);
        }

        public ILock GetLastLocker()
        {
            ILock result = null;
            if (Db.LocksStorage.Any())
                result = Db.LocksStorage.OrderByDescending(u => u.TimeOccurred).First();
            return result;
        }

        public IUnlock GetLastUnlocker()
        {
            IUnlock result = null;
            if (Db.UnlocksStorage.Any())
                result = Db.UnlocksStorage.OrderByDescending(u => u.TimeOccurred).First();
            return result;
        }

        public void SaveAttempt(string binData = "")
        {
            Db.AttemptsStorage.Add(new FailedAttempt {BinData = binData, TimeOccured = DateTime.Now});
        }
        public void CreateLock(int number)
        {
            Db.LocksStorage.Add(new ServiceMenuLocker(DateTime.Now, number));
        }
        public void CreateUnlock(int duration)
        {
            Db.UnlocksStorage.Add(new ServiceMenuUnlocker(DateTime.Now, duration));
        }

    }
}