using CommandLine;

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
    }
}
