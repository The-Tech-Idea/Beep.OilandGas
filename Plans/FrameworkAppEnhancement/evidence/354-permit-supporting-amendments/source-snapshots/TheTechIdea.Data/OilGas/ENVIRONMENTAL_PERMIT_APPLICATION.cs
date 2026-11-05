using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class ENVIRONMENTAL_PERMIT_APPLICATION : ModelEntityBase
    {
        private string _permitApplicationId = string.Empty;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string PERMIT_APPLICATION_ID { get => _permitApplicationId; set => SetProperty(ref _permitApplicationId, value); }

        private string? _remarks;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? REMARKS { get => _remarks; set => SetProperty(ref _remarks, value); }

        private string? _environmentalPermitType;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? ENVIRONMENTAL_PERMIT_TYPE { get => _environmentalPermitType; set => SetProperty(ref _environmentalPermitType, value); }

        private string? _wasteType;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? WASTE_TYPE { get => _wasteType; set => SetProperty(ref _wasteType, value); }

        private decimal? _wasteVolume;
        public decimal? WASTE_VOLUME { get => _wasteVolume; set => SetProperty(ref _wasteVolume, value); }

        private string? _wasteVolumeUnit;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? WASTE_VOLUME_UNIT { get => _wasteVolumeUnit; set => SetProperty(ref _wasteVolumeUnit, value); }

        private string? _facilityLocation;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? FACILITY_LOCATION { get => _facilityLocation; set => SetProperty(ref _facilityLocation, value); }

        private string? _monitoringPlan;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? MONITORING_PLAN { get => _monitoringPlan; set => SetProperty(ref _monitoringPlan, value); }

        private string? _environmentalImpact;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? ENVIRONMENTAL_IMPACT { get => _environmentalImpact; set => SetProperty(ref _environmentalImpact, value); }

        private string? _mitigationPlan;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? MITIGATION_PLAN { get => _mitigationPlan; set => SetProperty(ref _mitigationPlan, value); }

        private string? _disposalMethod;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? DISPOSAL_METHOD { get => _disposalMethod; set => SetProperty(ref _disposalMethod, value); }

        private string? _waterSource;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? WATER_SOURCE { get => _waterSource; set => SetProperty(ref _waterSource, value); }

        private string? _normInvolvedInd;
        [System.ComponentModel.DataAnnotations.MaxLength(1)]
        public string? NORM_INVOLVED_IND { get => _normInvolvedInd; set => SetProperty(ref _normInvolvedInd, value); }

    }
}
