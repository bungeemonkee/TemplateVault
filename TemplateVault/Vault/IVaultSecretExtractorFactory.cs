using System;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Vault
{
    public interface IVaultSecretExtractorFactory
    {
        IVaultSecretExtractor GetVaultSecretExtractor(IAuthMethodInfo auth, Uri vaultRoot);
    }
}