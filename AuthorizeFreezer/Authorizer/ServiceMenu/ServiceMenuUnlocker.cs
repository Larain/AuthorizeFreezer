using System;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.Authorizer.ServiceMenu {
    public class ServiceMenuUnlocker : IUnlock {
        public ServiceMenuUnlocker(DateTime timeOccurred, int duration = 0) {
            TimeOccurred = timeOccurred;
            DurationInMinutes = duration;
        }

        public DateTime TimeUnlockedTo => TimeOccurred.AddMinutes(DurationInMinutes);

        public bool IsActice => TimeUnlockedTo > DateTime.Now;

        public int DurationInMinutes { get; }
        public DateTime TimeOccurred { get; }
    }
}