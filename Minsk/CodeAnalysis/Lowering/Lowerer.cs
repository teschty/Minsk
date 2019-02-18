using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Lowering
{
   internal sealed class Lowerer : BoundTreeRewriter
   {
      private int _labelCount = 0;

      private Lowerer()
      {

      }

      private LabelSymbol GenerateLabel(string label = "Label")
      {
         var name = $"{label}{++_labelCount}";
         return new LabelSymbol(name);
      }

      public static BoundBlockStatement Lower(BoundStatement statement)
      {
         var lowerer = new Lowerer();
         var result = lowerer.RewriteStatement(statement);

         return Flatten(result);
      }

      private static BoundBlockStatement Flatten(BoundStatement statement)
      {
         var builder = ImmutableArray.CreateBuilder<BoundStatement>();
         var stack = new Stack<BoundStatement>();
         stack.Push(statement);

         while (stack.Count > 0)
         {
            var current = stack.Pop();

            if (current is BoundBlockStatement block)
            {
               foreach (var s in block.Statements.Reverse())
                  stack.Push(s);
            } else 
            {
               builder.Add(current);
            }
         }

         return new BoundBlockStatement(builder.ToImmutable());
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

      protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
      {
         // while <condition>
         //    <body>
         //
         // --->
         // 
         // goto check
         // continue:
         //    <body>
         // check:
         //    gotoTrue <condition> continue
         // end:
         // 

         var continueLabel = GenerateLabel("Continue");
         var checkLabel = GenerateLabel("Check");
         var endLabel = GenerateLabel("EndWhile");

         var gotoCheck = new BoundGotoStatement(checkLabel);
         var continueLabelStatement = new BoundLabelStatement(continueLabel);
         var checkLabelStatement = new BoundLabelStatement(checkLabel);
         var endLabelStatement = new BoundLabelStatement(endLabel);
         var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition);

         var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
            gotoCheck,
            continueLabelStatement,
            node.Body,
            checkLabelStatement,
            gotoTrue,
            endLabelStatement
         ));

         return RewriteStatement(result);
      }
      
      protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
      {
         if (node.ElseStatement == null) 
         {
            var endLabel = GenerateLabel("EndIf");
            var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
               gotoFalse, node.ThenStatement, endLabelStatement
            ));

            return RewriteStatement(result);
         } 
         else 
         {
            var endLabel = GenerateLabel("EndIf");
            var elseLabel = GenerateLabel("IfElse");

            var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, true);
            var gotoEndStatement = new BoundGotoStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
               gotoFalse, node.ThenStatement, gotoEndStatement, elseLabelStatement, node.ElseStatement, endLabelStatement
            ));

            return RewriteStatement(result);
         }
      }
   } 
}