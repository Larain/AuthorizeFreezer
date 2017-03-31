using System;
using AuthorizeLocker.Authorizer;
using AuthorizeLocker.Authorizer.ServiceMenu;
using AuthorizeLocker.Interfaces;
using Moq;
using NUnit.Framework;

namespace AuthorizeTests
{
    [TestFixture]
    public class AuthorizerTests
    {
        #region IsBlocked Status Tests

        [Test]
        public void AtStartIsNotBlockedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            var sma = new Authorizer(dbMock.Object);

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void WithUnlockIsUnlockedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(DateTime.Now, 1));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.IsUnlocked);
        }

        [Test]
        public void ActiveLockIsBlockedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 1));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.IsBlocked);
        }

        [Test]
        public void NonActiveLockIsNotBlockedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now.AddMinutes(-2), 1));
            var sma = new Authorizer(dbMock.Object);

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void OldLockAndNonActiveUnlockIsNotBlockedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now.AddMinutes(-2), 2));
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(DateTime.Now));
            var sma = new Authorizer(dbMock.Object);

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void NewLockAndActiveUnlockIsNotBlockedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(DateTime.Now, 1));
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 2));
            var sma = new Authorizer(dbMock.Object);

            Assert.False(sma.IsBlocked);
        }

        [Test]
        public void ThreeFailedAttemptsCreateLockIsCalledTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetFailedAttempts(It.IsAny<DateTime>())).Returns(3);
            new Authorizer(dbMock.Object);

            dbMock.Verify(m => m.CreateLock(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void NextLockCreatedCorrectlyTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now.AddMinutes(-2), 1));
            dbMock.Setup(m => m.GetFailedAttempts(It.IsAny<DateTime>())).Returns(3);
            var sma = new Authorizer(dbMock.Object);

            dbMock.Verify(m => m.CreateLock(It.Is<int>(l => l.Equals(2))));
        }

        // TO DO: Enhance test logic to invoke Event 
        [Test]
        public void OnLockEventInvokedTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();

            var sma = new Authorizer(dbMock.Object);
            sma.LockStarted += SmaOnLockStarted;

            var newDbMock = new Mock<IDBAuthorizeManager>();

            newDbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 1));
            newDbMock.Setup(m => m.GetFailedAttempts(It.IsAny<DateTime>())).Returns(3);

            sma = new Authorizer(newDbMock.Object);

            Assert.True(sma.IsBlocked);
        }

        // Is never invoked
        private void SmaOnLockStarted(object sender, EventArgs eventArgs)
        {
            Assert.Pass();
        }


        [Test]
        public void SuccessAttemptCreateUnlockIsCalledTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            var sma = new Authorizer(dbMock.Object);

            Func<bool> successfulLoginAttempt = () => true;

            sma.Login(successfulLoginAttempt);

            dbMock.Verify(m => m.CreateUnlock(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void SuccessAttemptWithLockCreateUnlockIsNotCalledTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 1));
            var sma = new Authorizer(dbMock.Object);

            Func<bool> successfulLoginAttempt = () => true;

            sma.Login(successfulLoginAttempt);

            dbMock.Verify(m => m.CreateUnlock(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region Attempts Tests

        [Test]
        public void SaveAttemptCalledTwoTimesTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            var sma = new Authorizer(dbMock.Object);

            Func<bool> failedLoginAttempt = () => false;

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            dbMock.Verify(m => m.SaveAttempt(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void SaveAttemptIsNotCalledWithActiveLockTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 1));
            var sma = new Authorizer(dbMock.Object);

            Func<bool> failedLoginAttempt = () => false;

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            dbMock.Verify(m => m.SaveAttempt(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SaveAttemptIsCalledWithNonActiveLockTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now.AddMinutes(-2), 1));
            var sma = new Authorizer(dbMock.Object);

            Func<bool> failedLoginAttempt = () => false;

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            dbMock.Verify(m => m.SaveAttempt(It.IsAny<string>()), Times.Exactly(3));
        }

        [Test]
        public void SaveAttemptIsNotCalledWithActiveUnlockTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(DateTime.Now, 1));
            var sma = new Authorizer(dbMock.Object);

            Func<bool> failedLoginAttempt = () => false;

            sma.Login(failedLoginAttempt);

            dbMock.Verify(m => m.SaveAttempt(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SaveAttemptIsCalledWithNonActiveUnlockTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(DateTime.Now.AddMinutes(-2), 1));
            var sma = new Authorizer(dbMock.Object);

            Func<bool> failedLoginAttempt = () => false;

            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);
            sma.Login(failedLoginAttempt);

            dbMock.Verify(m => m.SaveAttempt(It.IsAny<string>()), Times.Exactly(4));
        }

        [Test]
        public void LoginWithNullActionThrowExceptionTest()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            var sma = new Authorizer(dbMock.Object);

            Assert.Throws<ArgumentNullException>(() => sma.Login(null));
        }

        [Test]
        public void LookForAttemtpsFromLastEventTest()
        {
            DateTime recentTime = DateTime.Now.AddMinutes(-30);
            DateTime oldTime = DateTime.Now.AddMinutes(-35);

            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(recentTime, 1));
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(oldTime, 1));
            new Authorizer(dbMock.Object);

            dbMock.Verify(m => m.GetFailedAttempts(It.Is<DateTime>(d => d.Equals(recentTime))));
        }

        [Test]
        public void LookForAttemtpsFromLastEvent2Test()
        {
            DateTime recentTime = DateTime.Now.AddMinutes(-30);
            DateTime oldTime = DateTime.Now.AddMinutes(-35);

            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastUnlocker()).Returns(new ServiceMenuUnlocker(oldTime, 1));
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(recentTime, 1));
            new Authorizer(dbMock.Object);

            dbMock.Verify(m => m.GetFailedAttempts(It.Is<DateTime>(d => d.Equals(recentTime))));
        }

        #endregion

        #region Lock Duration Tests

        [Test]
        public void Test1MinsLock()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 1));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.BlockedTo > DateTime.Now && sma.BlockedTo <= DateTime.Now.AddMinutes(1));
        }

        [Test]
        public void Test5MinsLock()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 2));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(4) && sma.BlockedTo <= DateTime.Now.AddMinutes(5));
        }

        [Test]
        public void Test15MinsLock()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 3));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(14) && sma.BlockedTo <= DateTime.Now.AddMinutes(15));
        }

        [Test]
        public void Test30MinsLock()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 4));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(29) && sma.BlockedTo <= DateTime.Now.AddMinutes(30));
        }

        [Test]
        public void Test60MinsLock()
        {
            var dbMock = new Mock<IDBAuthorizeManager>();
            dbMock.Setup(m => m.GetLastLocker()).Returns(new ServiceMenuLocker(DateTime.Now, 5));
            var sma = new Authorizer(dbMock.Object);

            Assert.True(sma.BlockedTo > DateTime.Now.AddMinutes(59) && sma.BlockedTo <= DateTime.Now.AddMinutes(60));
        }

        #endregion
    }
}