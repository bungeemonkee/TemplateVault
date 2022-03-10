using System;
using System.Threading.Tasks;
using TemplateVault.Utilities;
using TemplateVault.Vault;

namespace TemplateVault
{
    public static class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var console = new AbstractConsole();
                var file = new AbstractFile();
                var authFactory = new VaultAuthFactory(console);
                var clientFactory = new VaultClientFactory();
                var secretExtractorFactory = new VaultSecretExtractorFactory(clientFactory);

                var logic = new Logic(console, file, authFactory, secretExtractorFactory);

                return await logic.Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unhandled error: {0}", ex.Message);
            }

            return -1;
        }
    }
}