using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TemplateVault.Utilities;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Azure;
using VaultSharp.V1.AuthMethods.GitHub;
using VaultSharp.V1.AuthMethods.GoogleCloud;
using VaultSharp.V1.AuthMethods.JWT;
using VaultSharp.V1.AuthMethods.Kerberos;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.AuthMethods.LDAP;
using VaultSharp.V1.AuthMethods.Okta;
using VaultSharp.V1.AuthMethods.RADIUS;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods.UserPass;

namespace TemplateVault.Vault
{
    public class VaultAuthFactory : IVaultAuthFactory
    {
        private readonly IAbstractConsole _console;
        private readonly IReadOnlyDictionary<string, (string description, Func<string?, IAuthMethodInfo> method)> _authTypes;
        
        public VaultAuthFactory(IAbstractConsole console)
        {
            _console = console;

            _authTypes = new Dictionary<string, (string, Func<string?, IAuthMethodInfo>)>
            {
                ["approle"] = ("AppRole authentication", GetApproleAuthorization),
                // ["alicloud"] = (AliCloud) SUPPORTED (needs base64 encoded request uri and headers)
                // ["aws-ec2"] (AWS EC2) NOT SUPPORTED (needs to read key files)
                // ["aws-iam"] (AWS IAM) NOT SUPPORTED (needs base64 encoded request uri and headers)
                ["azure"] = ("Azure JWT authorization", GetAzureAuthorization),
                // ["cf"] (Cloud Factory) NOT SUPPORTED (requires reading key files)
                ["github"] = ("GitHub private token authentication", GetGitHubAuthorization),
                ["gcp"] = ("Google Cloud JWT authentication", GetGcpAuthorization),
                // ["oidc"] (JWT/OIDC) NOT SUPPORTED (requires browser redirection)
                ["jwt"] = ("JWT authentication", GetJwtAuthorization),
                ["kerbos"] = ("Kerbos username and password authentication", GetKerbosAuthorization),
                ["kubernetes"] = ("Kubernetes JWT authentication", GetKubernetesAuthorization),
                ["ldap"] = ("LDAP username and password authentication", GetLdapAuthentication),
                // ["oci"] (Oracle Cloud Infrastructure) NOT SUPPORTED (requires request header dictionary)
                ["okta"] = ("OKTA username and password authentication", GetOktaAuthorization),
                ["radius"] = ("RADIUS username and password authentication", GetRadiusAuthorization),
                // ["cert"] (TLS Certificate) NOT SUPPORTED (requires reading cert files)
                ["token"] = ("VAULT Token authentication", GetTokenAuthorization),
                ["userpass"] = ("VAULT username and password authentication (default)", GetUserpassAuthorization),
            };
        }

        public IEnumerable<(string name, string description)> GetSupportedAuthTypes()
        {
            return _authTypes.Select(x => (x.Key, x.Value.description));
        }

        public IAuthMethodInfo GetAuth(string authType, string? mountPoint)
        {
            if (_authTypes.TryGetValue(authType, out var value))
            {
                return value.method(mountPoint);
            }

            throw new InvalidOperationException("Unknown auth type: " + authType);
        }

        private IAuthMethodInfo GetApproleAuthorization(string? mount)
        {
            var role = ReadValue("AppRole RoleId", true);
            var secret = ReadSecureValue("AppRole SecretId");
            return mount == null
                ? new AppRoleAuthMethodInfo(role, secret)
                : new AppRoleAuthMethodInfo(mount, role, secret);
        }

        private IAuthMethodInfo GetAzureAuthorization(string? mount)
        {
            var roleName = ReadValue("Azure RoleId", true);
            var jwt = ReadSecureValue("Azure JWT");
            var subscriptionId = ReadValue("Azure SubscriptionId", false);
            var resourceGroupName = ReadValue("Azure Resource Group Name", false);
            var virtualMachineName = ReadValue("Azure Virtual Machine Name", false);
            var virtualMachineScaleSetName = ReadValue("Azure Virtual Machine Scale Set Name", false);
            return mount == null
                ? new AzureAuthMethodInfo(roleName, jwt, subscriptionId, resourceGroupName, virtualMachineName, virtualMachineScaleSetName)
                : new AzureAuthMethodInfo(mount, roleName, jwt, subscriptionId, resourceGroupName, virtualMachineName, virtualMachineScaleSetName);
        }

