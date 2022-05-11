using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace TemplateVault
{
    public class CommandLineOptions
    {
        [Value(index: 0, MetaName = "Template File", Required = true, HelpText = "The template file to process.")]
        public string InputFile { get; set; } = "appsettings.tmpl";

        [Value(index: 1, MetaName = "Output File", Required = false, HelpText = "The output to put the processed template result into.")]
        public string? OutputFile { get; set; }

        [Option(shortName: 'a', longName: "auth", Required = false, Default = "userpass", HelpText = "The kind of authentication to use.")]
        public string AuthType { get; set; } = "userpass";

        [Option('m', "auth-mount", Required = false, Default = null, HelpText = "Override the default auth mount point.")]
        public string? AuthMount { get; set; } = null;

        [Option('y', HelpText = "Overwrite the output file without asking if it already exists.")]
        public bool Yes { get; set; }
        
        [Option(longName:"no-escape", Required = false, Default = false, HelpText = "Do not escape '\"' and newline characters in the replaced values")]
        public bool NoEscape { get; set; }

        [Usage(ApplicationAlias = "TemplateVault")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal usage", new CommandLineOptions{InputFile = "settings.conf.tmpl"});
                yield return new Example("Override output file", new CommandLineOptions{InputFile = "settings.conf.tmpl", OutputFile = "generated_settings.conf"});
                yield return new Example("Specify authorization", new CommandLineOptions{InputFile = "settings.conf.tmpl", AuthType = "okta"});
                yield return new Example("Override authorization mount", new CommandLineOptions{InputFile = "settings.conf.tmpl", AuthType = "okta", AuthMount = "okta_mount"});
            }
        }
    }
}
