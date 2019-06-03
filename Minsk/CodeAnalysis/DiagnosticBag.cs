using System;
using System.Collections;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        internal void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            var message = $"The number {text} isn't a valid {type}.";
            Report(span, message);
        }

        internal void ReportBadCharacter(int position, char current)
        {
            var span = new TextSpan(position, 1);
            var message = $"bad character in input: '{current}'";
            Report(span, message);
        }

        public void ReportUnterminatedString(TextSpan span)
        {
            var message = "Unterminated string literal.";
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind expectedKind, SyntaxKind actualKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            var message = $"Unary operator '{operatorText}' is not defined for type '{operandType}'.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variable '{name}' doesn't exist.";
            Report(span, message);
        }

        internal void ReportUndefinedType(TextSpan span, string name)
        {
            var message = $"Type '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportSymbolAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"'{name}' is already declared.";
            Report(span, message);
        }

        public void ReportParameterAlreadyDeclared(TextSpan span, string parameterName)
        {
            var message = $"A parameter with the name '{parameterName}' was already declared.";
            Report(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        public void ReportCannotConvertImplicity(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(span, message);
        }

        public void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(span, message);
        }

        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            var message = $"Function '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
        {
            var message = $"Function '{name}' expects {expectedCount} argument(s), but was given {actualCount}.";
            Report(span, message);
        }

        public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            var message = $"Parameter '{name}' expects a value of type '{expectedType}', but was given a value of type '{actualType}'.";
            Report(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            var message = $"Expression must have a value.";
            Report(span, message);
        }

        public void ReportInvalidBreakOrContinue(TextSpan span, string text)
        {
            var message = $"The keyword '{text}' is only valid inside inside loops.";
            Report(span, message);
        }

        internal void ReportAllPathsMustReturn(TextSpan span)
        {
            var message = $"Not all code paths return a value.";
            Report(span, message);
        }

        public void ReportInvalidReturn(TextSpan span)
        {
            var message = $"The 'return' keyword may only be used inside of functions.";
            Report(span, message);
        }

        public void ReportInvalidReturnExpression(TextSpan span, string functionName)
        {
            var message = $"Since '{functionName}' returns void, the return keyword must not be followed by an expression.";
            Report(span, message);
        }

        public void ReportMissingReturnExpression(TextSpan span, string functionName, TypeSymbol returnType)
        {
            var message = $"An expression of type '{returnType}' expected.";
            Report(span, message);
        }
    }
}
