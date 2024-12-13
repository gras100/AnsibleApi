using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace plenidev.AnsiblePlayer.Core;

public static partial class AnsibleChoices
{
    public class LabelConverter<TEnum> : JsonStringEnumConverter<TEnum> where TEnum : struct, Enum
    {
        public LabelConverter() : base(System.Text.Json.JsonNamingPolicy.SnakeCaseLower) { }
    }

    [JsonConverter(typeof(LabelConverter<JobType>))]
    public enum JobType
    {
        Run /* run */,
        Check /* check */
    }

    public enum Verbosity
    {
        Normal = 0,
        Verbose = 1,
        MoreVerbose = 2,
        Debug = 3,
        ConnectionDebug = 4,
        WinRmDebug = 5,
    }
}
