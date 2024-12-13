using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using plenidev.Common;
using static plenidev.Common.JsonHelpers;

using CallerMemberNameAttribute = System.Runtime.CompilerServices.CallerMemberNameAttribute;

namespace plenidev.Ansible.Api
{
    /// <summary>
    /// Options used when translating PropertyNames into Ansible Extra-Variable
    /// names, which are snake case, but with various conventions.
    /// </summary>
    public class AnsibleExtraVarNameOptions
    {
        public static readonly AnsibleExtraVarNameOptions Default = new();

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


    internal interface IAnsibleVariable
    {
        string Name { get; }
        public object Value { get; }
        public string ToJsonString(bool compact = true);
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
    public class AnsibleVariableBase<T>
    {
        protected readonly bool _setOnce;

        protected bool _readonly;
        protected readonly string _name;
        //private readonly JsonType _jsonType;
        protected bool _empty;
        protected T? _value;

        #region Construction

        internal AnsibleVariableBase(string name, bool setOnce, AnsibleExtraVarNameOptions? opts = null)
        {
            _name = ToAnsibleNameChecked(name, opts ?? AnsibleExtraVarNameOptions.Default);
            _empty = false;
            _value = default;
            _setOnce = setOnce;
        }

        #endregion

        #region Properties

        public string Name => _name;

        public T Value
        {
            get
            {
                if (_value == null) throw new InvalidOperationException("Value must be set before getting.");
                return _value!;       
            }
            set => _value = CheckedValue(value);
        }

        internal AnsibleVariableBase<T> WithReadOnly(bool value)
        {
            _readonly = value;
            return this;
        }

        #region Set/Replace Operator

        static public AnsibleVariableBase<T> operator <<(AnsibleVariableBase<T> inst, T? value)
        {
            inst.CheckedValue(value);
            
            //inst._jsonType = inst.CheckedJsonType(JsonType.JsonBoolean);
            inst._value = value;

            return inst;
        }

        #endregion

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

        private T CheckedValue([NotNull] T? value)
        {
            ExceptionHelpers.ThrowIfNull(value);
            ExceptionHelpers.ThrowIf(_setOnce && _value != null);
            ExceptionHelpers.ThrowIf(_readonly);

            return value;
        }

        //private JsonType CheckedJsonType(JsonType jsonType)
        //{
        //    ExceptionHelpers.ThrowIf(!(
        //        _jsonType == JsonType.Undetermined ||
        //        _jsonType == jsonType
        //        ));
        //    return jsonType;
        //}

        private string ToAnsibleNameChecked(string name, AnsibleExtraVarNameOptions options)
        {
            ExceptionHelpers.ThrowIfNullOrEmpty(name);

            var ansibleName = ToAnsibleNameUnchecked(name, options);

            if(!finalNameValidationRe.IsMatch(ansibleName) || pythonKeywords.Contains(ansibleName))
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

    }

    class AnsibleStringVariable: AnsibleVariableBase<string>, IAnsibleVariable
    {
        internal AnsibleStringVariable(string name, bool setOnce, AnsibleExtraVarNameOptions? opts = null) : base(name, setOnce, opts){}

        object IAnsibleVariable.Value => Value;

        public string ToJsonString(bool compact = true)
        {
            return JsonHelpers.GetPropertyString(_name, Value, compact);
        }
    }

    class AnsibleIntVariable : AnsibleVariableBase<int>, IAnsibleVariable
    {
        internal AnsibleIntVariable(string name, bool setOnce, AnsibleExtraVarNameOptions? opts = null) : base(name, setOnce, opts) { }

        object IAnsibleVariable.Value => Value;

        public string ToJsonString(bool compact = true)
        {
            return JsonHelpers.GetPropertyString(_name, Value, compact);
        }
    }

    class AnsibleBooleanVariable : AnsibleVariableBase<bool>, IAnsibleVariable
    {
        internal AnsibleBooleanVariable(string name, bool setOnce, AnsibleExtraVarNameOptions? opts = null) : base(name, setOnce, opts) { }

        object IAnsibleVariable.Value => Value;

        public string ToJsonString(bool compact = true)
        {
            return JsonHelpers.GetPropertyString(_name, Value, compact);
        }
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
        private readonly Dictionary<string, IAnsibleVariable> _data = [];

        private readonly AnsibleExtraVarNameOptions _options;
        private readonly bool _setOnce = false;

        #region Readonly

        private bool _readonly = false;

        public void SetReadonly() => _readonly = true;

        public T AsReadonlyChecked
        {
            get
            {
                ExceptionHelpers.ThrowIf(_readonly);
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
        protected AnsibleVariableBase<V> BaseGet<V>([CallerMemberName] string key = "")
        {
            return BaseGetForKey<V>(key);
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
        protected AnsibleVariableBase<V> BaseGetForKey<V>(string key)
        { 
            return GetItemAddIfNeeded<V>(key);
        }

        private AnsibleVariableBase<V> GetItemAddIfNeeded<V>(string key)
        {
            if(_data.TryGetValue(key, out var value)) { 
                return (value as AnsibleVariableBase<V>)!; 
            }

            ExceptionHelpers.ThrowIf(_readonly);

            value = new AnsibleVariableBase<V>(key, _setOnce, _options) as IAnsibleVariable;
            _data.Add(key, value!);
            return (value as AnsibleVariableBase<V>)!;

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
        protected void BaseSet<V>(AnsibleVariableBase<V> value, [CallerMemberName] string key = "")
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
        protected void BaseSet<V>(string key, AnsibleVariableBase<V> value)
        {
            var inst = (_data[key] as AnsibleVariableBase<V>)!;

            ExceptionHelpers.ThrowIf(inst.Name != value.Name);
            
            if (EqualityComparer<V>.Default.Equals(inst.Value, value.Value))
            {
                // avoids triggering setOnce logic when += has 
                // been used.
                return;
            }

            inst.Value = value.Value;
        }

        /// <summary>
        /// <para>
        /// Derived classes wishing to support
        /// </para><para>
        /// <c>Inst.Property += "value";</c>
        /// </para><para>but not</para><para>
        /// <c>Inst.Property = new AnsibleExtraVar(...)</c>
        /// </para><para>
        /// syntax should use this method to define properties as
        /// </para><para>
        /// <c>set => BaseSetConcatOnly(nameof(&lt;property>), value);</c>
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void BaseSetConcatOnly<V>(AnsibleVariableBase<V> value, [CallerMemberName] string key = "")
        {
            BaseSetConcatOnly(key, value);
        }

        /// <summary>
        /// <para>
        /// Derived classes wishing to support
        /// </para><para>
        /// <c>Inst.Property += "value";</c>
        /// </para><para>but not</para><para>
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
        protected void BaseSetConcatOnly<V>(string key, AnsibleVariableBase<V> value)
        {
            var inst = (_data[key] as AnsibleVariableBase<V>)!;

            ExceptionHelpers.ThrowIf(inst.Name != value.Name);

            if (!EqualityComparer<V>.Default.Equals(inst.Value, value.Value))
            {
                ThrowNotSetViaConcatOperatorException();
            }
        }

        private void ThrowNotSetViaConcatOperatorException() =>
            throw new InvalidOperationException($"Values must be set using += <string_value> or their Value property.");

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
