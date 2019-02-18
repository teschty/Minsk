using System.Collections.Immutable;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Lowering
{
   internal sealed class Lowerer : BoundTreeRewriter
   {
      private Lowerer()
      {

      }

      public static BoundStatement Lower(BoundStatement statement)
      {
         var lowerer = new Lowerer();
         return lowerer.RewriteStatement(statement);
      }

      protected override BoundStatement RewriteForStatement(BoundForStatement node)
      {
         // for i = <lower> to <upper>
         //     <body>
         //
         // ------>
         // {
         //
         //     var <var> = <lower>
         //     while <var> <= <upper>
         //     {
         //          <body>
         //          <var> = <var> + 1
         //     }
         // }

         var varDecl = new BoundVariableDeclaration(node.Variable, node.LowerBound);
         var varExpression = new BoundVariableExpression(node.Variable);

         var condition = new BoundBinaryExpression(
            varExpression,
            BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
            node.UpperBound);

         var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(node.Variable,
               new BoundBinaryExpression(
                  new BoundVariableExpression(node.Variable),
                  BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                  new BoundLiteralExpression(1)
               )
            )
         );

         var whileBlock = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));        
         var whileStatement = new BoundWhileStatement(condition, whileBlock);
         var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(varDecl, whileStatement));

         return RewriteStatement(result);
      }
   } 
}