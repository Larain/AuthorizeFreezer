using System;
using System.Threading;
using System.Timers;
using AuthorizeLocker.Interfaces;
using Timer = System.Timers.Timer;

namespace AuthorizeLocker.Authorizer {
    public class Authorizer : IAuthorizer {
        private const int MAX_FAILURES_AMOUNT = 3;

        private Timer _timer;
        private readonly IAuthorizeDbManager _dbManager;

        public Authorizer(IAuthorizeDbManager dbManager)
        {
            _dbManager = dbManager; 
            ProcessBolcking();
        }

        /// <summary>
        /// Invokes when authorizer lock authorization
        /// </summary>
        public event EventHandler LockStarted;
        /// <summary>
        /// Invokes when lock is released
        /// </summary>
        public event EventHandler LockReleased;

        #region DB Layer

        /// <summary>
        /// Return failed authorize attempts
        /// </summary>
        /// <param name="lookFrom">The time point to search attempts from</param>
        /// <returns></returns>
        protected int GetFailedAttempts(DateTime lookFrom)
        {
            return _dbManager.GetFailedAttempts(lookFrom);
        }

        /// <summary>
        /// Get last event of unlock
        /// </summary>
        protected IUnlock Unlocker => _dbManager.GetLastUnlocker();

        /// <summary>
        /// Get last event of lock
        /// </summary>
        protected ILock Locker => _dbManager.GetLastLocker(LookFrom);

        /// <summary>
        /// Save failed authorize attempt
        /// </summary>
        protected void SaveAttempt(string binData = "")
        {
            _dbManager.SaveAttempt(binData);
        }

        /// <summary>
        /// Create a lock for authorization
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Lock sequence number</returns>
        protected void CreateLock(int number)
        {
            _dbManager.CreateLock(number);
        }

        /// <summary>
        /// Create an unlock for authorization
        /// </summary>
        /// <param name="duration">Duration of unlock in minutes</param>
        protected void CreateUnlock(int duration)
        {
            _dbManager.CreateUnlock(duration);
        }

        #endregion

        /// <summary>
        /// Time when block ends
        /// </summary>
        public DateTime BlockedTo => Locker.TimeLockedTo;

        /// <summary>
        /// Time point to search event from
        /// </summary>
        protected DateTime LookFrom => Unlocker?.TimeOccurred ?? DateTime.Now.AddHours(-2);

        /// <summary>
        /// Amount of failed authorize attempts
        /// </summary>
        public int FailedAttempts
        {
            get
            {
                var cachedUnlocker = Unlocker;
                var cachedLocker = Locker;

                if (cachedUnlocker != null && cachedUnlocker.IsActive ||
                    cachedLocker != null && cachedLocker.IsActive)
                    return 0;

                // If there are non-active lock and unlock event
                // we have to choose which of them is more fresh
                if (cachedLocker != null && cachedUnlocker != null)
                {
                    return GetFailedAttempts(cachedLocker.TimeOccurred >= cachedUnlocker.TimeOccurred
                        ? cachedLocker.TimeOccurred
                        : cachedUnlocker.TimeOccurred);
                }

                return GetFailedAttempts(cachedLocker?.TimeOccurred ?? LookFrom);
            }
        }

        /// <summary>
        /// Indicator is authorization blocked
        /// </summary>
        public bool IsBlocked {
            get {
                var ublocker = Unlocker;
                if (ublocker != null)
                    if (ublocker.IsActive)
                        return false;

                var locker = Locker;
                if (locker != null)
                    if (locker.IsActive)
                        return true;

                return false;
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
            Thread.Sleep(1);
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
                if (unlocker.IsActive)
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