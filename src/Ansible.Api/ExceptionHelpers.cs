using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;


namespace plenidev.Common
{
    public static class ExceptionHelpers
    {
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument == null) ThrowArgumentNullException(paramName);
        }

        public static void ThrowIfNullOrEmpty(
            [NotNull] string? argument,
            [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (String.IsNullOrEmpty(argument)) ThrowArgumentNullException(paramName);
        }

        public static void ThrowIfNullOrWhitespace(
            [NotNull] string? argument,
            [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (String.IsNullOrWhiteSpace(argument)) ThrowArgumentNullException(paramName);
        }

        public static void ThrowArgumentNullException(string? paramName) =>
            throw new ArgumentNullException(paramName);

        public static void ThrowIf(
            bool condition, 
            [CallerArgumentExpression(nameof(condition))] string? message = null)
        {
            if (condition) ThrowInvalidOperationException(message!);
        }

        public static void ThrowInvalidOperationException(string message) => 
            throw new InvalidOperationException(message);

    }
    
}
