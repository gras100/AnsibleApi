using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using CallerMemberNameAttribute = System.Runtime.CompilerServices.CallerMemberNameAttribute;


namespace plenidev.Ansible.Api
{
    public class AnsibleExtraVarNameOptions
    {
        public static readonly AnsibleExtraVarNameOptions Default = new();

        public bool SeparateNumbers { get; init; } = true;
        public bool AllowSingleCharacterPrefix { get; init; } = false;
        public bool ConvertToLower { get; init; } = true;
    }

    /// <summary>
    /// <para>
    /// Represents an ansible extra-variable with-in ExtraVarsBase derived classes.
    /// </para><para>
    /// Converts given names to lower-snake-case then validates against Ansible's variable 
    /// specification regex of <c>[a-z_][0-9a-z_]*</c>.
    /// </para><para>
    /// Provides <c>ToJsonString()</c> for easy conversion to a <c>"name": "escaped_value"</c>
    /// JSON fragment.
    /// </para>
    /// </summary>
    public sealed class AnsibleExtraVar
    {
        private readonly bool _setOnce;

        private bool _readonly;
        private readonly string _name;
        private string? _value;

        #region Construction

        internal AnsibleExtraVar(string name, bool setOnce, AnsibleExtraVarNameOptions? opts = null)
        {
            _name = ToAnsibleNameChecked(name, opts ?? AnsibleExtraVarNameOptions.Default);
            _value = null;
            _setOnce = setOnce;
        }

        #endregion

        #region Properties

        public string Name => _name;

        public string Value
        {
            get => _value! == null ? throw new InvalidOperationException("Value must be set before getting.") : _value;
            set => _value = CheckedValue(value);
        }

        internal AnsibleExtraVar WithReadOnly(bool value)
        {
            _readonly = value;
            return this;
        }

        static public AnsibleExtraVar operator +(AnsibleExtraVar inst, string value)
        {
            if (inst._value == null)
            {
                inst._value = value;
            }
            else
            {
                inst.Value = inst._value + value;
            }
            return inst;
        }

        #endregion

        #region Checking/Validation

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

        private string? CheckedValue(string value)
        {
            if(value == null) throw new ArgumentNullException("value");
            if (_setOnce && _value != null) throw new InvalidOperationException("Value has already been set.");
            if (_readonly) throw new InvalidOperationException("Class is readonly.");
            return value;
        }

        private string ToAnsibleNameChecked(string name, AnsibleExtraVarNameOptions options)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            var ansName = ToAnsibleNameUnchecked(name, options);
            if (!finalNameValidationRe.IsMatch(ansName) || pythonKeywords.Contains(ansName))
            {
                throw new ArgumentException(
                    $"Ansible name << {ansName} >> for key << {name} >> must match << {finalNameValidationRe} >> and not be a python keyword.",
                    nameof(name)
                    );
            }
            return ansName;
        }

        private string ToAnsibleNameUnchecked(string name, AnsibleExtraVarNameOptions style)
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
            for(int i = lb; i < name.Length; i++)
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

        #region Json Composition

        public string ToJsonString(bool compact = true)
        {
            var assign = compact ? ":" : ": ";
            var escapedValue = JsonEscaped(Value, quoted: false);
            return $"\"{Name}\"{assign}\"{escapedValue}\"";
        }

