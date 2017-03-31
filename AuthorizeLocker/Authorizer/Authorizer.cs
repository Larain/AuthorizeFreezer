using System;
using System.Threading;
using System.Timers;
using Interfaces.Authorizer;
using Interfaces.DB;
using log4net;
using Timer = System.Timers.Timer;

namespace AuthorizeLocker
{
    public class Authorizer : IAuthorizer
    {
        private const int MAX_FAILURES_AMOUNT = 3;
        public static readonly ILog Logger = LogManager.GetLogger("Authorizer");

        private Timer _timer;
        private readonly IDBAuthorizeManager _dbManager;

        public Authorizer(IDBAuthorizeManager dbManager)
        {
            _dbManager = dbManager;
            ProcessLocking();
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
        protected ILock Locker => _dbManager.GetLastLocker();

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
        public DateTime BlockedTo => Locker?.TimeLockedTo ?? new DateTime();

        /// <summary>
        /// Is any active unlock
        /// </summary>
        public bool IsUnlocked => Unlocker != null && Unlocker.IsActive;
        /// <summary>
        /// Is any active lock
        /// </summary>
        protected bool IsLocked => Locker != null && Locker.IsActive;

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
                IUnlock cachedUnlocker = Unlocker;
                ILock cachedLocker = Locker;

                if (IsUnlocked || IsLocked)
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
        public bool IsBlocked
        {
            get
            {
                IUnlock cachedUnlocker = Unlocker;
                if (cachedUnlocker != null)
                    if (cachedUnlocker.IsActive)
                        return false;

                ILock cachedLocker = Locker;
                if(cachedUnlocker != null && cachedLocker != null)
                    if (cachedUnlocker.TimeOccurred > cachedLocker.TimeOccurred)
                        return false;

                return cachedLocker != null && cachedLocker.IsActive;
            }
        }

        /// <summary>
        /// Timer for lock period
        /// </summary>
        private Timer Timer
        {
            get { return _timer ?? (_timer = new Timer()); }
            set { _timer = value; }
        }

        /// <summary>
        /// Main method that is used for login
        /// </summary>
        /// <param name="action">Delegate to check inputed creadentials</param>
        /// <returns></returns>
        public bool Login(Func<bool> action)
        {
            Thread.Sleep(1);
            if (action == null) {
                throw new ArgumentNullException("Authorize error: Login action is not set");
            }
            if (IsBlocked)
                return false;
            var isAutenticated = action.Invoke();
            if (isAutenticated)
            {
                CreateUnlock(0);
                return true;
            }

            IUnlock unlocker = Unlocker;
            if (unlocker != null)
                if (unlocker.IsActive)
                    return false;
            SaveAttempt();
            ProcessLocking();
            return false;
        }

        /// <summary>
        /// Create new Lock if needed
        /// </summary>
        /// <returns>Indicates whether the lock was created</returns>
        private bool UpdateLocks() {
            ILock locker = Locker;
            if (FailedAttempts < MAX_FAILURES_AMOUNT) return false;

            Logger.Info("Количество неудачных попыток авторизации превысило лимит, создаем блокировку...");
            if (locker != null)
                CreateLock(locker.LockNumber + 1);
            else
                CreateLock(1); // 1 - the first element of sequence

            return true;
        }

        private void ProcessLocking() {
            if (UpdateLocks())
                SetTimer();
        }

        private void SetTimer() {
            if (Timer.Enabled) return;
            try {
                var interval = (Locker.TimeLockedTo - DateTime.Now).TotalMilliseconds;
                Timer = new Timer(interval) {AutoReset = true};
                Timer.Start();
                OnLockStarted();
                Timer.Elapsed += OnLockReleased;
            }
            catch (Exception e) {
                Logger.Error("Не удалось запустить таймер", e);
            }
        }

        private void OnLockReleased(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            LockReleased?.Invoke(this, EventArgs.Empty);
        }

        private void OnLockStarted()
        {
            LockStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}