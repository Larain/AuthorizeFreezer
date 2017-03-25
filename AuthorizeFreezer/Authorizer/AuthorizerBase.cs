﻿using System;
using System.Timers;
using AuthorizeLocker.Interfaces;

namespace AuthorizeLocker.Authorizer {
    public abstract class AuthorizerBase : IAuthorizer {
        private const int MAX_FAILURES_AMOUNT = 3;

        private Timer _timer;
        private bool _isBlocked;

        protected AuthorizerBase() {
            ProcessBolcking();
        }

        public event EventHandler LockStarted;
        public event EventHandler LockReleased;

        #region Abstract Methods

        protected abstract int GetFailedAttempts(DateTime lookFrom);
        protected abstract IUnlock Unlocker { get; }
        protected abstract ILock Locker { get; }
        protected abstract void SaveAttempt();

        /// <summary>
        /// Create a lock for authorization
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Lock sequence number</returns>
        protected abstract void CreateLock(int number);

        /// <summary>
        /// Create an unlock for authorization
        /// </summary>
        /// <param name="duration">Duration of unlock in minutes</param>
        protected abstract void CreateUnlock(int duration);

        #endregion
        /// <summary>
        /// Time when block ends
        /// </summary>
        public DateTime BlockedTo => Locker.TimeLockedTo;

        /// <summary>
        /// Time point when search starts
        /// </summary>
        protected DateTime LookFrom => Unlocker?.TimeOccurred ?? DateTime.Now.AddHours(-2);

        /// <summary>
        /// Amount of failed authorize attempts
        /// </summary>
        public int FailedAttempts
        {
            get
            {
                if (Unlocker != null)
                {
                    if (Unlocker.IsActice)
                        return GetFailedAttempts(Unlocker.TimeOccurred);
                }

                if (Locker != null)
                {
                    if (Locker.IsActive)
                        return GetFailedAttempts(Locker.TimeOccurred);
                }

                if (Locker != null && Unlocker != null)
                {
                    GetFailedAttempts(Locker.TimeOccurred >= Unlocker.TimeOccurred
                        ? Locker.TimeOccurred
                        : Unlocker.TimeOccurred);
                }

                return GetFailedAttempts(Locker?.TimeOccurred ?? LookFrom);
            }
        }

        /// <summary>
        /// Indicator is authorization blocked
        /// </summary>
        public bool IsBlocked {
            get {
                var ublocker = Unlocker;
                if (ublocker != null)
                    if (ublocker.IsActice)
                        return false;

                var locker = Locker;
                if (locker != null)
                    if (locker.IsActive)
                        return true;

                return false;
            }
            set {
                if (_isBlocked == value) {
                    return;
                }
                _isBlocked = value;
            }
        }

        /// <summary>
        /// Timer for lock period
        /// </summary>
        private Timer Timer {
            get { return _timer ?? (_timer = new Timer()); }
            set { _timer = value; }
        }

        /// <summary>
        /// Main method that is used for login
        /// </summary>
        /// <param name="action">Delegate to check inputed creadentials</param>
        /// <returns></returns>
        public bool Login(Func<bool> action) {
            if (action == null) {
                throw new NullReferenceException("Authorize error: Login action is not set");
            }
            if (IsBlocked)
                return false;
            var isAutenticated = action.Invoke();
            if (isAutenticated) {
                CreateUnlock(0);
                return true;
            }

            var unlocker = Unlocker;
            if (unlocker != null)
                if (unlocker.IsActice)
                    return false;
            SaveAttempt();
            ProcessBolcking();
            return false;
        }

        private bool ProcessBolcking() {
            var locker = Locker;
            if (FailedAttempts >= MAX_FAILURES_AMOUNT) {
                if (locker != null)
                    CreateLock(locker.LockNumber + 1);
                else
                    CreateLock(1); // 1 - the first element of sequence

                OnIsBlockedChanged();

                var interval = (Locker.TimeLockedTo - DateTime.Now).TotalMilliseconds;
                Timer = new Timer(interval) {AutoReset = true};
                Timer.Start();
                Timer.Elapsed += TimerOnElapsed;

                return true;
            }
            return false;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            LockReleased?.Invoke(this, EventArgs.Empty);
        }

        private void OnIsBlockedChanged() {
            LockStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}