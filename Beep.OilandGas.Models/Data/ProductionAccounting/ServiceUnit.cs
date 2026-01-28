using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ServiceUnit : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the service unit identifier.
        /// </summary>
        private string ServiceUnitIdValue = string.Empty;

        public string ServiceUnitId

        {

            get { return this.ServiceUnitIdValue; }

            set { SetProperty(ref ServiceUnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the service unit name.
        /// </summary>
        private string ServiceUnitNameValue = string.Empty;

        public string ServiceUnitName

        {

            get { return this.ServiceUnitNameValue; }

            set { SetProperty(ref ServiceUnitNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string? PropertyOrLeaseIdValue;

        public string? PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the participants.
        /// </summary>
        private List<ServiceUnitParticipant> ParticipantsValue = new();

        public List<ServiceUnitParticipant> Participants

        {

            get { return this.ParticipantsValue; }

            set { SetProperty(ref ParticipantsValue, value); }

        }

        /// <summary>
        /// Gets or sets the test separator.
        /// </summary>
        private TestSeparator? TestSeparatorValue;

        public TestSeparator? TestSeparator

        {

            get { return this.TestSeparatorValue; }

            set { SetProperty(ref TestSeparatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the LACT unit.
        /// </summary>
        private LACTUnit? LACTUnitValue;

        public LACTUnit? LACTUnit

        {

            get { return this.LACTUnitValue; }

            set { SetProperty(ref LACTUnitValue, value); }

        }
    }
}
