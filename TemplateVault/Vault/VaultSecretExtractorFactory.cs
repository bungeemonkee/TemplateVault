using System;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Vault
{
    public class VaultSecretExtractorFactory : IVaultSecretExtractorFactory
    {
        private readonly IVaultClientFactory _clientFactory;
        
        public VaultSecretExtractorFactory (IVaultClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        
        public IVaultSecretExtractor GetVaultSecretExtractor(IAuthMethodInfo auth, Uri vaultRoot)
        {
            var client = _clientFactory.GetVaultClient(auth, vaultRoot);
            return new VaultSecretExtractor(client, vaultRoot);
        }
    }
}