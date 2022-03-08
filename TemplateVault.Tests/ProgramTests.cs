using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TemplateVault.Utilities;
using TemplateVault.Vault;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Tests
{
    [ExcludeFromCodeCoverage]
    public class ProgramTests
    {
        [Test]
        public async Task Run_SimpleSuccessCase()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "settings.conf";
            const string outputContents = "{{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_MultipleVariablesOnOneLine()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}} {{variable1}} {{variable2}}";
            const string? authMount = null;
            const string outputFile = "settings.conf";
            const string outputContents = "{{VAULTROOT: https://example.com}} value_1 value_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_MultipleVariablesNoWhitespace()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}}{{variable1}}{{variable2}}";
            const string? authMount = null;
            const string outputFile = "settings.conf";
            const string outputContents = "{{VAULTROOT: https://example.com}}value_1value_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_ContentBeforeVaultroot()
        {
            const int expectedReturn = 0;
            const string template = "This is the beginning of the file {{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "settings.conf";
            const string outputContents = "This is the beginning of the file {{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_GenerateDefaultOutputFile()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "settings.conf";
            const string outputContents = "{{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_SpecifyNonDefaultOutputFile()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "some_other_file.txt";
            const string outputContents = "{{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_OverwriteOutputFileForced()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "some_other_file.txt";
            const string outputContents = "{{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth", "-y" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_OverwriteOutputWithConfirmation()
        {
            const int expectedReturn = 0;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "some_other_file.txt";
            const string outputContents = "{{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            const bool outputExists = true;
            const string overwriteConfirmationInput = "y";
            var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));
            consoleMock.Setup(x => x.Write(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.ReadLine())
                    .Returns(overwriteConfirmationInput);

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Returns(Task.CompletedTask);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_OverwriteOutputConfirmationDenied()
        {
            const int expectedReturn = 0;
            const bool outputExists = true;
            const string overwriteConfirmationInput = "nope";
            var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.Write(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.ReadLine())
                    .Returns(overwriteConfirmationInput);

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            
            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_UnparsedOptions()
        {
            const int expectedReturn = 1;
            var args = Array.Empty<string>();
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            var secretFactory = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactory.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretFactory.VerifyAll();
        }
        
        [Test]
        public async Task Run_BadAuthOverride()
        {
            const int expectedReturn = 1;
            var args = new [] { "settings.conf.tmpl", "--auth", "bad_auth" };
            var authMethods = new[] { ("auth", "an auth method") };

            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                .Returns(authMethods);
            
            var secretFactory = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactory.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretFactory.VerifyAll();
        }
        
        [Test]
        public async Task Run_IOExceptionOnRead()
        {
            const int expectedReturn = 1;
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .Throws<IOException>();
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_IOExceptionOnWrite()
        {
            const int expectedReturn = 1;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const string outputFile = "some_other_file.txt";
            const string outputContents = "{{VAULTROOT: https://example.com}}\nvalue_1\nvalue_2";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", "value_1"), ("variable2", "value_2") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            fileMock.Setup(x => x.WriteAllTextAsync(It.Is<string>(y => y == outputFile), It.Is<string>(y => y == outputContents)))
                    .Throws<IOException>();

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }

        [Test]
        public async Task Run_NoVariablesFound()
        {
                const int expectedReturn = 1;
                const string template = "this should not result in any variables found";
                const bool outputExists = false;
                var args = new[] { "settings.conf.tmpl", "some_other_file.txt", "--auth", "auth" };
                var authMethods = new[] { ("auth", "an auth method") };

                var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
                consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
                consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>()));

                var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
                fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                        .Returns(outputExists);
                fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                        .ReturnsAsync(template);

                var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
                authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                        .Returns(authMethods);

                var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

                var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

                var result = await program.Run(args);

                Assert.AreEqual(expectedReturn, result);

                consoleMock.VerifyAll();
                fileMock.VerifyAll();
                authFactoryMock.VerifyAll();
                secretFactoryMock.VerifyAll();
        }

        [Test]
        public async Task Run_FailedToParseVaultRoot()
        {
            const int expectedReturn = 1;
            const string template = "{{VAULTROOT: this is not a url}}\n{{variable1}}\n{{variable2}}";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }

        [Test]
        public async Task Run_NoVaultRootFound()
        {
            const int expectedReturn = 1;
            const string template = "{{variable1}}\n{{variable2}}";
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_SecretNotFound()
        {
            const int expectedReturn = 1;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", (string?)null) };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                    .ReturnsAsync(variableValue);
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
        
        [Test]
        public async Task Run_VaultApiException()
        {
            const int expectedReturn = 1;
            const string template = "{{VAULTROOT: https://example.com}}\n{{variable1}}\n{{variable2}}";
            const string? authMount = null;
            const bool outputExists = false;
            var args = new[] { "settings.conf.tmpl", "settings.conf", "--auth", "auth" };
            var authMethods = new[] { ("auth", "an auth method") };
            var variables = new[] { ("variable1", (string?)null) };
            
            var consoleMock = new Mock<IAbstractConsole>(MockBehavior.Strict);
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>(), It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteLine(It.IsNotNull<string>()));
            consoleMock.Setup(x => x.WriteErrorLine(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<string>()));

            var fileMock = new Mock<IAbstractFile>(MockBehavior.Strict);
            fileMock.Setup(x => x.Exists(It.IsNotNull<string>()))
                    .Returns(outputExists);
            fileMock.Setup(x => x.ReadAllTextAsync(It.IsNotNull<string>()))
                    .ReturnsAsync(template);

            var authMock = new Mock<IAuthMethodInfo>(MockBehavior.Strict);
            
            var authFactoryMock = new Mock<IVaultAuthFactory>(MockBehavior.Strict);
            authFactoryMock.Setup(x => x.GetSupportedAuthTypes())
                    .Returns(authMethods);
            authFactoryMock.Setup(x => x.GetAuth(It.IsNotNull<string>(), authMount))
                    .Returns(() => authMock.Object);

            var secretExtractorMock = new Mock<IVaultSecretExtractor>(MockBehavior.Strict);
            foreach (var (variableName, variableValue) in variables)
            {
                    secretExtractorMock.Setup(x => x.GetSecretValue(It.Is<string>(y => y == variableName)))
                            .Throws<VaultApiException>();
            }

            var secretFactoryMock = new Mock<IVaultSecretExtractorFactory>(MockBehavior.Strict);
            secretFactoryMock.Setup(x => x.GetVaultSecretExtractor(It.Is<IAuthMethodInfo>(y => y == authMock.Object), It.IsNotNull<Uri>()))
                    .Returns(() => secretExtractorMock.Object);

            var program = new Program(consoleMock.Object, fileMock.Object, authFactoryMock.Object, secretFactoryMock.Object);

            var result = await program.Run(args);
            
            Assert.AreEqual(expectedReturn, result);
            
            consoleMock.VerifyAll();
            fileMock.VerifyAll();
            authMock.VerifyAll();
            authFactoryMock.VerifyAll();
            secretExtractorMock.VerifyAll();
            secretFactoryMock.VerifyAll();
        }
    }
}