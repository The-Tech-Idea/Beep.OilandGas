using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for drilling operation.
    /// </summary>
    public class DrillingOperation : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private DateTime? SpudDateValue;

        public DateTime? SpudDate

        {

            get { return this.SpudDateValue; }

            set { SetProperty(ref SpudDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? CurrentDepthValue;

        public decimal? CurrentDepth

        {

            get { return this.CurrentDepthValue; }

            set { SetProperty(ref CurrentDepthValue, value); }

        }
        private decimal? TargetDepthValue;

        public decimal? TargetDepth

        {

            get { return this.TargetDepthValue; }

            set { SetProperty(ref TargetDepthValue, value); }

        }
        private string? DrillingContractorValue;

        public string? DrillingContractor

        {

            get { return this.DrillingContractorValue; }

            set { SetProperty(ref DrillingContractorValue, value); }

        }
        private string? RigNameValue;

        public string? RigName

        {

            get { return this.RigNameValue; }

            set { SetProperty(ref RigNameValue, value); }

        }
        private decimal? DailyCostValue;

        public decimal? DailyCost

        {

            get { return this.DailyCostValue; }

            set { SetProperty(ref DailyCostValue, value); }

        }
        private decimal? TotalCostValue;

        public decimal? TotalCost

        {

            get { return this.TotalCostValue; }

            set { SetProperty(ref TotalCostValue, value); }

        }
        private string? CurrencyValue;

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
        private List<DrillingReport> ReportsValue = new();

        public List<DrillingReport> Reports

        {

            get { return this.ReportsValue; }

            set { SetProperty(ref ReportsValue, value); }

        }
    }

    /// <summary>
    /// DTO for drilling report.
    /// </summary>
    public class DrillingReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private DateTime? ReportDateValue;

        public DateTime? ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
        private decimal? DepthValue;

        public decimal? Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }
        private string? ActivityValue;

        public string? Activity

        {

            get { return this.ActivityValue; }

            set { SetProperty(ref ActivityValue, value); }

        }
        private decimal? HoursValue;

        public decimal? Hours

        {

            get { return this.HoursValue; }

            set { SetProperty(ref HoursValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private string? ReportedByValue;

        public string? ReportedBy

        {

            get { return this.ReportedByValue; }

            set { SetProperty(ref ReportedByValue, value); }

        }
    }

    /// <summary>
    /// DTO for well construction.
    /// </summary>
    public class WellConstruction : ModelEntityBase
    {
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private List<CasingString> CasingStringsValue = new();

        public List<CasingString> CasingStrings

        {

            get { return this.CasingStringsValue; }

            set { SetProperty(ref CasingStringsValue, value); }

        }
        private List<CompletionString> CompletionStringsValue = new();

        public List<CompletionString> CompletionStrings

        {

            get { return this.CompletionStringsValue; }

            set { SetProperty(ref CompletionStringsValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for casing string.
    /// </summary>
    public class CasingString : ModelEntityBase
    {
        private string CasingIdValue = string.Empty;

        public string CasingId

        {

            get { return this.CasingIdValue; }

            set { SetProperty(ref CasingIdValue, value); }

        }
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string CasingTypeValue = string.Empty;

        public string CasingType

        {

            get { return this.CasingTypeValue; }

            set { SetProperty(ref CasingTypeValue, value); }

        }
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BottomDepthValue;

        public decimal? BottomDepth

        {

            get { return this.BottomDepthValue; }

            set { SetProperty(ref BottomDepthValue, value); }

        }
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private string? DiameterUnitValue;

        public string? DiameterUnit

        {

            get { return this.DiameterUnitValue; }

            set { SetProperty(ref DiameterUnitValue, value); }

        }
        private string? GradeValue;

        public string? Grade

        {

            get { return this.GradeValue; }

            set { SetProperty(ref GradeValue, value); }

        }
        private decimal? WeightValue;

        public decimal? Weight

        {

            get { return this.WeightValue; }

            set { SetProperty(ref WeightValue, value); }

        }
        private string? WeightUnitValue;

        public string? WeightUnit

        {

            get { return this.WeightUnitValue; }

            set { SetProperty(ref WeightUnitValue, value); }

        }
    }

    /// <summary>
    /// DTO for completion string.
    /// </summary>
    public class CompletionString : ModelEntityBase
    {
        private string CompletionIdValue = string.Empty;

        public string CompletionId

        {

            get { return this.CompletionIdValue; }

            set { SetProperty(ref CompletionIdValue, value); }

        }
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string CompletionTypeValue = string.Empty;

        public string CompletionType

        {

            get { return this.CompletionTypeValue; }

            set { SetProperty(ref CompletionTypeValue, value); }

        }
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BottomDepthValue;

        public decimal? BottomDepth

        {

            get { return this.BottomDepthValue; }

            set { SetProperty(ref BottomDepthValue, value); }

        }
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private string? DiameterUnitValue;

        public string? DiameterUnit

        {

            get { return this.DiameterUnitValue; }

            set { SetProperty(ref DiameterUnitValue, value); }

        }
        private List<Perforation> PerforationsValue = new();

        public List<Perforation> Perforations

        {

            get { return this.PerforationsValue; }

            set { SetProperty(ref PerforationsValue, value); }

        }
    }

    /// <summary>
    /// DTO for perforation.
    /// </summary>
    public class Perforation : ModelEntityBase
    {
        private string PerforationIdValue = string.Empty;

        public string PerforationId

        {

            get { return this.PerforationIdValue; }

            set { SetProperty(ref PerforationIdValue, value); }

        }
        private string CompletionIdValue = string.Empty;

        public string CompletionId

        {

            get { return this.CompletionIdValue; }

            set { SetProperty(ref CompletionIdValue, value); }

        }
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BottomDepthValue;

        public decimal? BottomDepth

        {

            get { return this.BottomDepthValue; }

            set { SetProperty(ref BottomDepthValue, value); }

        }
        private int? ShotsPerFootValue;

        public int? ShotsPerFoot

        {

            get { return this.ShotsPerFootValue; }

            set { SetProperty(ref ShotsPerFootValue, value); }

        }
        private string? PerforationTypeValue;

        public string? PerforationType

        {

            get { return this.PerforationTypeValue; }

            set { SetProperty(ref PerforationTypeValue, value); }

        }
        private DateTime? PerforationDateValue;

        public DateTime? PerforationDate

        {

            get { return this.PerforationDateValue; }

            set { SetProperty(ref PerforationDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for facility construction.
    /// </summary>
    public class FacilityConstruction : ModelEntityBase
    {
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string FacilityTypeValue = string.Empty;

        public string FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? CommissioningDateValue;

        public DateTime? CommissioningDate

        {

            get { return this.CommissioningDateValue; }

            set { SetProperty(ref CommissioningDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private decimal? ActualCostValue;

        public decimal? ActualCost

        {

            get { return this.ActualCostValue; }

            set { SetProperty(ref ActualCostValue, value); }

        }
        private string? CurrencyValue;

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
        private string? ContractorValue;

        public string? Contractor

        {

            get { return this.ContractorValue; }

            set { SetProperty(ref ContractorValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating a drilling operation.
    /// </summary>
    public class CreateDrillingOperation : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime? PlannedSpudDateValue;

        public DateTime? PlannedSpudDate

        {

            get { return this.PlannedSpudDateValue; }

            set { SetProperty(ref PlannedSpudDateValue, value); }

        }
        private decimal? TargetDepthValue;

        public decimal? TargetDepth

        {

            get { return this.TargetDepthValue; }

            set { SetProperty(ref TargetDepthValue, value); }

        }
        private string? DrillingContractorValue;

        public string? DrillingContractor

        {

            get { return this.DrillingContractorValue; }

            set { SetProperty(ref DrillingContractorValue, value); }

        }
        private string? RigNameValue;

        public string? RigName

        {

            get { return this.RigNameValue; }

            set { SetProperty(ref RigNameValue, value); }

        }
        private decimal? EstimatedDailyCostValue;

        public decimal? EstimatedDailyCost

        {

            get { return this.EstimatedDailyCostValue; }

            set { SetProperty(ref EstimatedDailyCostValue, value); }

        }
    }

    /// <summary>
    /// DTO for updating a drilling operation.
    /// </summary>
    public class UpdateDrillingOperation : ModelEntityBase
    {
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? CurrentDepthValue;

        public decimal? CurrentDepth

        {

            get { return this.CurrentDepthValue; }

            set { SetProperty(ref CurrentDepthValue, value); }

        }
        private decimal? DailyCostValue;

        public decimal? DailyCost

        {

            get { return this.DailyCostValue; }

            set { SetProperty(ref DailyCostValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating a drilling report.
    /// </summary>
    public class CreateDrillingReport : ModelEntityBase
    {
        private DateTime ReportDateValue;

        public DateTime ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
        private decimal? DepthValue;

        public decimal? Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }
        private string? ActivityValue;

        public string? Activity

        {

            get { return this.ActivityValue; }

            set { SetProperty(ref ActivityValue, value); }

        }
        private decimal? HoursValue;

        public decimal? Hours

        {

            get { return this.HoursValue; }

            set { SetProperty(ref HoursValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}







