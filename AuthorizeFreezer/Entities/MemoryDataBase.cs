using System.Collections.Generic;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.Entities
{
    public class MemoryDataBase
    {
        private static MemoryDataBase _instance;
        private MemoryDataBase()
        {
            UsersStorage = new List<User>();
            LocksStorage = new List<ILock>();
            UnlocksStorage = new List<IUnlock>();
            AttemptsStorage = new List<FailedAttempt>();

            GenerateUsers();
        }

        public static MemoryDataBase Instance => _instance ?? (_instance = new MemoryDataBase());

        public List<User> UsersStorage { get; }
        public List<ILock> LocksStorage { get; }
        public List<IUnlock> UnlocksStorage { get; }
        public List<FailedAttempt> AttemptsStorage { get; }

        private void GenerateUsers()
        {
            UsersStorage.Add(new User() {ID = 1, Login = "Admin", Password = "Secret"});
            UsersStorage.Add(new User() {ID = 1, Login = "Lara1n", Password = "2324"});
            UsersStorage.Add(new User() {ID = 1, Login = "User", Password = "pass"});
        }
    }
}