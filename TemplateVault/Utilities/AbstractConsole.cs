using System;

namespace TemplateVault.Utilities
{
    public class AbstractConsole : IAbstractConsole
    {
        public void Write(string fmt, params object[] args)
        {
            Console.Write(fmt, args);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
        }

        public void WriteError(string fmt, params object[] args)
        {
            Console.Error.Write(fmt, args);
        }

        public void WriteErrorLine(string fmt, params object[] args)
        {
            Console.Error.WriteLine(fmt, args);
        }

        public string? ReadLine()
        {
            return Console.ReadLine();
        }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return Console.ReadKey(intercept);
        }
    }
}
