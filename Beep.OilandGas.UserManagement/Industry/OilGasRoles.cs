namespace Beep.OilandGas.UserManagement.Industry
{
    /// <summary>
    /// Standard Oil & Gas industry role definitions
    /// Based on industry best practices and common organizational structures
    /// </summary>
    public static class OilGasRoles
    {
        /// <summary>
        /// Administrator - Full system access
        /// </summary>
        public const string Administrator = "Administrator";

        /// <summary>
        /// Manager - Management-level access, reporting, oversight
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>
        /// Petroleum Engineer - Production operations, well analysis, optimization
        /// </summary>
        public const string PetroleumEngineer = "PetroleumEngineer";

        /// <summary>
        /// Reservoir Engineer - Reservoir analysis, forecasting, reserve estimation
        /// </summary>
        public const string ReservoirEngineer = "ReservoirEngineer";

        /// <summary>
        /// Production Engineer - Production operations, optimization, artificial lift
        /// </summary>
        public const string ProductionEngineer = "ProductionEngineer";

        /// <summary>
        /// Geologist - Geological data, interpretation, prospect identification
        /// </summary>
        public const string Geologist = "Geologist";

        /// <summary>
        /// Accountant - Financial data, accounting operations, reporting
        /// </summary>
        public const string Accountant = "Accountant";

        /// <summary>
        /// Land Manager - Lease management, land operations, mineral rights
        /// </summary>
        public const string LandManager = "LandManager";

        /// <summary>
        /// Regulatory Specialist - Permits, compliance, regulatory reporting
        /// </summary>
        public const string RegulatorySpecialist = "RegulatorySpecialist";

        /// <summary>
        /// Viewer - Read-only access
        /// </summary>
        public const string Viewer = "Viewer";

        /// <summary>
        /// Field Operator - Field operations, data entry, well monitoring
        /// </summary>
        public const string FieldOperator = "FieldOperator";

        /// <summary>
        /// Data Analyst - Data analysis, reporting, business intelligence
        /// </summary>
        public const string DataAnalyst = "DataAnalyst";

        /// <summary>
        /// Drilling Engineer - Drilling operations, well design, drilling optimization
        /// </summary>
        public const string DrillingEngineer = "DrillingEngineer";

        /// <summary>
        /// Completions Engineer - Well completions, stimulation, production enhancement
        /// </summary>
        public const string CompletionsEngineer = "CompletionsEngineer";

        /// <summary>
        /// Facilities Engineer - Surface facilities, processing, infrastructure
        /// </summary>
        public const string FacilitiesEngineer = "FacilitiesEngineer";

        /// <summary>
        /// Environmental Specialist - Environmental compliance, reporting, remediation
        /// </summary>
        public const string EnvironmentalSpecialist = "EnvironmentalSpecialist";

        /// <summary>
        /// Safety Specialist - Safety compliance, incident management, training
        /// </summary>
        public const string SafetySpecialist = "SafetySpecialist";

        /// <summary>
        /// Gets all standard role names
        /// </summary>
        public static IEnumerable<string> GetAllRoles()
        {
            return new[]
            {
                Administrator,
                Manager,
                PetroleumEngineer,
                ReservoirEngineer,
                ProductionEngineer,
                Geologist,
                Accountant,
                LandManager,
                RegulatorySpecialist,
                Viewer,
                FieldOperator,
                DataAnalyst,
                DrillingEngineer,
                CompletionsEngineer,
                FacilitiesEngineer,
                EnvironmentalSpecialist,
                SafetySpecialist
            };
        }

        /// <summary>
        /// Checks if a role name is a standard role
        /// </summary>
        public static bool IsStandardRole(string roleName)
        {
            return GetAllRoles().Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