        private object JsonEscaped(string value, bool quoted)
        {
            var sb = new StringBuilder();
            foreach(var c in value.ToCharArray())
            {
                switch (c)
                {
                    case  '"': sb.Append(@"\"""); break;
                    case '\\': sb.Append(@"\"""); break;
                    case  '/': sb.Append(@"\"""); break;
                    case '\t': sb.Append(@"\"""); break;
                    case '\r': sb.Append(@"\"""); break;
                    case '\n': sb.Append(@"\"""); break;
                    case '\b': sb.Append(@"\"""); break;
                    case '\f': sb.Append(@"\"""); break;
                    default: sb.Append(c); break;
                }
            }
            return quoted ? $"\"{sb}\"" : $"{sb}";
        }

        #endregion

    }

    /// <summary>
    /// <para>
    /// Base for ExtraVars classes to give dot access to Ansible ExtraVar items.
    /// </para><para>
    /// Usage:
    /// </para><para>
    /// <c><para>MyExtraVars: ExtraVarsBase&lt;MyExtraVars>{</para>
    /// <para>...</para>
    /// <para>  public ExtraVar Prop => BaseGet(nameof(Prop));</para>
    /// <para>  public ExtraVar Prop => BaseSet(nameof(Prop), value);</para>
    /// <para>etc.</para>
    /// <para>...</para>
    /// <para>}</para></c>
    /// </para><para>
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnsibleExtraVarsBase<T> where T: AnsibleExtraVarsBase<T>
    {
        private readonly Dictionary<string, AnsibleExtraVar> _data = [];

        private readonly AnsibleExtraVarNameOptions _options;
        private readonly bool _setOnce = false;

        #region Readonly

        private bool _readonly = false;

        public void SetReadonly() => _readonly = true;

        public T AsReadonlyChecked
        {
            get
            {
                if (_readonly) throw new InvalidOperationException("Readonly already.");
                return AsReadonlyChecked;
            }
        }

        public T AsReadonly
        {
            get
            {
                _readonly = true;
                return (T)this;
            }
        }

        #endregion

        #region Construction

        protected AnsibleExtraVarsBase(bool setOnce = true, AnsibleExtraVarNameOptions? options = null) 
        { 
            _setOnce = setOnce; 
            _options = options ?? AnsibleExtraVarNameOptions.Default;
        }

        #endregion

        #region Get Definers

        /// <summary>
        /// <para>
        /// Use to define getters as
        /// </para>
        /// <para>
        /// <c>get => BaseGet();</c>
        /// </para>
        /// </summary>
        /// <returns></returns>
        protected AnsibleExtraVar BaseGet([CallerMemberName] string key = "")
        {
            return BaseGetForKey(key);
        }

        /// <summary>
        /// <para>
        /// Use to define getters as
        /// </para>
        /// <para>
        /// <c>get => BaseGet(nameof(&lt;DefiningProperty>));</c>
        /// </para>
        /// </summary>
        /// <param name="key">nameof(&lt;DefiningProperty>)</param>
        /// <returns></returns>
        protected AnsibleExtraVar BaseGetForKey(string key)
        { 
            return GetItemAddIfNeeded(key);
        }

        private AnsibleExtraVar GetItemAddIfNeeded(string key)
        {
            if(_data.TryGetValue(key, out var value)) { 
                return value; 
            }

            if (_readonly) throw new InvalidOperationException("Readonly additions not allowed.");

            value = new AnsibleExtraVar(key, _setOnce, _options);
            _data.Add(key, value);
            return value;

        }

        #endregion

        #region Set Definers

        /// <summary>
        /// <para>
        /// Derived classes wishing to support both
        /// </para><para>
        /// <c>Inst.Property += "value";</c>
        /// </para><para>and</para><para>
        /// <c>Inst.Property = new AnsibleExtraVar(...)</c>
        /// </para><para>
        /// syntax should use this method to define properties as
        /// </para><para>
        /// <c>set => BaseSet(nameof(&lt;property>), value);</c>
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void BaseSet(AnsibleExtraVar value, [CallerMemberName] string key = "")
        {
            BaseSet(key, value);
        }

        /// <summary>
        /// <para>
        /// Derived classes wishing to support both
        /// </para><para>
        /// <c>Inst.Property += "value";</c>
        /// </para><para>and</para><para>
        /// <c>Inst.Property = new AnsibleExtraVar(...)</c>
        /// </para><para>
        /// syntax should use this method to define properties as
        /// </para><para>
        /// <c>set => BaseSet(nameof(&lt;property>), value);</c>
        /// </para>
        /// </summary>
        /// <param name="key">nameof(&tl;property>)</param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void BaseSet(string key, AnsibleExtraVar value)
        {
            var inst = _data[key];
            if(inst.Name != value.Name)
            {
                throw new InvalidOperationException($"Names are read-only.");
            }
            if(inst.Value == value.Value)
            {
                // avoids triggering setOnce logic when += has 
                // been used.
                return;
            }

            inst.Value = value.Value;
        }

        /// <summary>
        /// <para>
        /// Derived classes wishing to support both
        /// </para><para>
        /// <c>Inst.Property += "value";</c>
        /// </para><para>and</para><para>
        /// <c>Inst.Property = new AnsibleExtraVar(...)</c>
        /// </para><para>
        /// syntax should use this method to define properties as
        /// </para><para>
        /// <c>set => BaseSetConcatOnly(nameof(&lt;property>), value);</c>
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void BaseSetConcatOnly(AnsibleExtraVar value, [CallerMemberName] string key = "")
        {
            BaseSetConcatOnly(key, value);
        }

        /// <summary>
        /// <para>
        /// Derived classes wishing to support both
        /// </para><para>
        /// <c>Inst.Property += "value";</c>
        /// </para><para>and</para><para>
        /// <c>Inst.Property = new AnsibleExtraVar(...)</c>
        /// </para><para>
        /// syntax should use this method to define properties as
        /// </para><para>
        /// <c>set => BaseSetConcatOnly(nameof(&lt;property>), value);</c>
        /// </para>
        /// </summary>
        /// <param name="key">nameof(&lt;property>)</param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void BaseSetConcatOnly(string key, AnsibleExtraVar value)
        {
            var inst = _data[key];
            if (inst.Name != value.Name)
            {
                throw new InvalidOperationException($"Names are read-only.");
            }
            if (inst.Value != value.Value)
            {
                throw new InvalidOperationException($"Values must be set using += <string_value> or Value property directly.");
            }
        }

        #endregion

        #region JSON Composition

        public string ToJsonString(bool compact = true)
        {
            const char open = '{';
            const char close = '}';
            var space = compact ? "" : " ";
            var comma = compact ? "," : ", ";

            var sep = space;
            var sb = new StringBuilder($"{open}{space}\"extra_vars\":{space}{open}");
            foreach (var kv in _data) 
            {
                sb.Append(sep).Append(kv.Value.ToJsonString(compact));
                sep = comma;
            }
            return sb.Append($"{space}{close}{space}{close}").ToString();
        }

        #endregion

    }
}
