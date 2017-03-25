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

        #region IsBlocked Status Tests

        [Test]
        public void TestAtStartIsNotBlocked()
        {
            var sma = new ServiceMenuAuthorizer();

            Assert.False(sma.IsBlocked);
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

        [Test]
        public void Test3AttemptsWithSuccessIsBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            Func<bool> successfulLoginAttempt = () => true;

            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(successfulLoginAttempt);

            Assert.True(sma.IsBlocked);
        }

        [Test]
        public void TestSuccessAndFailIsNotBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            Func<bool> successfulLoginAttempt = () => true;

            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(successfulLoginAttempt);
            sma.Login(failedLoginAttempt);
            
            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void TestSuccessAndFailIsBlocked()
        {    
            Func<bool> failedLoginAttempt = () => false;
            Func<bool> successfulLoginAttempt = () => true;

            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            sma.Login(successfulLoginAttempt);

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.True(sma.IsBlocked);
        }

        [Test]
        public void TestWithUnlockPeriodIsNotBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;

            var sma = new ServiceMenuAuthorizer();

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.True(sma.IsBlocked);

            DbManager.AddUnlcok(1);

            Assert.False(sma.IsBlocked);

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.False(sma.IsBlocked);
        }

        #endregion

        #region Attempts Tests

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

        #endregion

        #region Lock Duration Tests

        [Test]
        public void Test1MinsLock()
        {
            DbManager.AddLock(1);

            var sma = new ServiceMenuAuthorizer();

            Assert.True(sma.BlockedTo > DateTime.Now && sma.BlockedTo <= DateTime.Now.AddMinutes(1));
        }

        [Test]
        public void Test5MinsLock()
        {
            DbManager.AddLock(2);

            var sma = new ServiceMenuAuthorizer();

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(4) && sma.BlockedTo <= DateTime.Now.AddMinutes(5));
        }

        [Test]
        public void Test15MinsLock()
        {
            DbManager.AddLock(3);

            var sma = new ServiceMenuAuthorizer();

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(14) && sma.BlockedTo <= DateTime.Now.AddMinutes(15));
        }

        [Test]
        public void Test30MinsLock()
        {
            DbManager.AddLock(4);

            var sma = new ServiceMenuAuthorizer();

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(29) && sma.BlockedTo <= DateTime.Now.AddMinutes(30));
        }

        [Test]
        public void Test60MinsLock()
        {
            DbManager.AddLock(5);

            var sma = new ServiceMenuAuthorizer();

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(59) && sma.BlockedTo <= DateTime.Now.AddMinutes(60));
        }

        #endregion

    }
}