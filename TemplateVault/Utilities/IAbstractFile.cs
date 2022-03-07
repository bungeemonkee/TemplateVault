using System.Threading.Tasks;

namespace TemplateVault.Utilities
{
    public interface IAbstractFile
    {
        Task<string> ReadAllTextAsync(string path);
        Task WriteAllTextAsync(string path, string contents);
        bool Exists(string path);
    }
}