using System.Threading.Tasks;
using TemplateVault.Utilities;
using TemplateVault.Vault;

namespace TemplateVault
{
    public static class Program
    {
        static Task<int> Main(string[] args)
        {
            var console = new AbstractConsole();
            var file = new AbstractFile();
            var authFactory = new VaultAuthFactory(console);
            var clientFactory = new VaultClientFactory();
            var secretExtractorFactory = new VaultSecretExtractorFactory(clientFactory);

            var logic = new Logic(console, file, authFactory, secretExtractorFactory);

            return logic.Run(args);
        }
    }
}