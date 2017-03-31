using System;

namespace Interfaces.Authorizer
{
    public interface IUnlock : IAuthorizeEvent
    {
        int DurationInMinutes { get; }
        DateTime TimeUnlockedTo { get; }

    }
}