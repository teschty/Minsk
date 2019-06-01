using System;
using System.CodeDom.Compiler;
using System.IO;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.IO
{
    internal static class TextWriterExtensions
    {
        private static bool IsConsoleOut(this TextWriter writer)
        {
            if (writer == Console.Out)
                return true;

            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut())
                return true;

            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut())
                Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsoleOut())
                Console.ResetColor();
        }

        public static void WriteWithColor(this TextWriter writer, string text, ConsoleColor color)
        {
            writer.SetForeground(color);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
        {
            writer.WriteWithColor(SyntaxFacts.GetText(kind), ConsoleColor.Blue);
        }

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.WriteWithColor(text, ConsoleColor.Blue);
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.WriteWithColor(text, ConsoleColor.DarkYellow);
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.WriteWithColor(text, ConsoleColor.Cyan);
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.WriteWithColor(text, ConsoleColor.Magenta);
        }

        public static void WriteSpace(this TextWriter writer)
        {
            writer.WritePunctuation(" ");
        }

        public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
        {
            writer.WriteWithColor(SyntaxFacts.GetText(kind), ConsoleColor.DarkGray);
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.WriteWithColor(text, ConsoleColor.DarkGray);
        }
    }
}
