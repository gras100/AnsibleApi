using plenidev.Ansible.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace plenidev.Common
{
    static class JsonHelpers
    {
        public enum JsonType
        {
            JsonString = 1,
            JsonNumber = 2,
            JsonBoolean = 3,
            JsonObject = 4,
            JsonArray = 5,
            //JsonNull = 6,
            //Undetermined = 0
        }    

        public static string JsonStringEscaped(string value, bool quoted)
        {
            var sb = new StringBuilder();
            foreach (var c in value.ToCharArray())
            {
                switch (c)
                {
                    case '"': sb.Append(@"\"""); break;
                    case '\\': sb.Append(@"\"""); break;
                    case '/': sb.Append(@"\"""); break;
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

        private static string GetEnumerablePropertyBase<T>(Func<T, string> convertItem, string name, IEnumerable<T> _data, char open = '{', char close = '{', bool compact = true)
        {
            var space = compact ? "" : " ";
            var comma = compact ? "," : ", ";

            var sep = space;
            var sb = new StringBuilder($"\"{name}\":{space}{open}");
            foreach (var item in _data)
            {
                sb.Append(sep).Append(convertItem(item));
                sep = comma;
            }
            return sb.Append($"{space}{close}").ToString();
        }

        static private string GetBasicPropertyBase<T>(string name, T value, bool compact = true)
        {
            var assign = compact ? ":" : ": ";
            return $"\"{name}\"{assign}{value}";
        }

        public static string GetPropertyString(string name, int value, bool compact = true) => 
            GetBasicPropertyBase(name, value, compact);

        public static string GetPropertyString(string name, Int64 value, bool compact = true) => 
            GetBasicPropertyBase(name, value, compact);

        public static string GetPropertyString(string name, bool value, bool compact = true) => 
            GetBasicPropertyBase(name, value ? "true" : "false", compact);

        public static string GetPropertyString(string name, string value, bool compact = true) => 
            GetBasicPropertyBase(name, JsonStringEscaped(value, quoted: true), compact);

        public static string GetPropertyString(string name, IEnumerable<string> value, bool compact = true) => 
            GetEnumerablePropertyBase((item => JsonStringEscaped(item, quoted: true)), name, value, '[',  ']', compact);

        public static string GetPropertyString(string name, IEnumerable<int> value, bool compact = true) =>
            GetEnumerablePropertyBase((item => $"\"{item}\""), name, value, '[', ']', compact);

        public static string GetPropertyString(string name, IEnumerable<Int64> value, bool compact = true) =>
            GetEnumerablePropertyBase((item => $"\"{item}\""), name, value, '[', ']', compact);

        public static string GetPropertyString(string name, IEnumerable<bool> value, bool compact = true) =>
            GetEnumerablePropertyBase((item => $"\"{item}\"".ToLowerInvariant()), name, value, '[', ']', compact);

        public static string GetPropertyString(string name, IEnumerable<IAnsibleVariable> value, bool compact = true) =>
            GetEnumerablePropertyBase((item => (item as IAnsibleVariable).ToJsonString()), name, value, '{', '}', compact);

    }
}
