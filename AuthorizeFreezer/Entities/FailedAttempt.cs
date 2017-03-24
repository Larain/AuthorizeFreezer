using System;

namespace AuthorizeLocker.Entities
{
    public class FailedAttempt
    {
        public string BinData { get; set; }
        public DateTime TimeOccured { get; set; }
    }
}