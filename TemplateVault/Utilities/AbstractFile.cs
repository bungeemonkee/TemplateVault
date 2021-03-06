using System.IO;
using System.Threading.Tasks;

namespace TemplateVault.Utilities
{
    public class AbstractFile : IAbstractFile
    {
        public Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }

        public Task WriteAllTextAsync(string path, string contents)
        {
            return File.WriteAllTextAsync(path, contents);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}