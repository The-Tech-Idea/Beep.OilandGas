using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: AllocationResult and AllocationDetail are defined in AllocationModelsDto.cs
    // This file contains request classes for allocation operations.

    /// <summary>
    /// Request to perform allocation
    /// </summary>
    public class AllocationRequest : ModelEntityBase
    {
        private DateTime AllocationDateValue;

        [Required]
        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }
        private AllocationMethod MethodValue;

        [Required]
        public AllocationMethod Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }
        private decimal TotalVolumeValue;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalVolume

        {

            get { return this.TotalVolumeValue; }

            set { SetProperty(ref TotalVolumeValue, value); }

        }
        private List<AllocationEntityRequest> EntitiesValue = new();

        [Required]
        public List<AllocationEntityRequest> Entities

        {

            get { return this.EntitiesValue; }

            set { SetProperty(ref EntitiesValue, value); }

        }
        private string AllocationRequest_IDValue;

        public string AllocationRequest_ID

        {

            get { return this.AllocationRequest_IDValue; }

            set { SetProperty(ref AllocationRequest_IDValue, value); }

        }
        private string RUN_TICKET_IDValue;

        public string RUN_TICKET_ID

        {

            get { return this.RUN_TICKET_IDValue; }

            set { SetProperty(ref RUN_TICKET_IDValue, value); }

        }
        private DateTime ALLOCATION_DATEValue;

        public DateTime ALLOCATION_DATE

        {

            get { return this.ALLOCATION_DATEValue; }

            set { SetProperty(ref ALLOCATION_DATEValue, value); }

        }
        private string ALLOCATION_METHODValue;

        public string ALLOCATION_METHOD

        {

            get { return this.ALLOCATION_METHODValue; }

            set { SetProperty(ref ALLOCATION_METHODValue, value); }

        }
        private decimal TOTAL_VOLUMEValue;

        public decimal TOTAL_VOLUME

        {

            get { return this.TOTAL_VOLUMEValue; }

            set { SetProperty(ref TOTAL_VOLUMEValue, value); }

        }
        private decimal ALLOCATED_VOLUMEValue;

        public decimal ALLOCATED_VOLUME

        {

            get { return this.ALLOCATED_VOLUMEValue; }

            set { SetProperty(ref ALLOCATED_VOLUMEValue, value); }

        }
        private decimal ALLOCATION_VARIANCEValue;

        public decimal ALLOCATION_VARIANCE

        {

            get { return this.ALLOCATION_VARIANCEValue; }

            set { SetProperty(ref ALLOCATION_VARIANCEValue, value); }

        }

    }

    /// <summary>
    /// Request for allocation entity
    /// </summary>
    public class AllocationEntityRequest : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        [Required]
        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? EntityNameValue;

        public string? EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }
        private decimal? WorkingInterestValue;

        public decimal? WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal? NetRevenueInterestValue;

        public decimal? NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal? ProductionHistoryValue;

        public decimal? ProductionHistory

        {

            get { return this.ProductionHistoryValue; }

            set { SetProperty(ref ProductionHistoryValue, value); }

        }
    }

    /// <summary>
    /// Request for volume reconciliation
    /// </summary>
    public class VolumeReconciliationRequest : ModelEntityBase
    {
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        private DateTime StartDateValue;

        
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate

        
        {

        
            get { return this.StartDateValue; }

        
            set { SetProperty(ref StartDateValue, value); }

        
        }
        
        private DateTime EndDateValue;

        
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate

        
        {

        
            get { return this.EndDateValue; }

        
            set { SetProperty(ref EndDateValue, value); }

        
        }
    }
}


