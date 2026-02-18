namespace Beep.OilandGas.Models.Data.Security
{
    public static class PermissionConstants
    {
        public static class Accounting
        {
            public const string View = "Accounting.View";
            public const string PostJournal = "Accounting.PostJournal";
            public const string ApproveJournal = "Accounting.ApproveJournal";
            public const string EditSettings = "Accounting.EditSettings";
            public const string ViewReports = "Accounting.ViewReports";
            public const string ManagePeriods = "Accounting.ManagePeriods";
        }

        public static class Admin
        {
            public const string ManageUsers = "Admin.ManageUsers";
            public const string AssignRoles = "Admin.AssignRoles";
            public const string ViewAuditLogs = "Admin.ViewAuditLogs";
            public const string ConfigureSystem = "Admin.ConfigureSystem";
        }

        public static class Tax
        {
            public const string ViewProvision = "Tax.ViewProvision";
            public const string Calculate = "Tax.Calculate";
            public const string Adjust = "Tax.Adjust";
        }
    }
}
