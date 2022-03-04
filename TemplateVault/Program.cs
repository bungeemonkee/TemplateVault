using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using VaultSharp.Core;

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
            if (vaultAuthFactory.GetSupportedAuthTypes().All(x => x.name != options.AuthType))
            {
                console.WriteErrorLine("Unsupported auth type: {0}", options.AuthType);
                console.WriteErrorLine("Valid authentication types are:");
                foreach (var auth in vaultAuthFactory.GetSupportedAuthTypes())
                {
                    var padding = new string(' ', 20 - auth.name.Length);
                    console.WriteErrorLine("  --auth {0}{1}{2}", auth.name, padding, auth.description);
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
            var vaultAuth = vaultAuthFactory.GetAuth(options.AuthType, options.AuthMount);
            
            // get the vault secret extractor
            var secretExtractor = new VaultSecretExtractor(vaultAuth, vaultRoot);

            var variableValues = new Dictionary<string, string>();
            foreach (var variable in variables)
            {
                try
                {
                    var secretValue = await secretExtractor.GetSecretValue(variable);
                    if (secretValue == null)
                    {
                        console.WriteErrorLine("No secret found at path {1}", variable);
                        return -1;
                    }

                    variableValues[variable] = secretValue;
                } catch (VaultApiException e)
                {
                    console.WriteErrorLine("Failed to get secret {0}: {1}", variable, e.Message.Trim());
                    return 1;
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
    }
}