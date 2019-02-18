using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

        public IEnumerable<BoundNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(prop.PropertyType))
                {
                    var child = (BoundNode)prop.GetValue(this);

                    if (child != null)
                        yield return (BoundNode)prop.GetValue(this);
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType))
                {
                    var children = (IEnumerable<BoundNode>)prop.GetValue(this);
                    foreach (var child in children)
                        if (child != null)
                            yield return child;
                }
            }
        }

        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.Name == nameof(Kind) || prop.Name == nameof(BoundBinaryExpression.Op))
                    continue;
                    
                if (typeof(BoundNode).IsAssignableFrom(prop.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType))
                    continue;

                var value = prop.GetValue(this);
                if (value != null)
                    yield return (prop.Name, value);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }

        private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true)
        {
            var isToConsole = writer == Console.Out;
            var marker = isLast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(indent);
            writer.Write(marker);

            WriteNode(writer, node);
            WriteProperties(writer, node);

            if (isToConsole)
                Console.ResetColor();

            writer.WriteLine();

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == lastChild);
        }

        private static void WriteNode(TextWriter writer, BoundNode node)
        {
            // TODO: handle binary and unary expressions
            if (writer == Console.Out)
                Console.ForegroundColor = GetColor(node);

            var text = GetText(node);
            writer.Write(text);

            if (writer == Console.Out)
                Console.ResetColor();
        }

        private static void WriteProperties(TextWriter writer, BoundNode node)
        {
            var isFirstProp = true;

            if (writer == Console.Out)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            foreach (var p in node.GetProperties()) 
            {

                if (isFirstProp)
                    isFirstProp = false;
                else
                    writer.Write(",");

                writer.Write(" ");

                writer.Write(p.Name);
                writer.Write(" = ");
                writer.Write(p.Value);
            }

            if (writer == Console.Out)
                Console.ResetColor();
        }

        private static string GetText(BoundNode node)
        {
            if (node is BoundBinaryExpression b)
                return b.Op.Kind.ToString() + "Expression";

            if (node is BoundUnaryExpression u)
                return u.Op.Kind.ToString() + "Expression";

            return node.Kind.ToString();
        }

        private static ConsoleColor GetColor(BoundNode node)
        {
            if (node is BoundExpression)
                return ConsoleColor.Blue;

            if (node is BoundStatement)
                return ConsoleColor.Cyan;

            return ConsoleColor.Yellow;
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);

                return writer.ToString();
            }
        }
    }
}
