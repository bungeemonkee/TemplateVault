using CommandLine;

namespace TemplateVault
{
    public class CommandLineOptions
    {
        [Value(index:0, Required = true, HelpText = "The template file to process.")]
        public string InputFile { get; set; }
        
        [Value(index:1, Required = false, HelpText = "The output to put the processed template result into.")]
        public string OutputFile { get; set; }
        
        [Option(shortName:'a', longName: "auth", Required = false, Default="userpass", HelpText = "The kind of authentication to use.")]
        public string AuthType { get; set; }
    }
}
