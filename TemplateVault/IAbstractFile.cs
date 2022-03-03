using System.Threading.Tasks;

namespace TemplateVault
{
    public interface IAbstractFile
    {
        Task<string> ReadAllTextAsync(string path);
        Task WriteAllTextAsync(string path, string contents);
    }
}