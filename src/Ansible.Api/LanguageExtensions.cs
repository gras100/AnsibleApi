using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;


namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        // https://github.com/dotnet/aspnet-api-versioning/blob/3fc071913dcded23eeb5ebe55bca44f3828488bf/src/Common/src/Common.Backport/CallerArgumentExpressionAttribute.cs#L7
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}