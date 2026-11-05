using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class INJECTION_PERMIT_APPLICATION : ModelEntityBase
    {
        private string _permitApplicationId = string.Empty;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string PERMIT_APPLICATION_ID { get => _permitApplicationId; set => SetProperty(ref _permitApplicationId, value); }

        private string? _remarks;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? REMARKS { get => _remarks; set => SetProperty(ref _remarks, value); }

        private string? _injectionType;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? INJECTION_TYPE { get => _injectionType; set => SetProperty(ref _injectionType, value); }

        private string? _injectionZone;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? INJECTION_ZONE { get => _injectionZone; set => SetProperty(ref _injectionZone, value); }

        private string? _injectionFluid;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? INJECTION_FLUID { get => _injectionFluid; set => SetProperty(ref _injectionFluid, value); }

        private decimal? _maximumInjectionPressure;
        public decimal? MAXIMUM_INJECTION_PRESSURE { get => _maximumInjectionPressure; set => SetProperty(ref _maximumInjectionPressure, value); }

        private decimal? _maximumInjectionRate;
        public decimal? MAXIMUM_INJECTION_RATE { get => _maximumInjectionRate; set => SetProperty(ref _maximumInjectionRate, value); }

        private string? _injectionPressureUnit;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? INJECTION_PRESSURE_UNIT { get => _injectionPressureUnit; set => SetProperty(ref _injectionPressureUnit, value); }

        private string? _injectionRateUnit;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? INJECTION_RATE_UNIT { get => _injectionRateUnit; set => SetProperty(ref _injectionRateUnit, value); }

        private string? _monitoringRequirements;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? MONITORING_REQUIREMENTS { get => _monitoringRequirements; set => SetProperty(ref _monitoringRequirements, value); }

        private string? _isCo2StorageInd;
        [System.ComponentModel.DataAnnotations.MaxLength(1)]
        public string? IS_CO2_STORAGE_IND { get => _isCo2StorageInd; set => SetProperty(ref _isCo2StorageInd, value); }

        private string? _isGasStorageInd;
        [System.ComponentModel.DataAnnotations.MaxLength(1)]
        public string? IS_GAS_STORAGE_IND { get => _isGasStorageInd; set => SetProperty(ref _isGasStorageInd, value); }

        private string? _isBrineMiningInd;
        [System.ComponentModel.DataAnnotations.MaxLength(1)]
        public string? IS_BRINE_MINING_IND { get => _isBrineMiningInd; set => SetProperty(ref _isBrineMiningInd, value); }
    }
}
