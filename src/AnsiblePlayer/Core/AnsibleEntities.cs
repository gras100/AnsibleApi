using System;
using System.Collections.Generic;
using System.Text;

namespace plenidev.AnsiblePlayer.Core
{
    public class AnsibleId
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public int? CredentialType { get; init; } = null;
        public string[]? PasswordsNeeded { get; init; } = null;
    }

    public class AnsibleCredential
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
        public string? Kind { get; init; } = null;
        public bool? Cloud { get; init; } = null;
    }

    public class AnsibleOrganization
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
    }

    public class AnsibleInventory
    {
        #region Get

        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;

        #endregion

        #region Post

        public string? Description { get; init; } = null;
        public bool HasActiveFailures { get; init; } = false;
        public int? TotalHosts { get; init; } = null;
        public int? HostsWithActiveFailures { get; init; } = null;
        public int? TotalGroups { get; init; } = null;
        public bool? HasInventorySources { get; init; } = null;
        public int? TotalInventorySources { get; init; } = null;
        public int? InventorySourcesWithFailures { get; init; } = null;
        public int? OrganizationId { get; init; } = null;
        public string? Kind { get; init; } = null;

        #endregion
    }

    public class AnsibleExecutionEnvironment
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
        public string? Image { get; init; } = null;
    }

    public class AnsibleProject
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
        public string? Status { get; init; } = null;
        public string? ScmType { get; init; } = null;
        public bool? AllowOverride { get; init; } = null;
    }

    public class AnsibleJob
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
        public DateTime? Finished { get; init; } = null;
        public string? Status { get; init; } = null;
        public bool? Failed { get; init; } = null;
    }

    public class AnsibleUser
    {
        public int? Id { get; init; }
        public string? Username { get; init; } = null;
        public string? FirstName { get; init; } = null;
        public string? LastName { get; init; } = null;
    }

    public class AnsibleRole
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
    }

    public class AnsibleObjectRoles
    {
        public AnsibleRole? AdminRole { get; init; } = null;
        public AnsibleRole? ExecuteRole { get; init; } = null;
        public AnsibleRole[]? ReadRole { get; init; } = null;
    }

    public class AnsibleUserCapabilities
    {
        public bool? Edit { get; init; } = null;
        public bool? Delete { get; init; } = null;
        public bool? Start { get; init; } = null;
        public bool? Schedule { get; init; } = null;
        public bool? Copy { get; init; } = null;
    }

    public class AnsibleLabel
    {
        public int? Id { get; init; } = null;
        public string? Name { get; init; } = null;
        public AnsibleId? Organization { get; init; } = null;
    }

    public class AnsibleLabels
    {
        public bool? Count { get; init; } = null;
        public AnsibleLabel[]? Results { get; init; } = null;
    }

}
