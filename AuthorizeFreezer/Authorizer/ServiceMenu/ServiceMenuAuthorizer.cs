using System;
using AuthorizeLocker.DBLayer;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.Authorizer.ServiceMenu {
    public class ServiceMenuAuthorizer : AuthorizerBase
    {
        protected override IUnlock Unlocker => DbManager.GetLastUnlocker();

        protected override ILock Locker => DbManager.GetLastLocker(LookFrom);

        protected override void CreateLock(int number)
        {
            DbManager.AddLock(number);
        }

        protected override void CreateUnlock(int duration)
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