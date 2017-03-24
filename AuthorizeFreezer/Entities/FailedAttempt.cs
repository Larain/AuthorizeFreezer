
using System;

namespace AuthorizeLocker
{
    public class FailedAttempt
    {
        public string BinData { get; set; }
        public DateTime TimeOccured { get; set; }
    }
}