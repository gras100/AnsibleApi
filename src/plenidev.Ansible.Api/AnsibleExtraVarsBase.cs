using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using CallerMemberNameAttribute = System.Runtime.CompilerServices.CallerMemberNameAttribute;


namespace plenidev.Ansible.Api
{
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

        internal AnsibleExtraVar(in string name, in bool setOnce)
        {
            _name = ToAnsibleNameChecked(name);
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

        internal AnsibleExtraVar WithReadOnly(in bool value)
        {
            _readonly = value;
            return this;
        }

        static public AnsibleExtraVar operator +(in AnsibleExtraVar inst, in string value)
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

        private static readonly Regex snakeCaseReplacer = new("([^_])([A-Z])", RegexOptions.Compiled);
        private static readonly string snakeCaseReplaceToken = "$1_$2";

        private static readonly Regex nameValidationRe = new("[a-z_][0-9a-z_]*", RegexOptions.Compiled);

        private string? CheckedValue(in string value)
        {
            if(value == null) throw new ArgumentNullException("value");
            if (_setOnce && _value != null) throw new InvalidOperationException("Value has already been set.");
            if (_readonly) throw new InvalidOperationException("Class is readonly.");
            return value;
        }

        private string ToAnsibleNameChecked(in string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            var ansName = ToAnsibleNameUnchecked(name);
            if (!nameValidationRe.IsMatch(ansName) || pythonKeywords.Contains(ansName))
            {
                throw new ArgumentException(
                    $"Must match regex << {nameValidationRe} >> and not be a python keyword << key: {name} >>.",
                    nameof(name)
                    );
            }
            return ansName;
        }

        private string ToAnsibleNameUnchecked(in string name)
        {
            return snakeCaseReplacer.Replace(name, snakeCaseReplaceToken);
        }

        #endregion

        #region Json Composition

        public string ToJsonString(in bool compact = true)
        {
            var assign = compact ? ":" : ": ";
            var escapedValue = JsonEscaped(Value, quoted: false);
            return $"\"{Name}\"{assign}\"{escapedValue}\"";
        }

        private object JsonEscaped(in string value, in bool quoted)
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

        #region Readonly

        private bool _setOnce = false;
        private bool _readonly = false;

        public void SetREadonly() => _readonly = true;

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

        protected AnsibleExtraVarsBase(in bool setOnce = true) { _setOnce = setOnce; }

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
            return BaseGet(key);
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
        protected AnsibleExtraVar BaseGet(in string key)
        {
            return GetItemAddIfNeeded(key);
        }

        private AnsibleExtraVar GetItemAddIfNeeded(in string key)
        {
            if(_data.TryGetValue(key, out var value)) { 
                return value; 
            }

            if (_readonly) throw new InvalidOperationException("Readonly additions not allowed.");

            value = new AnsibleExtraVar(key, _setOnce);
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
        protected void BaseSet(in AnsibleExtraVar value, [CallerMemberName] string key = "")
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
        protected void BaseSet(in string key, in AnsibleExtraVar value)
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
        protected void BaseSetConcatOnly(in AnsibleExtraVar value, [CallerMemberName] string key = "")
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
        protected void BaseSetConcatOnly(in string key, in AnsibleExtraVar value)
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

        public string ToJsonString(in bool compact = true)
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
