using System;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;

namespace TemplateVault.Vault
{
    public class VaultSecretExtractor
    {
        private readonly IAuthMethodInfo _auth;
        private readonly Uri _root;
        
        private IVaultClient? _client;
        
        public VaultSecretExtractor(IAuthMethodInfo auth, Uri vaultRoot)
        {
            _auth = auth;
            _root = vaultRoot;
            _client = null;
        }

        public async Task<string?> GetSecretValue(string path)
        {
            var client = GetClient();
            
            var (variableMount, variablePath, variableName) = ExtractSecretPathParts(_root, path);
            if (variableMount == null || variablePath == null || variableName == null)
            {
                return null;
            }
                
            // get the secret at that path
            var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(variablePath, mountPoint:variableMount);
           

            // get the actual variable value
            return secret.Data.Data.TryGetValue(variableName, out var variableValue)
                ? variableValue?.ToString()
                : null;
        }

        private IVaultClient GetClient()
        {
            if (_client != null)
            {
                return _client;
            }
            
            // create the vault client object
            var vaultRootNoPath = new Uri(_root, "/");
            var vaultSettings = new VaultClientSettings(vaultRootNoPath.ToString(), _auth);
            _client = new VaultClient(vaultSettings);

            return _client;
        }
        
        private static (string? mount, string? path, string? name) ExtractSecretPathParts(Uri vaultRoot, string path)
        {
            // combine the root uri with the path
            // ...get the absolute path that results
            // ...split it, removing empty path sections
            var parts = new Uri(vaultRoot, path)
                .AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
            {
                // somehow the path wasn't long enough to contain the necessary parts
                return (null, null, null);
            }

            // the path is everything except the first and last segments
            path = string.Join('/', parts[1..^1]);
            return (parts[0], path, parts[^1]);
        }
    }
}