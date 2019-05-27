using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk
{
    internal static class Program
    {
        public static void Main()
        {
            var repl = new MinskRepl();
            repl.Run();
        }
    }
}
