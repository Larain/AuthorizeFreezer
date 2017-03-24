using System;
using System.Diagnostics.Tracing;
using AuthorizeLocker.Entities;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker {
    public class ServiceMenuAuthorizer : AuthorizerBase
    {
        protected override IUnlock Unlocker => DbManager.GetLastUnlocker();

        protected override ILock Locker => DbManager.GetLastLocker(LookFrom);

        public override void CreateLock(int number)
        {
            DbManager.AddLock(number);
        }

        public override void CreateUnlock(int duration)
        {
            DbManager.AddUnlcok(duration);
        }

        protected override void SaveAttempt()
        {
            DbManager.AddFailedAuthorizeAttempts("Попытка авторизации в сервисном меню");
        }

        protected override int GetFailedAttempts(DateTime lookFrom)
        {
            return DbManager.GetFailedAuthorizeAttempts(lookFrom);
        }
    }
}