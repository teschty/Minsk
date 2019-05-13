namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        //Statements
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        GotoStatement,
        LabelStatement,
        ConditionalGotoStatement,

        // Expressions
        ErrorExpression,
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
        CallExpression,
    }
}
