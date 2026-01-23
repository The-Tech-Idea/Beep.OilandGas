using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for enhanced recovery operation.
    /// </summary>
    public class EnhancedRecoveryOperation : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string EORTypeValue = string.Empty;

        public string EORType

        {

            get { return this.EORTypeValue; }

            set { SetProperty(ref EORTypeValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? InjectionRateValue;

        public decimal? InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private decimal? ProductionIncreaseValue;

        public decimal? ProductionIncrease

        {

            get { return this.ProductionIncreaseValue; }

            set { SetProperty(ref ProductionIncreaseValue, value); }

        }
        private string? ProductionUnitValue;

        public string? ProductionUnit

        {

            get { return this.ProductionUnitValue; }

            set { SetProperty(ref ProductionUnitValue, value); }

        }
        private decimal? EfficiencyValue;

        public decimal? Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private List<InjectionWell> InjectionWellsValue = new();

        public List<InjectionWell> InjectionWells

        {

            get { return this.InjectionWellsValue; }

            set { SetProperty(ref InjectionWellsValue, value); }

        }
    }

    /// <summary>
    /// DTO for injection operation.
    /// </summary>
    public class InjectionOperation : ModelEntityBase
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
        private string InjectionTypeValue = string.Empty;

        public string InjectionType

        {

            get { return this.InjectionTypeValue; }

            set { SetProperty(ref InjectionTypeValue, value); }

        }
        private DateTime OperationDateValue;

        public DateTime OperationDate

        {

            get { return this.OperationDateValue; }

            set { SetProperty(ref OperationDateValue, value); }

        }
        private decimal? InjectionRateValue;

        public decimal? InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private decimal? InjectionPressureValue;

        public decimal? InjectionPressure

        {

            get { return this.InjectionPressureValue; }

            set { SetProperty(ref InjectionPressureValue, value); }

        }
        private string? PressureUnitValue;

        public string? PressureUnit

        {

            get { return this.PressureUnitValue; }

            set { SetProperty(ref PressureUnitValue, value); }

        }
        private decimal? CumulativeInjectionValue;

        public decimal? CumulativeInjection

        {

            get { return this.CumulativeInjectionValue; }

            set { SetProperty(ref CumulativeInjectionValue, value); }

        }
        private string? CumulativeUnitValue;

        public string? CumulativeUnit

        {

            get { return this.CumulativeUnitValue; }

            set { SetProperty(ref CumulativeUnitValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for injection well.
    /// </summary>
    public class InjectionWell : ModelEntityBase
    {
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
        private string InjectionTypeValue = string.Empty;

        public string InjectionType

        {

            get { return this.InjectionTypeValue; }

            set { SetProperty(ref InjectionTypeValue, value); }

        }
        private string? InjectionZoneValue;

        public string? InjectionZone

        {

            get { return this.InjectionZoneValue; }

            set { SetProperty(ref InjectionZoneValue, value); }

        }
        private decimal? InjectionRateValue;

        public decimal? InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for water flooding operation.
    /// </summary>
    public class WaterFlooding : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? WaterInjectionRateValue;

        public decimal? WaterInjectionRate

        {

            get { return this.WaterInjectionRateValue; }

            set { SetProperty(ref WaterInjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private string? WaterSourceValue;

        public string? WaterSource

        {

            get { return this.WaterSourceValue; }

            set { SetProperty(ref WaterSourceValue, value); }

        }
        private decimal? ProductionIncreaseValue;

        public decimal? ProductionIncrease

        {

            get { return this.ProductionIncreaseValue; }

            set { SetProperty(ref ProductionIncreaseValue, value); }

        }
        private string? ProductionUnitValue;

        public string? ProductionUnit

        {

            get { return this.ProductionUnitValue; }

            set { SetProperty(ref ProductionUnitValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas injection operation.
    /// </summary>
    public class GasInjection : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string GasTypeValue = string.Empty;

        public string GasType

        {

            get { return this.GasTypeValue; }

            set { SetProperty(ref GasTypeValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? GasInjectionRateValue;

        public decimal? GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private decimal? ProductionIncreaseValue;

        public decimal? ProductionIncrease

        {

            get { return this.ProductionIncreaseValue; }

            set { SetProperty(ref ProductionIncreaseValue, value); }

        }
        private string? ProductionUnitValue;

        public string? ProductionUnit

        {

            get { return this.ProductionUnitValue; }

            set { SetProperty(ref ProductionUnitValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating an enhanced recovery operation.
    /// </summary>
    public class CreateEnhancedRecoveryOperation : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string EORTypeValue = string.Empty;

        public string EORType

        {

            get { return this.EORTypeValue; }

            set { SetProperty(ref EORTypeValue, value); }

        }
        private DateTime? PlannedStartDateValue;

        public DateTime? PlannedStartDate

        {

            get { return this.PlannedStartDateValue; }

            set { SetProperty(ref PlannedStartDateValue, value); }

        }
        private decimal? PlannedInjectionRateValue;

        public decimal? PlannedInjectionRate

        {

            get { return this.PlannedInjectionRateValue; }

            set { SetProperty(ref PlannedInjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
    }
}







