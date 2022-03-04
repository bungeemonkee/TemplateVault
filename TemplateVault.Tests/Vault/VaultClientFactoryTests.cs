using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using NUnit.Framework;
using TemplateVault.Vault;
using VaultSharp;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Tests.Vault
{
    [ExcludeFromCodeCoverage]
    public class VaultClientFactoryTests
    {
        [Test]
        public void GetVaultClient()
        {
            var uri = new Uri("https://www.example.com/things/and/stuff");
            
            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            authMock.Setup(x => x.AuthMethodType)
                .Returns(AuthMethodType.UserPass);

            var factory = new VaultClientFactory();
            var client = factory.GetVaultClient(authMock.Object, uri);
            
            Assert.IsInstanceOf<VaultClient>(client);
            
            authMock.VerifyAll();
        }
    }
}