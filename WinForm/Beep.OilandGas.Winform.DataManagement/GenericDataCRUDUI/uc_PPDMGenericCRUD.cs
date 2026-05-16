using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace Beep.OilandGas.Winform.DataManagement.GenericDataCRUDUI
{
    public partial class uc_PPDMGenericCRUD : TemplateUserControl
    {
        public uc_PPDMGenericCRUD()
        {
            InitializeComponent();
        }
        public uc_PPDMGenericCRUD(IServiceProvider services) : base(services)
        {
            InitializeComponent();

            Details.AddinName = "PPDM Generic CRUD";

        }
        public override void Configure(Dictionary<string, object> settings)
        {
            // Call the base class's Configure method to ensure any necessary setup is performed
            base.Configure(settings);
            // Add any additional configuration logic specific to this user control here
        }
        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            // Call the base class's OnNavigatedTo method to ensure any necessary setup is performed
            base.OnNavigatedTo(parameters);
            // Add any additional logic to handle when the user control is navigated to here
        }
    }
}
