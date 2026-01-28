using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class SaveConnectionRequest : ModelEntityBase
    {
        /// <summary>
        /// Connection properties to save
        /// </summary>
        private ConnectionProperties? ConnectionValue;

        [Required]
        public ConnectionProperties? Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// If true, set this connection as the current active connection
        /// </summary>
        private bool SetAsCurrentValue = false;

        public bool SetAsCurrent

        {

            get { return this.SetAsCurrentValue; }

            set { SetProperty(ref SetAsCurrentValue, value); }

        }

        /// <summary>
        /// After saving, optionally test the connection
        /// </summary>
        private bool TestAfterSaveValue = true;

        public bool TestAfterSave

        {

            get { return this.TestAfterSaveValue; }

            set { SetProperty(ref TestAfterSaveValue, value); }

        }

        /// <summary>
        /// After saving, optionally open the connection in the editor
        /// </summary>
        private bool OpenAfterSaveValue = false;

        public bool OpenAfterSave

        {

            get { return this.OpenAfterSaveValue; }

            set { SetProperty(ref OpenAfterSaveValue, value); }

        }

        /// <summary>
        /// Original connection name when updating an existing connection
        /// </summary>
        private string? OriginalConnectionNameValue;

        public string? OriginalConnectionName

        {

            get { return this.OriginalConnectionNameValue; }

            set { SetProperty(ref OriginalConnectionNameValue, value); }

        }

        /// <summary>
        /// User performing the operation
        /// </summary>
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
