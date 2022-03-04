using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TemplateVault.Vault;
using VaultSharp;
using VaultSharp.V1;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.KeyValue;
using VaultSharp.V1.SecretsEngines.KeyValue.V2;

namespace TemplateVault.Tests.Vault
{
    [ExcludeFromCodeCoverage]
    public class VaultSecretExtractorTests
    {
        [Test]
        [TestCase("https://www.example.com/things/and/stuff", "thing1", "and/stuff", "things", "thing1", "value1")]
        [TestCase("https://www.example.com/things/and/stuff", "thing1", "and/stuff", "things", "/things/and/stuff/thing1", "value1")]
        public async Task GetSecretValue(string rootUrl, string secretName, string secretPath, string mount, string key, string value)
        {
            var secret = new Secret<SecretData>
            {
                Data = new SecretData
                {
                    Data = new Dictionary<string, object>
                    {
                        [secretName] = value
                    }
                }
            };
            
            var keyValueV2Mock = new Mock<IKeyValueSecretsEngineV2>(MockBehavior.Strict);
            keyValueV2Mock.Setup(x => x.ReadSecretAsync(secretPath, null, mount, null))
                .ReturnsAsync(secret);

            var keyValueMock = new Mock<IKeyValueSecretsEngine>(MockBehavior.Strict);
            keyValueMock.Setup(x => x.V2)
                .Returns(() => keyValueV2Mock.Object);
            
            var secretsMock = new Mock<ISecretsEngine>(MockBehavior.Strict);
            secretsMock.Setup(x => x.KeyValue)
                .Returns(() => keyValueMock.Object);

            var clientV1Mock = new Mock<IVaultClientV1>(MockBehavior.Strict);
            clientV1Mock.Setup(x => x.Secrets)
                .Returns(() => secretsMock.Object);
            
            var clientMock = new Mock<IVaultClient>(MockBehavior.Strict);
            clientMock.Setup(x => x.V1)
                .Returns(() => clientV1Mock.Object);

            var extractor = new VaultSecretExtractor(clientMock.Object, new Uri(rootUrl));

            var result = await extractor.GetSecretValue(key);
            
            Assert.AreEqual(value, result);
            
            keyValueV2Mock.VerifyAll();
            keyValueMock.VerifyAll();
            secretsMock.VerifyAll();
            clientV1Mock.VerifyAll();
            clientMock.VerifyAll();
        }
    }
}