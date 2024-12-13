using System.Text.Json;
using System.Text.Json.Serialization;


namespace plenidev.AnsiblePlayer.Core
{
    public interface IAnsibleOptions
    {
        public string ApiVersionSupport { get; }
        public string ToJsonDataString(bool pretty = false);
    }

    public class AnsibleOptionsBase<T>(string apiVersionSupport = @"api/v2"): IAnsibleOptions where T : AnsibleOptionsBase<T>
    {
        private static readonly JsonSerializerOptions wireSerializationOptions = new()
        {
            IgnoreReadOnlyFields = true,
            PropertyNamingPolicy = new AnsibleNamingPolicy(AnsibleNamingPolicyOptions.Default),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions prettySerializationOptions = new()
        {
            WriteIndented = true,
            // Breaks F♯ Scripting:
            // System.MissingMethodException: Method not found:
            // 'Void System.Text.Json.JsonSerializerOptions.set_IndentCharacter(Char)'. 
            // IndentCharacter = ' ',
            // Breaks F♯ Scripting:
            // System.MissingMethodException: Method not found:
            // 'Void System.Text.Json.JsonSerializerOptions.set_IndentSize(Char)'.
            // See https://sergeyteplyakov.github.io/Blog/csharp/2024/03/21/Mythical_MissingMethodException.html
            //IndentSize = 2,
            IgnoreReadOnlyFields = true,
            PropertyNamingPolicy = new AnsibleNamingPolicy(AnsibleNamingPolicyOptions.Default),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public string ApiVersionSupport => apiVersionSupport;

        public string ToJsonDataString(bool pretty = false)
        {
            return JsonSerializer.Serialize<T>((T)this, 
                pretty ? prettySerializationOptions : wireSerializationOptions
                );
        }
    }
}
