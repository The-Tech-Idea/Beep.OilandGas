using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models
{
    public class ComponentSpecification
    {
        public List<string> ComponentNames { get; set; } = new List<string>();
    }

    public class DefaultMap
    {
        public ComponentSpecification ComponentSpecification { get; set; }
        public string SymbolFile { get; set; }
    }
}

