namespace Beep.OilandGas.Models.Constants;

/// <summary>
/// HSE reference values — canonical codes for incidents, permits, and compliance.
/// </summary>
public static class HseReferenceCodes
{
    public static class FieldContext
    {
        public const string PrimaryRole = "PRIMARY_FIELD_CONTEXT";
        public const string ComponentType = "INCIDENT_LOCATION";
    }

    public static class Hazop
    {
        public const string ProjectType = "HAZOP";
        public const string NodeType = "HAZOP_NODE";
        public const string StudyStatusType = "HAZOP_STUDY";
        public const string DeviationStatusType = "HAZOP_DEVIATION";
        public const string FieldContextSource = "Beep.OilandGas.HSE.Field";
        public const string Draft = "DRAFT";
        public const string Open = "OPEN";
        public const string ActionRequired = "ACTION_REQUIRED";
        public const string Closed = "CLOSED";
    }

    public static class CorrectiveAction
    {
        public const string ProjectType = "CORRECTIVE_ACTION";
        public const string StatusType = "CA_STEP";
        public const string ResponsibleRole = "RESPONSIBLE";
        public const string Open = "OPEN";
        public const string Completed = "COMPLETED";
    }

    public static class Investigator
    {
        public const string Role = "INVESTIGATOR";
        public const string Assigned = "ASSIGNED";
    }

    public static class IncidentType
    {
        public const string LostTime = "LOST_TIME";
        public const string FirstAid = "FIRST_AID";
        public const string NearMiss = "NEAR_MISS";
        public const string Environmental = "ENVIRONMENTAL";
        public const string PropertyDamage = "PROPERTY_DAMAGE";
        public const string Vehicle = "VEHICLE";
        public const string Fire = "FIRE";
        public const string Spill = "SPILL";
        public static readonly string[] All = { LostTime, FirstAid, NearMiss, Environmental, PropertyDamage, Vehicle, Fire, Spill };
    }

    public static class Severity
    {
        public const string Tier1 = "TIER_1"; // Major / Fatal
        public const string Tier2 = "TIER_2"; // Serious / Recordable
        public const string Tier3 = "TIER_3"; // Minor / First Aid
        public const string Tier4 = "TIER_4"; // Near Miss / Observation
        public static readonly string[] All = { Tier1, Tier2, Tier3, Tier4 };
    }

    public static class PermitType
    {
        public const string HotWork = "HOT_WORK";
        public const string ColdWork = "COLD_WORK";
        public const string ConfinedSpace = "CONFINED_SPACE";
        public const string Excavation = "EXCAVATION";
        public const string Electrical = "ELECTRICAL";
        public const string Lifting = "LIFTING";
        public const string PressureTest = "PRESSURE_TEST";
        public static readonly string[] All = { HotWork, ColdWork, ConfinedSpace, Excavation, Electrical, Lifting, PressureTest };
    }

    public static class ComplianceStatus
    {
        public const string Compliant = "COMPLIANT";
        public const string NonCompliant = "NON_COMPLIANT";
        public const string UnderReview = "UNDER_REVIEW";
        public const string Exempt = "EXEMPT";
        public const string Overdue = "OVERDUE";
        public static readonly string[] All = { Compliant, NonCompliant, UnderReview, Exempt, Overdue };
    }
}
