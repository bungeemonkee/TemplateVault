using System;

namespace TemplateVault
{
    public interface IAbstractConsole
    {
        void Write(string fmt, params object[] args);
        void WriteLine();
        void WriteLine(string fmt, params object[] args);
        void WriteError(string fmt, params object[] args);
        void WriteErrorLine(string fmt, params object[] args);
        string? ReadLine();
        ConsoleKeyInfo ReadKey(bool intercept);
    }
}