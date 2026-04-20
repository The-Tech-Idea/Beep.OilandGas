using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Development
{
    /// <summary>Request body for POST /api/field/current/development/fdp</summary>
    public class SubmitFdpDraftRequest
    {
        public double   StoiipMmbo            { get; set; }
        public double   GiipBcf               { get; set; }
        public double   RecoveryFactor        { get; set; }
        public double   ReservesMmbo          { get; set; }
        public string   ReservesClass         { get; set; } = string.Empty;
        public string   ReservoirDescription  { get; set; } = string.Empty;
        public string   SelectedConcept       { get; set; } = string.Empty;
        public string   FacilityType          { get; set; } = string.Empty;
        public string   ConceptRationale      { get; set; } = string.Empty;
        public double   CapexMm               { get; set; }
        public double   OpexMmPerYear         { get; set; }
        public double   Npv10Mm               { get; set; }
        public double   Irr                   { get; set; }
        public double   BreakevenUsd          { get; set; }
        public string   EconomicsNotes        { get; set; } = string.Empty;
        public string   Jurisdiction          { get; set; } = string.Empty;
        public string   PermitList            { get; set; } = string.Empty;
        public DateTime? SubmissionDate       { get; set; }
        public DateTime? ExpectedApprovalDate { get; set; }
        public string   RegulatoryNotes       { get; set; } = string.Empty;
        public List<FdpWellProgramItem> WellProgram { get; set; } = new();
    }

    public class FdpWellProgramItem
    {
        public string WellName   { get; set; } = string.Empty;
        public string WellType   { get; set; } = string.Empty;
        public string TargetZone { get; set; } = string.Empty;
        public int    PlannedYear { get; set; }
    }

    /// <summary>Response body returned by POST /api/field/current/development/fdp</summary>
    public class SubmitFdpDraftResponse
    {
        public string InstanceId { get; set; } = string.Empty;
        public string Status     { get; set; } = string.Empty;
    }

    /// <summary>Response body returned by GET /api/field/current/development/fdp</summary>
    public class FdpStatusResponse
    {
        public bool   HasFdp            { get; set; }
        public string? ProcessInstanceId { get; set; }
        public string Status            { get; set; } = string.Empty;
        public string CurrentStepId     { get; set; } = string.Empty;
        public DateTime? StartDate      { get; set; }
        public List<FdpStepStatus> Steps { get; set; } = new();
    }

    public class FdpStepStatus
    {
        public string StepId        { get; set; } = string.Empty;
        public string StepName      { get; set; } = string.Empty;
        public bool   IsCompleted   { get; set; }
        public bool   IsActive      { get; set; }
        public string CompletedDate { get; set; } = string.Empty;
    }
}
