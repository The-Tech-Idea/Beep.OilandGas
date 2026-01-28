using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class CreateDatabaseRequest : ModelEntityBase
    {
        /// <summary>
        /// Connection configuration
        /// </summary>
        private ConnectionProperties ConnectionValue = null!;

        public ConnectionProperties Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// Database creation options
        /// </summary>
        private DatabaseCreationOptions OptionsValue = null!;

        public DatabaseCreationOptions Options

        {

            get { return this.OptionsValue; }

            set { SetProperty(ref OptionsValue, value); }

        }
    }
}
