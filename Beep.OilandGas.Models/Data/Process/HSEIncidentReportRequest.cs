using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Request to report a new HSE incident.
    /// Severity tiers follow API RP 754 (Process Safety Event Tiers).
    /// </summary>
    public class HSEIncidentReportRequest : ModelEntityBase
    {
        private string IncidentTypeValue = string.Empty;

        /// <summary>Incident type code (e.g. SPILL, FIRE, INJURY, NEAR_MISS)</summary>
        public string IncidentType
        {
            get { return this.IncidentTypeValue; }
            set { SetProperty(ref IncidentTypeValue, value); }
        }

        private string SeverityValue = string.Empty;

        /// <summary>API RP 754 tier: TIER1, TIER2, TIER3, TIER4</summary>
        public string Severity
        {
            get { return this.SeverityValue; }
            set { SetProperty(ref SeverityValue, value); }
        }

        private string LocationDescriptionValue = string.Empty;

        public string LocationDescription
        {
            get { return this.LocationDescriptionValue; }
            set { SetProperty(ref LocationDescriptionValue, value); }
        }

        private string DescriptionValue = string.Empty;

        public string Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private DateTime IncidentDateTimeValue = DateTime.UtcNow;

        public DateTime IncidentDateTime
        {
            get { return this.IncidentDateTimeValue; }
            set { SetProperty(ref IncidentDateTimeValue, value); }
        }

        private string InjuredPartyIdValue = string.Empty;

        /// <summary>Business Associate ID of injured/affected party (maps to HSE_INCIDENT_BA)</summary>
        public string InjuredPartyId
        {
            get { return this.InjuredPartyIdValue; }
            set { SetProperty(ref InjuredPartyIdValue, value); }
        }

        private List<string> WitnessIdsValue = new();

        public List<string> WitnessIds
        {
            get { return this.WitnessIdsValue; }
            set { SetProperty(ref WitnessIdsValue, value); }
        }
    }
}
