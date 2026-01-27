using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Represents strongly typed step data for a process step instance.
    /// Replaces Dictionary<string, object> StepData.
    /// </summary>
    public class PROCESS_STEP_DATA : ModelEntityBase
    {
        private string StepInstanceIdValue = string.Empty;
        public string StepInstanceId
        {
            get => StepInstanceIdValue;
            set => SetProperty(ref StepInstanceIdValue, value);
        }

        private string DataJsonValue = string.Empty;
        public string DataJson
        {
            get => DataJsonValue;
            set => SetProperty(ref DataJsonValue, value);
        }

        private DateTime? LastUpdatedValue;
        public DateTime? LastUpdated
        {
            get => LastUpdatedValue;
            set => SetProperty(ref LastUpdatedValue, value);
        }
        public string StepType { get; set; }
        public WorkOrderUpdateRequest WorkOrderUpdate { get; set; }
        public WorkOrderCreationRequest WorkOrderCreation { get; set; }
        public WellTestRequest WellTest { get; set; }
        public ProductionForecast ProductionForecast { get; set; }
        public EconomicAnalysisRequest EconomicAnalysis { get; set; }
        public DeclineCurveAnalysisRequest DeclineCurveAnalysis { get; set; }
        public DCARequest DCA { get; set; }
        public DailyOperationsRequest DailyOperations { get; set; }
        public FieldOperationsRequest FieldOperations { get; set; }
        public FieldCreationRequest FieldCreation { get; set; }
        public FieldPerformanceRequest FieldPerformance { get; set; }
        public FieldPlanningRequest FieldPlanning { get; set; }
        public GenerateOperationalReportRequest GenerateOperationalReport { get; set; }
        public string Status { get; set; }
    }
}
