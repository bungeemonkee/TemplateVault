using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using NUnit.Framework;
using TemplateVault.Utilities;
using TemplateVault.Vault;
using VaultSharp.V1.AuthMethods.AppRole;

namespace TemplateVault.Tests.Vault
{
    [ExcludeFromCodeCoverage]
    public class VaultAuthFactoryTests
    {
        [Test]
        public void GetAuth_InvalidAuthType()
        {
            var consoleMock = new Mock<IAbstractConsole>();
            
            var factory = new VaultAuthFactory(consoleMock.Object);

            Assert.Throws<InvalidOperationException>(() => factory.GetAuth("invalid type", null));
        }

        [Test]
        public void GetAuth_Approle()
        {
            var consoleMock = new Mock<IAbstractConsole>();
            consoleMock.SetupReadValue("username");
            consoleMock.SetupReadSecureValue("password");

            var factory = new VaultAuthFactory(consoleMock.Object);
            var auth = factory.GetAuth("approle", null);

            Assert.IsInstanceOf<AppRoleAuthMethodInfo>(auth);
            
            consoleMock.VerifyAll();
        }

        [Test]
        public void GetAuth_Approle_Mount()
        {
            var consoleMock = new Mock<IAbstractConsole>();
            consoleMock.SetupReadValue("username");
            consoleMock.SetupReadSecureValue("password");

            var factory = new VaultAuthFactory(consoleMock.Object);
            var auth = factory.GetAuth("approle", "mount");

            Assert.IsInstanceOf<AppRoleAuthMethodInfo>(auth);
            
            consoleMock.VerifyAll();
        }
    }
}