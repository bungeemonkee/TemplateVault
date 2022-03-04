using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using TemplateVault.Utilities;

namespace TemplateVault.Tests
{
    [ExcludeFromCodeCoverage]
    public static class ConsoleMockExtensions
    {
        public static void SetupReadValue(this Mock<IAbstractConsole> console, params string?[] values)
        {
            console.Setup(x => x.Write(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<string>()));
            console.Setup(x => x.ReadLine())
                .Returns(new Queue<string?>(values).Dequeue);
        }

        public static void SetupReadSecureValue(this Mock<IAbstractConsole> console, params string[] values)
        {
            console.Setup(x => x.Write(It.IsNotNull<string>(), It.IsNotNull<string>()));
            console.Setup(x => x.ReadKey(true))
                .Returns(GetConsoleKeyQueue(values).Dequeue);
            console.Setup(x => x.WriteLine());
        }

        private static Queue<ConsoleKeyInfo> GetConsoleKeyQueue(string[] values)
        {
            var queue = new Queue<ConsoleKeyInfo>();
            foreach (var value in values)
            {
                foreach (var c in value)
                {
                    queue.Enqueue(new ConsoleKeyInfo(c, GetConsoleKey(c), false, false, false));
                }

                queue.Enqueue(new ConsoleKeyInfo((char)13, ConsoleKey.Enter, false, false, false));
            }

            return queue;
        }

        private static ConsoleKey GetConsoleKey(char c)
        {
            // special handling for backspace
            if (c == '\b')
            {
                return ConsoleKey.Backspace;
            }

            if (Enum.TryParse<ConsoleKey>(c.ToString().ToUpper(), out var val))
            {
                return val;
            }

            throw new InvalidOperationException("Failed to parse character into ConsoleKey");
        }
    }
}
