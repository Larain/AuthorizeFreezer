using System;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker {
    public class ServiceMenuLocker : ILock {
        public ServiceMenuLocker(int lockNumber, DateTime timeOccured) {
            LockNumber = lockNumber;
            TimeOccurred = timeOccured;
        }

        public DateTime TimeLockedTo => TimeOccurred.AddMinutes(GetBlockingDuration(LockNumber));

        public bool IsActive => TimeLockedTo > DateTime.Now;

        public int LockNumber { get; }

        public DateTime TimeOccurred { get; }

        /// <summary>
        /// Formula for blocking duration calculating
        /// </summary>
        /// <param name="lockNumber"></param>
        /// <returns>Duration of lock in minutes</returns>
        private int GetBlockingDuration(int lockNumber) {
            switch (lockNumber) {
                case 1:
                    return 1;
                case 2:
                    return 5;
                case 3:
                    return 15;
                case 4:
                    return 30;
                case 5:
                    return 60;
                default:
                    return 60;
            }
        }
    }
}