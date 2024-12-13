using System;
using System.Collections.Generic;
using System.Text;

using plenidev.AnsiblePlayer.Core;

namespace plenidev.AnsiblePlayer
{
    public static partial class JobTemplates
    {
        public static partial class Launch
        {
            public class PostOptionsCommon<ExVar>() : AnsibleOptionsBase<PostOptionsCommon<ExVar>>(apiVersionSupport: "api/v2") where ExVar : class
            {
                public ExVar? ExtraVars { get; init; } = null;
                public string? JobTags { get; init; } = null;
                public string? SkipTags { get; init; } = null;
                public Core.AnsibleChoices.Verbosity? Verbosity { get; init; } = null;
                public bool? DiffMode { get; init; } = null;
                public Core.AnsibleLabels? Labels { get; init; } = null;
                public int? TimeOut { get; init; } = null;
            }
        }
    }
}
