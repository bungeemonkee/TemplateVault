using System.Threading.Tasks;

namespace TemplateVault.Vault
{
    public interface IVaultSecretExtractor
    {
        Task<string?> GetSecretValue(string path);
    }
}