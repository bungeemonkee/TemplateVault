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
    public class VaultSecretExtractorFactoryTests
    {
        [Test]
        public void GetVaultSecretExtractor()
        {
            var uri = new Uri("https://www.example.com/things/and/stuff");

            var clientMock = new Mock<IVaultClient>(MockBehavior.Strict);
            
            var clientFactoryMock = new Mock<IVaultClientFactory>(MockBehavior.Strict);
            clientFactoryMock.Setup(x => x.GetVaultClient(It.IsNotNull<IAuthMethodInfo>(), It.IsNotNull<Uri>()))
                .Returns(() => clientMock.Object);
                
            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);

            var factory = new VaultSecretExtractorFactory(clientFactoryMock.Object);
            var extractor = factory.GetVaultSecretExtractor(authMock.Object, uri);
            
            Assert.IsInstanceOf<VaultSecretExtractor>(extractor);
            
            clientMock.VerifyAll();
            clientFactoryMock.VerifyAll();
            authMock.VerifyAll();
        }
    }
}