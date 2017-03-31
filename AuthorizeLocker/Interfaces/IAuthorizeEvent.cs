using System;

namespace Interfaces.Authorizer {
    public interface IAuthorizeEvent
    {
        bool IsActive { get; }
        DateTime TimeOccurred { get; }
    }
}