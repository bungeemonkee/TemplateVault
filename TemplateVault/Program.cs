using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.Commons;

namespace TemplateVault
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var console = new AbstractConsole();
            var file = new AbstractFile();
            var vaultAuthFactory = new VaultAuthFactory(console);
            
            var options = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(x => x, x => null!);

            if (options == null)
            {
                return 1;
            }

            // make sure the auth type is supported
            if (!vaultAuthFactory.GetSupportedAuthTypes().Contains(options.AuthType))
            {
                console.WriteErrorLine("Unsupported auth type: {0}", options.AuthType);
                return 1;
            }

            // if the output file is not defined generate it from the input file name
            if (string.IsNullOrWhiteSpace(options.OutputFile))
            {
                options.OutputFile = options.InputFile
                    .Replace(".tmpl", "")
                    .Replace(".tpl", "");
            }

            console.WriteLine("Using template file: {0}", options.InputFile);
            
            string template;
            try
            {
                template = await file.ReadAllTextAsync(options.InputFile);
            }
            catch (IOException)
            {
                console.WriteErrorLine("Unable to read template file: {0}", options.InputFile);
                return 1;
            }

            Uri? vaultRoot = null;
            var variables = ExtractTemplateVariables(template);
            if (variables[0].StartsWith("VAULTROOT:", StringComparison.InvariantCultureIgnoreCase))
            {
                var vaultUri = variables[0]
                    .Substring(10)
                    .Trim();

                try
                {
                    vaultRoot = new Uri(vaultUri);
                }
                catch (IOException)
                {
                    console.WriteErrorLine("Failed to parse VAULTROOT uri: {0}", vaultUri);
                    return -1;
                }

                // remove the Vault root from the variable list
                variables = variables[1..];
            }

            if (vaultRoot == null)
            {
                console.WriteErrorLine("No VAULTROOT variable found in template.");
                return 1;
            }

            console.WriteLine("Using VAULTROOT uri: {0}", vaultRoot.AbsoluteUri);

            console.WriteLine("Found secret paths:");
            foreach (var variable in variables)
            {
                console.WriteLine("  * {0}", variable);
            }

            // get the login for use with Vault
            var vaultAuth = vaultAuthFactory.GetAuth(options.AuthType);

            // get the vault object
            var vaultRootNoPath = new Uri(vaultRoot, "/");
            var vaultSettings = new VaultClientSettings(vaultRootNoPath.ToString(), vaultAuth);
            var vault = new VaultClient(vaultSettings);

            var variableValues = new Dictionary<string, string>();
            foreach (var variable in variables)
            {
                // separate the path and the variable name
                var (variableMount, variablePath, variableName) = ExtractSecretPathParts(vaultRoot, variable);
                if (variableMount == null || variablePath == null || variableName == null)
                {
                    console.WriteErrorLine("Unable to extract mount, path, and name for variable: {0}", variable);
                }
                
                // get the secret at that path
                Secret<SecretData> secret;
                try
                {
                    secret = await vault.V1.Secrets.KeyValue.V2.ReadSecretAsync(variablePath, mountPoint:variableMount);
                }
                catch (VaultApiException e)
                {
                    console.WriteErrorLine("Failed to get secret {0}: {1}", variable, e.Message.Trim());
                    console.WriteErrorLine("  * Mount: {0}", variableMount);
                    console.WriteErrorLine("  * Path:  {0}", variablePath);
                    console.WriteErrorLine("  * Name:  {0}", variableName);
                    return 1;
                }

                // get the actual variable value
                if (secret.Data.Data.TryGetValue(variableName!, out var variableValue))
                {
                    variableValues[variable] = variableValue?.ToString() ?? string.Empty;
                }
                else
                {
                    console.WriteErrorLine("Secret {0} not found at path {1}", variableName, variablePath);
                    return -1;
                }
            }

            var result = template;
            foreach (var variable in variableValues)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }
            
            console.WriteLine("Saving result to {0}", options.OutputFile);

            try
            {
                await file.WriteAllTextAsync(options.OutputFile, result);
            }
            catch (IOException)
            {
                console.WriteErrorLine("Unable to write result file: {0}", options.OutputFile);
            }

            // done, success
            return 0;
        }

        private static string[] ExtractTemplateVariables(string template)
        {
            const string regex = @"(?<={{).+(?=}})";

            var variables = Regex.Matches(template, regex);

            return variables
                .Select(x => x.Captures)
                .SelectMany(x => x)
                .Select(x => x.Value)
                .Distinct()
                .ToArray();
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