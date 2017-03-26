using System;
using AuthorizeLocker.Authorizer;
using AuthorizeLocker.Authorizer.ServiceMenu;
using AuthorizeLocker.DBLayer;
using AuthorizeLocker.Interfaces;

namespace ClinetAuthorizer
{
    class Program
    {
        private static string _login;
        private static string _password;
        private static readonly TestDbManager DbManager = new TestDbManager();

        static void Main(string[] args)
        {
            bool again = true;
            IAuthorizer authorizer = new Authorizer(new DbManager());

            authorizer.LockStarted += AuthorizerOnLockStarted;
            authorizer.LockReleased += AuthorizerOnLockReleased;

            while (again)
            {
                Console.WriteLine("Enter your credentials:");
                Console.Write("Login > ");
                _login = Console.ReadLine();
                Console.WriteLine();

                Console.Write("Password > ");
                _password = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Cheking your credentials...");
                if (authorizer.IsBlocked)
                {
                    var oldVal = Console.ForegroundColor;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("######################");
                    Console.WriteLine("### AUTHORIZE LOCK ###");
                    Console.WriteLine($"## Locked to: {authorizer.BlockedTo:hh:mm} ##");
                    Console.WriteLine("######################");

                    Console.ForegroundColor = oldVal;
                }
                else
                {
                    if (authorizer.Login(Authetificate))
                    {
                        var oldVal = Console.ForegroundColor;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("### SUCCESS ###");

                        Console.ForegroundColor = oldVal;
                    }
                    else
                    {
                        var oldVal = Console.ForegroundColor;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("### FAIL ###");

                        Console.ForegroundColor = oldVal;
                    }
                }


                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    again = false;

                Console.WriteLine();
            }

        }

        public static bool Authetificate()
        {
            return DbManager.Login(_login, _password);
        }

        private static void AuthorizerOnLockReleased(object sender, EventArgs eventArgs)
        {
            var oldVal = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Authorization lock has been released");

            Console.ForegroundColor = oldVal;
        }

        private static void AuthorizerOnLockStarted(object sender, EventArgs eventArgs)
        {
            IAuthorizer authorizer = (IAuthorizer)sender;

            var oldVal = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Authorization has been locked to {authorizer.BlockedTo}");

            Console.ForegroundColor = oldVal;
        }
    }
}
