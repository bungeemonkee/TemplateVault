using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace TemplateVault
{
    public class VaultAuthFactory : IVaultAuthFactory
    {
        private readonly IAbstractConsole _console;
        private readonly IReadOnlyDictionary<string, Func<string?, IAuthMethodInfo>> _authTypes;
        
        public VaultAuthFactory(IAbstractConsole console)
        {
            _console = console;

            _authTypes = new Dictionary<string, Func<string?, IAuthMethodInfo>>
            {
                ["approle"] = GetApproleAuthorization,
                // ["alicloud"] = (AliCloud) SUPPORTED (needs base64 encoded request uri and headers)
                // ["aws-ec2"] (AWS EC2) NOT SUPPORTED (needs to read key files)
                // ["aws-iam"] (AWS IAM) NOT SUPPORTED (needs base64 encoded request uri and headers)
                ["azure"] = GetAzureAuthorization,
                // ["cf"] (Cloud Factory) NOT SUPPORTED (requires reading key files)
                ["github"] = GetGitHubAuthorization,
                ["gcp"] = GetGcpAuthorization,
                // ["oidc"] (JWT/OIDC) NOT SUPPORTED (requires browser redirection)
                ["jwt"] = GetJetAuthorization,
                ["kerbos"] = GetKerbosAuthorization,
                ["kubernetes"] = GetKubernetesAuthorization,
                ["ldap"] = GetLdapAuthentication,
                // ["oci"] (Oracle Cloud Infrastructure) NOT SUPPORTED (requires request header dictionary)
                ["okta"] = GetOktaAuthorization,
                ["radius"] = GetRadiusAuthorization,
                // ["cert"] (TLS Certificate) NOT SUPPORTED (requires reading cert files)
                ["token"] = GetTokenAuthorization,
                ["userpass"] = GetUserpassAuthorization,
            };
        }

        public string[] GetSupportedAuthTypes()
        {
            return _authTypes.Keys.ToArray();
        }

        public IAuthMethodInfo GetAuth(string authType, string? mountPoint)
        {
            if (_authTypes.TryGetValue(authType, out var func))
            {
                return func(mountPoint);
            }

            throw new InvalidOperationException("Unknown auth type: " + authType);
        }

        private IAuthMethodInfo GetApproleAuthorization(string? mount)
        {
            var role = ReadValue("AppRole RoleId", false);
            var secret = ReadSecureValue("AppRole SecretId");
            return mount == null
                ? new AppRoleAuthMethodInfo(role, secret)
                : new AppRoleAuthMethodInfo(mount, role, secret);
        }

        private IAuthMethodInfo GetAzureAuthorization(string? mount)
        {
            var roleName = ReadValue("Azure RoleId", false);
            var jwt = ReadSecureValue("Azure JWT");
            var subscriptionId = ReadValue("Azure SubscriptionId", true);
            var resourceGroupName = ReadValue("Azure Resource Group Name", true);
            var virtualMachineName = ReadValue("Azure Virtual Machine Name", true);
            var virtualMachineScaleSetName = ReadValue("Azure Virtual Machine Scale Set Name", true);
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
            var role = ReadValue("Google RoleId", false);
            var jwt = ReadSecureValue("Google JWT");
            return mount == null
                ? new GoogleCloudAuthMethodInfo(role, jwt)
                : new GoogleCloudAuthMethodInfo(mount, role, jwt);
        }

        private IAuthMethodInfo GetJetAuthorization(string? mount)
        {
            var role = ReadValue("JWT Role", false);
            var jwt = ReadSecureValue("JWT");
            return mount == null
                ? new JWTAuthMethodInfo(role, jwt)
                : new JWTAuthMethodInfo(mount, role, jwt);
        }

        private IAuthMethodInfo GetKerbosAuthorization(string? mount)
        {
            var user = ReadValue("Kerbos Username", false);
            var pass = ReadSecureValue("Kerbos Password");
            var domain = ReadValue("Kerbos Domain", false);
            
            var credentials = new NetworkCredential(user, pass, domain);
            
            return mount == null
                ? new KerberosAuthMethodInfo(credentials)
                : new KerberosAuthMethodInfo(mount, credentials);
        }

        private IAuthMethodInfo GetKubernetesAuthorization(string? mount)
        {
            var role = ReadValue("Kubernetes Role Id", false);
            var jwt = ReadSecureValue("Kubernetes JWT");
            
            return mount == null
                ? new KubernetesAuthMethodInfo(role, jwt)
                : new KubernetesAuthMethodInfo(mount, role, jwt);
        }

        private IAuthMethodInfo GetLdapAuthentication(string? mount)
        {
            var user = ReadValue("LDAP Username", false);
            var pass = ReadSecureValue("LDAP Password");
            
            return mount == null
                ? new LDAPAuthMethodInfo(user, pass)
                : new LDAPAuthMethodInfo(mount, user, pass);
        }
        
        private IAuthMethodInfo GetOktaAuthorization(string? mount)
        {
            var user = ReadValue("OKTA Username", false);
            var pass = ReadSecureValue("OKTA Password");
            
            return mount == null
                ? new OktaAuthMethodInfo(user, pass)
                : new OktaAuthMethodInfo(mount, user, pass);
        }
        
        private IAuthMethodInfo GetRadiusAuthorization(string? mount)
        {
            var user = ReadValue("RADIUS Username", false);
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
            var user = ReadValue("Username", false);
            var pass = ReadSecureValue("Password");
            return mount == null
                ? new UserPassAuthMethodInfo(user, pass)
                : new UserPassAuthMethodInfo(mount, user, pass);
        }

        private string? ReadValue(string prompt, bool allowEmpty)
        {
            _console.Write("{0} ({1}): ", prompt, allowEmpty ? "may be blank" : "required");

            string? value = null;
            do
            {
                value = _console.ReadLine();
            } while (allowEmpty || string.IsNullOrWhiteSpace(value));

            if (string.IsNullOrWhiteSpace(value))
            {
                // turn empty and whitespace strings into null
                value = null;
            }

            return value;
        }

        private string ReadSecureValue (string prompt)
        {
            _console.Write("{0}: (required)", prompt);

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