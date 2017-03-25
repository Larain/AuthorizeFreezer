using System;
using AuthorizeLocker.Authorizer.ServiceMenu;
using AuthorizeLocker.DBLayer;
using NUnit.Framework;

namespace AuthorizeTests
{
    [TestFixture]
    public class ServiceMenuTests
    {
        [SetUp]
        public void CleanDb()
        {
            DbManager.Reset();
        }
        [Test]
        public void Test3AttemptsIsBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.True(sma.IsBlocked);
        }

        [Test]
        public void Test2AttemptsIsNotBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void Test2Attempts()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.AreEqual(2, sma.FailedAttempts);
        }

        [Test]
        public void Test4Attempts()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.AreEqual(0, sma.FailedAttempts);
        }

        [Test]
        public void Test2AttemptsWithSuccess()
        {
            Func<bool> failedLoginAttempt = () => false;
            Func<bool> successfulLoginAttempt = () => true;

            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(successfulLoginAttempt);

            Assert.AreEqual(0, sma.FailedAttempts);
        }

        [Test]
        public void Test2AttemptsWithSuccessIsNotBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            Func<bool> successfulLoginAttempt = () => true;

            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(successfulLoginAttempt);

            Assert.False(sma.IsBlocked);
        }
    }
}
