using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using plenidev.AnsiblePlayer.Core;


namespace plenidev.AnsiblePlayer
{
    public static partial class JobTemplates
    {
        public static partial class Launch
        {
            public class PostOptionsAll<ExVar>() : AnsibleOptionsBase<PostOptionsAll<ExVar>>(apiVersionSupport: "api/v2") where ExVar : class
            {
                public ExVar? ExtraVars { get; init; } = null;
                public int? Inventory { get; init; } = null;
                public string? ScmBranch { get; init; } = null;
                public string? Limit { get; init; } = null;
                public string? JobTags { get; init; } = null;
                public string? SkipTags { get; init; } = null;
                public Core.AnsibleChoices.JobType? JobType { get; init; } = null;
                public Core.AnsibleChoices.Verbosity? Verbosity { get; init; } = null;
                public bool? DiffMode { get; init; } = null;
                public Core.AnsibleCredential[]? Credentials { get; init; } = null;
                //public AnsibleId[]? CredentialPasswords { get; init; } = null;
                public Core.AnsibleId? ExecutionEnvironment { get; init; } = null;
                public Core.AnsibleLabels? Labels { get; init; } = null;
                public int? Forks { get; init; } = null;
                public int? JobSliceCount { get; init; } = null;
                public int? TimeOut { get; init; } = null;
                // Admins.
                //public ExVar? InstanceGroups { get; init; }
            }
        }

    }
}
