using System;

namespace AuthorizeLocker.Interfaces
{
    public interface IAuthorizeEvent
    {
        bool IsActive { get; }
        DateTime TimeOccurred { get; }
    }
}