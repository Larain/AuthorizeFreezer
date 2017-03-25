using System.Collections.Generic;
using AuthorizeLocker.Entities;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.DBLayer
{
    public class MemoryDataBase
    {
        private static MemoryDataBase _instance;
        private static readonly object Locker = new object();

        private MemoryDataBase()
        {
            UsersStorage = new List<User>();
            LocksStorage = new List<ILock>();
            UnlocksStorage = new List<IUnlock>();
            AttemptsStorage = new List<FailedAttempt>();

            GenerateUsers();
        }

        public static MemoryDataBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                            _instance = new MemoryDataBase();
                    }
                }

                return _instance;
            }
        }

        public List<User> UsersStorage { get; }
        public List<ILock> LocksStorage { get; }
        public List<IUnlock> UnlocksStorage { get; }
        public List<FailedAttempt> AttemptsStorage { get; }

        private void GenerateUsers()
        {
            UsersStorage.Add(new User {ID = 1, Login = "Admin", Password = "Secret"});
            UsersStorage.Add(new User {ID = 1, Login = "Lara1n", Password = "2324"});
            UsersStorage.Add(new User {ID = 1, Login = "User", Password = "pass"});
        }
    }
}