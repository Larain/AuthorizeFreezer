using System;
using AuthorizeLocker.Authorizer;
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
            var tdbm = new TestDbManager();
            tdbm.Reset();
        }

        #region IsBlocked Status Tests

        [Test]
        public void TestAtStartIsNotBlocked()
        {
            var sma = new Authorizer(new DbManager());

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void Test3AttemptsIsBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new Authorizer(new DbManager());

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.True(sma.IsBlocked);
        }

        [Test]
        public void Test2AttemptsIsNotBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new Authorizer(new DbManager());

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void Test2AttemptsWithSuccessIsNotBlocked()
        {
            Func<bool> failedLoginAttempt = () => false;
            Func<bool> successfulLoginAttempt = () => true;

            var sma = new Authorizer(new DbManager());

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

            var sma = new Authorizer(new DbManager());

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

            var sma = new Authorizer(new DbManager());

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

            var sma = new Authorizer(new DbManager());

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
            var dbManager = new DbManager();

            var sma = new Authorizer(dbManager);

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.True(sma.IsBlocked);

            dbManager.CreateUnlock(1);

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
            var sma = new Authorizer(new DbManager());

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            Assert.AreEqual(2, sma.FailedAttempts);
        }

        [Test]
        public void Test4Attempts()
        {
            Func<bool> failedLoginAttempt = () => false;
            var sma = new Authorizer(new DbManager());

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

            var sma = new Authorizer(new DbManager());

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
            var dbManager = new DbManager();
            dbManager.CreateLock(1);

            var sma = new Authorizer(dbManager);

            Assert.True(sma.BlockedTo > DateTime.Now && sma.BlockedTo <= DateTime.Now.AddMinutes(1));
        }

        [Test]
        public void Test5MinsLock()
        {
            var dbManager = new DbManager();
            dbManager.CreateLock(2);

            var sma = new Authorizer(dbManager);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(4) && sma.BlockedTo <= DateTime.Now.AddMinutes(5));
        }

        [Test]
        public void Test15MinsLock()
        {
            var dbManager = new DbManager();
            dbManager.CreateLock(3);

            var sma = new Authorizer(dbManager);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(14) && sma.BlockedTo <= DateTime.Now.AddMinutes(15));
        }

        [Test]
        public void Test30MinsLock()
        {
            var dbManager = new DbManager();
            dbManager.CreateLock(4);

            var sma = new Authorizer(dbManager);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(29) && sma.BlockedTo <= DateTime.Now.AddMinutes(30));
        }

        [Test]
        public void Test60MinsLock()
        {
            var dbManager = new DbManager();
            dbManager.CreateLock(5);

            var sma = new Authorizer(dbManager);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(59) && sma.BlockedTo <= DateTime.Now.AddMinutes(60));
        }

        #endregion

    }
}