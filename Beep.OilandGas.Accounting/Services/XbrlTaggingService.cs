using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.OilandGas.Accounting.Services
{
    public class XbrlTaggingService
    {
        // In a real system, this would load from a taxonomy (XSD/Linkbase)
        private readonly Dictionary<string, string> _defaultMappings = new Dictionary<string, string>
        {
            { "1000", "ifrs-full:CashAndCashEquivalents" },
            { "1100", "ifrs-full:TradeAndOtherReceivables" },
            { "2000", "ifrs-full:TradeAndOtherPayables" },
            { "4000", "ifrs-full:Revenue" },
            { "5000", "ifrs-full:CostOfSales" }
        };

        public XbrlTaggingService()
        {
        }

        public string ApplyXbrlTags(Dictionary<string, decimal> trialBalance)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<xbrli:xbrl xmlns:ifrs-full=\"http://xbrl.ifrs.org/taxonomy/2021-03-24/ifrs-full\" xmlns:xbrli=\"http://www.xbrl.org/2003/instance\">");
            sb.AppendLine($"  <xbrli:context id=\"CurrentPeriod\">");
            sb.AppendLine($"    <xbrli:entity><xbrli:identifier scheme=\"http://www.beep.com\">SampleCompany</xbrli:identifier></xbrli:entity>");
            sb.AppendLine($"    <xbrli:period><xbrli:instant>{DateTime.Today:yyyy-MM-dd}</xbrli:instant></xbrli:period>");
            sb.AppendLine($"  </xbrli:context>");
            
            foreach (var kvp in trialBalance)
            {
                var accountCode = kvp.Key;
                var amount = kvp.Value;

                // Simple check for mapping
                // In reality, we would lookup based on account properties or a DB table 'XBRL_MAPPING'
                var tag = _defaultMappings.ContainsKey(accountCode) ? _defaultMappings[accountCode] : $"custom:{accountCode}";

                sb.AppendLine($"  <{tag} contextRef=\"CurrentPeriod\" unitRef=\"USD\" decimals=\"2\">{amount}</{tag}>");
            }

            sb.AppendLine("</xbrli:xbrl>");
            return sb.ToString();
        }

        public async Task<string> GenerateXbrlDocumentAsync(List<XbrlMapping> mappings, Dictionary<string, decimal> accountBalances)
        {
            await Task.Yield(); // Simulating async work
            
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<xbrli:xbrl xmlns:ifrs-full=\"http://xbrl.ifrs.org/taxonomy/2021-03-24/ifrs-full\" xmlns:xbrli=\"http://www.xbrl.org/2003/instance\">");
           
            foreach (var kvp in accountBalances)
            {
                var mapping = mappings.FirstOrDefault(m => m.GL_ACCOUNT_CODE == kvp.Key);
                var tag = mapping?.XBRL_TAG ?? $"custom:{kvp.Key}";
                
                sb.AppendLine($"  <{tag} contextRef=\"CurrentPeriod\" unitRef=\"USD\" decimals=\"2\">{kvp.Value}</{tag}>");
            }

            sb.AppendLine("</xbrli:xbrl>");
            return sb.ToString();
        }
    }

    public class XbrlMapping
    {
        public string XBRL_MAPPING_ID { get; set; }
        public string GL_ACCOUNT_CODE { get; set; }
        public string XBRL_TAG { get; set; }
        public string TAXONOMY_VERSION { get; set; }
    }
}
