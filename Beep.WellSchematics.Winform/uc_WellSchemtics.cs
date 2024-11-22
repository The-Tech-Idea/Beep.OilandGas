
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml.Linq;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;

namespace Beep.WellSchematics.Winform
{
    [AddinAttribute(Caption = "Well Schematics", Name = "uc_WellSchemtics", misc = "OilandGas", menu = "OilandGas", displayType = DisplayType.Popup, addinType = AddinType.Control)]
    public partial class uc_WellSchemtics : UserControl,IDM_Addin
    {
        public uc_WellSchemtics()
        {
            InitializeComponent();
        }

        public string AddinName { get; set; } = "Well Schematics";
        public string Description { get; set; } = "Well Schematicsr";
        public string ObjectName { get; set; }
        public string ObjectType { get; set; } = "UserControl";
        public string DllName { get; set; }
        public string DllPath { get; set; }
        public string NameSpace { get; set; }
        public string ParentName { get; set; }
        public Boolean DefaultCreate { get; set; } = true;
        public IRDBSource DestConnection { get; set; }
        public DataSet Dset { get; set; }
        public IErrorsInfo ErrorObject { get; set; }
        public IDMLogger Logger { get; set; }
        public IRDBSource SourceConnection { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public string EntityName { get; set; }
        public IPassedArgs Passedarg { get; set; }
        public IUtil util { get; set; }
        public IVisManager Visutil { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        ITree tree;
        IBranch branch;
        public void Run(IPassedArgs pPassedarg)
        {
           
        }

        public void SetConfig(IDMEEditor pbl, IDMLogger plogger, IUtil putil, string[] args, IPassedArgs e, IErrorsInfo per)
        {
            Passedarg = e;
            Visutil = (IVisManager)e.Objects.Where(c => c.Name == "VISUTIL").FirstOrDefault().obj;
            Logger = plogger;
            DMEEditor = pbl;
            ErrorObject = per;
            tree = (ITree)Visutil.Tree;
           
        }
    }
}
