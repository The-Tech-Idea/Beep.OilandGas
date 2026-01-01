namespace Beep.OilandGas.UserManagement.Core.DataAccess
{
    /// <summary>
    /// Types of row-level security rules
    /// </summary>
    public enum RowLevelSecurityRuleType
    {
        /// <summary>
        /// Creator-based: Users see only rows they created (ROW_CREATED_BY)
        /// </summary>
        Creator,

        /// <summary>
        /// Source-based: Users see only rows from certain sources (SOURCE field)
        /// </summary>
        Source,

        /// <summary>
        /// Tenant-based: Users see only rows for their tenant (TENANT_ID)
        /// </summary>
        Tenant,

        /// <summary>
        /// Role-based: Different RLS rules based on user roles
        /// </summary>
        Role,

        /// <summary>
        /// Custom: Custom filter expression
        /// </summary>
        Custom
    }

    /// <summary>
    /// Model for defining row-level security rules
    /// </summary>
    public class RowLevelSecurityRule
    {
        /// <summary>
        /// Gets or sets the rule ID
        /// </summary>
        public string RuleId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the rule type
        /// </summary>
        public RowLevelSecurityRuleType RuleType { get; set; }

        /// <summary>
        /// Gets or sets the table name (use "*" for all tables)
        /// </summary>
        public string TableName { get; set; } = "*";

        /// <summary>
        /// Gets or sets the filter expression (for Custom rule type)
        /// </summary>
        public string? FilterExpression { get; set; }

        /// <summary>
        /// Gets or sets the priority (lower number = higher priority)
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether the rule is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the roles this rule applies to (for Role rule type)
        /// </summary>
        public IEnumerable<string> ApplicableRoles { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the source values (for Source rule type)
        /// </summary>
        public IEnumerable<string> SourceValues { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the tenant ID (for Tenant rule type)
        /// </summary>
        public string? TenantId { get; set; }
    }
}