        private IAuthMethodInfo GetGitHubAuthorization(string? mount)
        {
            var token = ReadSecureValue("GitHub Personal Token");
            return mount == null
                ? new GitHubAuthMethodInfo(token)
                : new GitHubAuthMethodInfo(mount, token);
        }

        private IAuthMethodInfo GetGcpAuthorization(string? mount)
        {
            var role = ReadValue("Google RoleId", true);
            var jwt = ReadSecureValue("Google JWT");
            return mount == null
                ? new GoogleCloudAuthMethodInfo(role, jwt)
                : new GoogleCloudAuthMethodInfo(mount, role, jwt);
        }

        private IAuthMethodInfo GetJwtAuthorization(string? mount)
        {
            var role = ReadValue("JWT Role", true);
            var jwt = ReadSecureValue("JWT");
            return mount == null
                ? new JWTAuthMethodInfo(role, jwt)
                : new JWTAuthMethodInfo(mount, role, jwt);
        }

        private IAuthMethodInfo GetKerbosAuthorization(string? mount)
        {
            var user = ReadValue("Kerbos Username", true);
            var pass = ReadSecureValue("Kerbos Password");
            var domain = ReadValue("Kerbos Domain", true);
            
            var credentials = new NetworkCredential(user, pass, domain);
            
            return mount == null
                ? new KerberosAuthMethodInfo(credentials)
                : new KerberosAuthMethodInfo(mount, credentials);
        }

        private IAuthMethodInfo GetKubernetesAuthorization(string? mount)
        {
            var role = ReadValue("Kubernetes Role Id", true);
            var jwt = ReadSecureValue("Kubernetes JWT");
            
            return mount == null
                ? new KubernetesAuthMethodInfo(role, jwt)
                : new KubernetesAuthMethodInfo(mount, role, jwt);
        }

        private IAuthMethodInfo GetLdapAuthentication(string? mount)
        {
            var user = ReadValue("LDAP Username", true);
            var pass = ReadSecureValue("LDAP Password");
            
            return mount == null
                ? new LDAPAuthMethodInfo(user, pass)
                : new LDAPAuthMethodInfo(mount, user, pass);
        }
        
        private IAuthMethodInfo GetOktaAuthorization(string? mount)
        {
            var user = ReadValue("OKTA Username", true);
            var pass = ReadSecureValue("OKTA Password");
            
            return mount == null
                ? new OktaAuthMethodInfo(user, pass)
                : new OktaAuthMethodInfo(mount, user, pass);
        }
        
        private IAuthMethodInfo GetRadiusAuthorization(string? mount)
        {
            var user = ReadValue("RADIUS Username", true);
            var pass = ReadSecureValue("RADIUS Password");

            return mount == null
                ? new RADIUSAuthMethodInfo(user, pass)
                : new RADIUSAuthMethodInfo(mount, user, pass);
        }

        private IAuthMethodInfo GetTokenAuthorization(string? mount)
        {
            var token = ReadSecureValue("Token");
            return new TokenAuthMethodInfo(token);
        }

        private IAuthMethodInfo GetUserpassAuthorization(string? mount)
        {
            var user = ReadValue("Username", true);
            var pass = ReadSecureValue("Password");
            return mount == null
                ? new UserPassAuthMethodInfo(user, pass)
                : new UserPassAuthMethodInfo(mount, user, pass);
        }

        private string? ReadValue(string prompt, bool required)
        {
            string? value = null;
            do
            {
                _console.Write("{0} ({1}): ", prompt, required ? "required":  "may be blank" );
                
                value = _console.ReadLine();
            } while (required && string.IsNullOrWhiteSpace(value));

            if (string.IsNullOrWhiteSpace(value))
            {
                // turn empty and whitespace strings into null
                value = null;
            }

            return value;
        }

        private string ReadSecureValue (string prompt)
        {
            _console.Write("{0} (required):", prompt);

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