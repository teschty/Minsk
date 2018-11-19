using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public virtual TextSpan Span {
            get 
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;

                return TextSpan.FromBounds(first.Start, last.End);
            }
        }

        public IEnumerable<SyntaxNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(prop.PropertyType))
                {
                    yield return (SyntaxNode)prop.GetValue(this);
                }
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(prop.PropertyType))
                {
                    var children = (IEnumerable<SyntaxNode>)prop.GetValue(this);
                    foreach (var child in children)
                        yield return child;
                }
            }
        }
    }
}
