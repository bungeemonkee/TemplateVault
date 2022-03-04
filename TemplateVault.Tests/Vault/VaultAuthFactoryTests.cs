using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using NUnit.Framework;
using TemplateVault.Utilities;
using TemplateVault.Vault;
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

namespace TemplateVault.Tests.Vault
{
    [ExcludeFromCodeCoverage]
    public class VaultAuthFactoryTests
    {
        [Test]
        public void GetSupportedAuthTypes()
        {
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            
            var factory = new VaultAuthFactory(consoleMock.Object);
            var authTypes = factory.GetSupportedAuthTypes();
            
            Assert.IsNotEmpty(authTypes);
            
            consoleMock.VerifyAll();
        }
        
        [Test]
        public void GetAuth_InvalidAuthType()
        {
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            
            var factory = new VaultAuthFactory(consoleMock.Object);

            Assert.Throws<InvalidOperationException>(() => factory.GetAuth("invalid type", null));
        }
        
        [Test]
        [TestCase("approle", typeof(AppRoleAuthMethodInfo), new[] {"roleId"}, new [] {"secret"}, null)]
        [TestCase("approle", typeof(AppRoleAuthMethodInfo), new[] {"roleId"}, new [] {"secret"}, "mount")]
        [TestCase("azure", typeof(AzureAuthMethodInfo), new[] {"roleId", null, null, null, null}, new [] {"jwt"}, null)]
        [TestCase("azure", typeof(AzureAuthMethodInfo), new[] {"roleId", null, null, null, null}, new [] {"jwt"}, "mount")]
        [TestCase("github", typeof(GitHubAuthMethodInfo), null, new [] {"token"}, null)]
        [TestCase("github", typeof(GitHubAuthMethodInfo), null, new [] {"token"}, "mount")]
        [TestCase("gcp", typeof(GoogleCloudAuthMethodInfo), new[] {"roleId"}, new [] {"jwt"}, null)]
        [TestCase("gcp", typeof(GoogleCloudAuthMethodInfo), new[] {"roleId"}, new [] {"jwt"}, "mount")]
        [TestCase("jwt", typeof(JWTAuthMethodInfo), new[] {"roleId"}, new [] {"jwt"}, null)]
        [TestCase("jwt", typeof(JWTAuthMethodInfo), new[] {"roleId"}, new [] {"jwt"}, "mount")]
        [TestCase("kerbos", typeof(KerberosAuthMethodInfo), new[] {"username", "domain"}, new [] {"password"}, null)]
        [TestCase("kerbos", typeof(KerberosAuthMethodInfo), new[] {"username", "domain"}, new [] {"password"}, "mount")]
        [TestCase("kubernetes", typeof(KubernetesAuthMethodInfo), new[] {"roleId"}, new [] {"jwt"}, null)]
        [TestCase("kubernetes", typeof(KubernetesAuthMethodInfo), new[] {"roleId"}, new [] {"jwt"}, "mount")]
        [TestCase("ldap", typeof(LDAPAuthMethodInfo), new[] {"username"}, new [] {"password"}, null)]
        [TestCase("ldap", typeof(LDAPAuthMethodInfo), new[] {"username"}, new [] {"password"}, "mount")]
        [TestCase("okta", typeof(OktaAuthMethodInfo), new[] {"username"}, new [] {"password"}, null)]
        [TestCase("okta", typeof(OktaAuthMethodInfo), new[] {"username"}, new [] {"password"}, "mount")]
        [TestCase("radius", typeof(RADIUSAuthMethodInfo), new[] {"username"}, new [] {"password"}, null)]
        [TestCase("radius", typeof(RADIUSAuthMethodInfo), new[] {"username"}, new [] {"password"}, "mount")]
        [TestCase("token", typeof(TokenAuthMethodInfo), null, new [] {"token"}, null)]
        [TestCase("token", typeof(TokenAuthMethodInfo), null, new [] {"token"}, "mount")]
        [TestCase("userpass", typeof(UserPassAuthMethodInfo), new[] {"username"}, new [] {"password"}, null)]
        [TestCase("userpass", typeof(UserPassAuthMethodInfo), new[] {"username"}, new [] {"password"}, "mount")]
        [TestCase("userpass", typeof(UserPassAuthMethodInfo), new[] {"username"}, new [] {"pass\bsword"}, null)]
        public void GetAuth(string authType, Type expectedType, string?[]? values, string[]? secureValues, string mount)
        {
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            if (values != null)
            {
                consoleMock.SetupReadValue(values);
            }

            if (secureValues != null)
            {
                consoleMock.SetupReadSecureValue(secureValues);
            }

            var factory = new VaultAuthFactory(consoleMock.Object);
            var auth = factory.GetAuth(authType, mount);

            Assert.IsInstanceOf(expectedType, auth);
            
            consoleMock.VerifyAll();
        }
    }
}