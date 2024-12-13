using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


using plenidev.AnsiblePlayer.Utils;

namespace plenidev.AnsiblePlayer.Core
{
    public class AnsibleNamingPolicyOptions
    {
        public static readonly AnsibleNamingPolicyOptions Default = new();

        /// <summary>
        /// <para>
        /// If true, '_',  when not already present, will be inserted before and 
        /// after numbers in the name being translated.
        /// </para><para>
        /// Examples (true vs false):
        /// </para>
        /// <para>
        /// <c>ab10cd -> ab_10_cd vs ab10cd</c>
        /// </para><para>
        /// <c>Ab10Cd -> ab_10_cd vs ab10_cd</c>
        /// </para>
        /// <para>
        /// <c>Ab_10Cd -> ab_10_cd vs ab_10_cd</c>
        /// </para>
        /// </summary>
        /// <remarks>Other options may effect this in some cases, e.g., if a number 
        /// appears as the 2nd character and AllowSingleCharacterPrefix is false.
        /// </remarks>
        public bool SeparateNumbers { get; init; } = true;

        /// <summary>
        /// <para>
        /// If true, '_' may be inserted in the second position of an Ansible name
        /// otherwise it can only come later.
        /// </para><para>
        /// Examples (true vs false):
        /// </para><para>
        /// <c>ABc -> a_bc  vs abc</c>
        /// </para><para>
        /// <c>a1c -> a_1_c vs a1_c</c>
        /// </para>
        /// </summary>
        public bool AllowSingleCharacterPrefix { get; init; } = false;

        /// <summary>
        /// If true (the default), resulting Ansible names will always be lower cased.
        /// </summary>
        public bool ConvertToLower { get; init; } = true;
    }

    internal sealed class AnsibleNamingPolicy(AnsibleNamingPolicyOptions style) : JsonNamingPolicy
    {
        public override string ConvertName(string name) => ToAnsibleNameChecked(name, style);

        #region Conversion

        // See: https://docs.ansible.com/ansible/latest/playbook_guide/playbooks_variables.html

        static private readonly HashSet<string> pythonKeywords =
            [
            "False",
            "None",
            "True",
            "and",
            "as",
            "assert",
            "async",
            "await",
            "break",
            "class",
            "continue",
            "def",
            "del",
            "elif",
            "else",
            "except",
            "finally",
            "for",
            "from",
            "global",
            "if",
            "import",
            "in",
            "is",
            "lambda",
            "nonlocal",
            "not",
            "or",
            "pass",
            "raise",
            "return",
            "try",
            "while",
            "with",
            "yield",
            "_",
            "case",
            "match",
            "type",
            ];

        private static readonly Regex finalNameValidationRe = new("^[A-Za-z_][0-9A-Za-z_]*$", RegexOptions.Compiled);

        private string ToAnsibleNameChecked(string name, AnsibleNamingPolicyOptions options)
        {
            ExceptionHelpers.ThrowIfNullOrEmpty(name);

            var ansibleName = ToAnsibleNameUnchecked(name, options);

            if (!finalNameValidationRe.IsMatch(ansibleName) || pythonKeywords.Contains(ansibleName))
            {
                ThrowOnBadAnsibleName(ansibleName, name, "key");
            }

            return ansibleName;
        }

        private void ThrowOnBadAnsibleName(string ansibleName, string name, string paramName)
        {
            throw new ArgumentException(
                $"Ansible name << {ansibleName} >> for key << {name} >> must match << {finalNameValidationRe} >> and not be a python keyword.",
                paramName
                );
        }

        private string ToAnsibleNameUnchecked(string name, AnsibleNamingPolicyOptions style)
        {
            // We are only interested in converting valid dotnet property names to snake case, 
            // so don't worry about symbols other than '_' turning up; the result will be validated
            // in the Checked function in anycase, and last resort devs can over-ride keys when 
            // defining properties in AnsibleExtraVarBase derived classes.

            bool IsUnderscorePoint(char pc, char c) =>
                c != '_' && pc != '_' && (
                    (Char.IsUpper(c)) || (
                    style.SeparateNumbers && Char.IsNumber(pc) != Char.IsNumber(c)
                    ));

            Debug.WriteLine($"# Starting for name: {name}");

            int lb = style.AllowSingleCharacterPrefix ? 1 : 2;
            var sb = new StringBuilder(name.Length * 2).Append(name.Substring(0, lb));
            for (int i = lb; i < name.Length; i++)
            {
                var pc = name[i - 1];
                var c = name[i];
                Console.WriteLine($"pc={pc}; c={c} ({pc}{c}");
                if (IsUnderscorePoint(pc, c))
                {
                    Debug.WriteLine($"  => \"{pc}_{c}\"");
                    sb.Append('_').Append(c);
                }
                else
                {
                    Debug.WriteLine("  => \"{pc}{c}\"");
                    sb.Append(c);
                }
                Debug.WriteLine($"  sb: \"{sb}\"");
            }
            if (style.ConvertToLower)
            {
                return sb.ToString().ToLowerInvariant();
            }

            return sb.ToString();
        }

        #endregion
    }
}
