using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount = 0;

        private Lowerer()
        {

        }

        private BoundLabel GenerateLabel(string label = "Label")
        {
            var name = $"{label}{++_labelCount}";
            return new BoundLabel(name);
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
                }
                else
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
            var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
            var upperBoundDecl = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);

            var condition = new BoundBinaryExpression(
               varExpression,
               BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
               new BoundVariableExpression(upperBoundSymbol)
            );

            var increment = new BoundExpressionStatement(
               new BoundAssignmentExpression(node.Variable,
                  new BoundBinaryExpression(
                     varExpression,
                     BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                     new BoundLiteralExpression(1)
                  )
               )
            );

            var whileBlock = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBlock);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
               varDecl, upperBoundDecl, whileStatement
            ));

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

            var gotoCheck = new BoundGotoStatement(checkLabel);
            var continueLabelStatement = new BoundLabelStatement(continueLabel);
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
               gotoCheck,
               continueLabelStatement,
               node.Body,
               checkLabelStatement,
               gotoTrue
            ));

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            // do
            //      <body>
            // while <condition>
            //
            // ----->
            //
            // continue:
            // <body>
            // gotoTrue <condition> continue
            //

            var continueLabel = GenerateLabel();

            var continueLabelStatement = new BoundLabelStatement(continueLabel);
            var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
               continueLabelStatement,
               node.Body,
               gotoTrue
            ));

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = GenerateLabel("EndIf");
                var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
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

                var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
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