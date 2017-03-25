using System;

namespace AuthorizeLocker.Interfaces {
    public interface IUnlock : IAuthorizeEvent {
        int DurationInMinutes { get; }
        DateTime TimeUnlockedTo { get; }

    }
}