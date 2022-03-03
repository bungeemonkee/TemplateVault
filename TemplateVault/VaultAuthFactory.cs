using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Okta;
using VaultSharp.V1.AuthMethods.UserPass;

namespace TemplateVault
{
    public class VaultAuthFactory : IVaultAuthFactory
    {
        private readonly IAbstractConsole _console;
        private readonly IReadOnlyDictionary<string, Func<IAuthMethodInfo>> _authTypes;
        
        public VaultAuthFactory(IAbstractConsole console)
        {
            _console = console;

            _authTypes = new Dictionary<string, Func<IAuthMethodInfo>>
            {
                ["userpass"] = GetUserpassAuthorization,
                ["okta"] = GetOktaAuthorization,
            };
        }

        public string[] GetSupportedAuthTypes()
        {
            return _authTypes.Keys.ToArray();
        }

        public IAuthMethodInfo GetAuth(string authType)
        {
            if (_authTypes.TryGetValue(authType, out var func))
            {
                return func();
            }

            throw new InvalidOperationException("Unknown auth type: " + authType);
        }
        
        private IAuthMethodInfo GetUserpassAuthorization()
        {
            var user = ReadValue("Username");
            var pass = ReadSecureValue("Password");
            return new UserPassAuthMethodInfo(user, pass);
        }
        
        private IAuthMethodInfo GetOktaAuthorization()
        {
            var user = ReadValue("OKTA Username");
            var pass = ReadSecureValue("OKTA Password");
            return new OktaAuthMethodInfo(user, pass);
        }

        private string ReadValue(string? prompt)
        {
            if (prompt != null)
            {
                _console.Write("{0}: ", prompt);
            }

            string? user = null;
            while (string.IsNullOrWhiteSpace(user))
            {
                user = _console.ReadLine();
            }

            return user;
        }

        private string ReadSecureValue (string? prompt)
        {
            if (prompt != null)
            {
                _console.Write("{0}: ", prompt);
            }

            var passwordBuilder = new StringBuilder(32);
            do
            {
                ConsoleKey key;
                do
                {
                    // get the next key
                    // intercept the event to prevent it from being printed on the command line
                    var keyInfo = _console.ReadKey(true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && passwordBuilder.Length > 0)
                    {
                        // remove the last character from the string builder
                        passwordBuilder.Remove(passwordBuilder.Length - 1, 1);
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        // add any new non-control character to the password
                        passwordBuilder.Append(keyInfo.KeyChar);
                    }
                } while (key != ConsoleKey.Enter);
            } while (passwordBuilder.Length == 0);
            
            // insert the final enter that was captured and swallowed
            _console.WriteLine();

            // get the final string
            return passwordBuilder.ToString();
        }
    }
}