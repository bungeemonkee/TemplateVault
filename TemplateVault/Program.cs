using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using TemplateVault.Utilities;
using TemplateVault.Vault;
using VaultSharp.Core;

namespace TemplateVault
{
    public class Program
    {
        private readonly IAbstractConsole _console;
        private readonly IAbstractFile _file;
        private readonly IVaultAuthFactory _authFactory;
        private readonly IVaultSecretExtractorFactory _secretExtractorFactory;
        
        static Task<int> Main(string[] args)
        {
            var console = new AbstractConsole();
            var file = new AbstractFile();
            var authFactory = new VaultAuthFactory(console);
            var clientFactory = new VaultClientFactory();
            var secretExtractorFactory = new VaultSecretExtractorFactory(clientFactory);

            var program = new Program(console, file, authFactory, secretExtractorFactory);

            return program.Run(args);
        }

        public Program(IAbstractConsole console, IAbstractFile file, IVaultAuthFactory authFactory, IVaultSecretExtractorFactory secretExtractorFactory)
        {
            _console = console;
            _file = file;
            _authFactory = authFactory;
            _secretExtractorFactory = secretExtractorFactory;
        }

        public async Task<int>Run(string[] args)
        {
            var options = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(x => {
                    if (string.IsNullOrWhiteSpace(x.AuthMount))
                    {
                        // clean any whitespace or empty strings into null
                        x.AuthMount = null;
                    }
                } )
                .MapResult(x => x, x => null!);

            if (options == null)
            {
                return 1;
            }

            // make sure the auth type is supported
            if (_authFactory.GetSupportedAuthTypes().All(x => x.name != options.AuthType))
            {
                _console.WriteErrorLine("Unsupported auth type: {0}", options.AuthType);
                _console.WriteErrorLine("Valid authentication types are:");
                foreach (var auth in _authFactory.GetSupportedAuthTypes())
                {
                    var padding = new string(' ', 20 - auth.name.Length);
                    _console.WriteErrorLine("  --auth {0}{1}{2}", auth.name, padding, auth.description);
                }
                return 1;
            }

            // if the output file is not defined generate it from the input file name
            if (string.IsNullOrWhiteSpace(options.OutputFile))
            {
                options.OutputFile = options.InputFile
                    .Replace(".tmpl", "")
                    .Replace(".tpl", "");
            }
            
            _console.WriteLine("Using template file: {0}", options.InputFile);
            _console.WriteLine("Using output file: {0}", options.OutputFile);
            
            // detect if the output file exists
            if (!options.Yes && _file.Exists(options.OutputFile))
            {
                _console.Write("Output file ({0}) already exists. Overwrite? (y/n): ", options.OutputFile);
                var overwrite = _console.ReadLine();

                if (!string.Equals("y", overwrite, StringComparison.InvariantCultureIgnoreCase))
                {
                    return 0;
                }
            }
            
            string template;
            try
            {
                template = await _file.ReadAllTextAsync(options.InputFile);
            }
            catch (IOException)
            {
                _console.WriteErrorLine("Unable to read template file: {0}", options.InputFile);
                return 1;
            }

            Uri? vaultRoot = null;
            var variables = ExtractTemplateVariables(template);

            if (variables.Length == 0)
            {
                _console.WriteErrorLine("No variables found in template.");
                return 1;
            }

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
                    _console.WriteErrorLine("Failed to parse VAULTROOT uri: {0}", vaultUri);
                    return -1;
                }

                // remove the Vault root from the variable list
                variables = variables[1..];
            }

            if (vaultRoot == null)
            {
                _console.WriteErrorLine("No VAULTROOT variable found in template.");
                return 1;
            }

            _console.WriteLine("Using VAULTROOT uri: {0}", vaultRoot.AbsoluteUri);

            _console.WriteLine("Found secret paths:");
            foreach (var variable in variables)
            {
                _console.WriteLine("  * {0}", variable);
            }

            // get the login for use with Vault
            var vaultAuth = _authFactory.GetAuth(options.AuthType, options.AuthMount);
            
            // get the vault secret extractor
            var secretExtractor = _secretExtractorFactory.GetVaultSecretExtractor(vaultAuth, vaultRoot);

            var variableValues = new Dictionary<string, string>();
            foreach (var variable in variables)
            {
                try
                {
                    var secretValue = await secretExtractor.GetSecretValue(variable);
                    if (secretValue == null)
                    {
                        _console.WriteErrorLine("No secret found at path {1}", variable);
                        return -1;
                    }

                    variableValues[variable] = secretValue;
                } catch (VaultApiException e)
                {
                    _console.WriteErrorLine("Failed to get secret {0}: {1}", variable, e.Message.Trim());
                    return 1;
                }
            }

            var result = template;
            foreach (var variable in variableValues)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }
            
            _console.WriteLine("Saving result to {0}", options.OutputFile);

            try
            {
                await _file.WriteAllTextAsync(options.OutputFile, result);
            }
            catch (IOException)
            {
                _console.WriteErrorLine("Unable to write result file: {0}", options.OutputFile);
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
    }
}