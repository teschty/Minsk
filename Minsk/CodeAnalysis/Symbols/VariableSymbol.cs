using System;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        internal TypeSymbol(string name) : base(name) { }

        public override SymbolKind Kind => SymbolKind.Type;
    }

    public sealed class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, bool isReadOnly, Type type) : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
        }

        public bool IsReadOnly { get; }
        public Type Type { get; }
        public override SymbolKind Kind => SymbolKind.Variable;
    }
}
