using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Aggregates accounting KPIs for dashboard display.
    /// Provides revenue summaries, cost breakdowns, and production accounting metrics
    /// scoped by field and time period.
    /// </summary>
    public class AccountingAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<AccountingAggregationService> _logger;

        /// <summary>
        /// Initializes the accounting aggregation service with BeepDM dependencies.
        /// </summary>
        public AccountingAggregationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<AccountingAggregationService>? logger = null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string t) =>
            new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(T), _connectionName, t);

        public async Task<RevenueSummary> GetRevenueSummaryAsync(string? fieldId=null,DateTime? start=null,DateTime? end=null)
        {
            var r=new RevenueSummary();
            var repo=GetRepo<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");
            var f=new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            if(!string.IsNullOrWhiteSpace(fieldId))f.Add(new(){FieldName="FIELD_ID",Operator="=",FilterValue=fieldId});
            if(start.HasValue)f.Add(new(){FieldName="TRANSACTION_DATE",Operator=">=",FilterValue=start.Value.ToString("yyyy-MM-dd")});
            if(end.HasValue)f.Add(new(){FieldName="TRANSACTION_DATE",Operator="<=",FilterValue=end.Value.ToString("yyyy-MM-dd")});
            var entities=(await repo.GetAsync(f)).OfType<REVENUE_TRANSACTION>().ToList();
            if(!entities.Any())return r;
            r.TotalRevenue=entities.Sum(e=>e.GROSS_REVENUE??0);
            r.TransactionCount=entities.Count;
            r.ByProduct=entities.GroupBy(e=>e.REVENUE_TYPE??"Unknown").ToDictionary(g=>g.Key,g=>g.Sum(e=>e.GROSS_REVENUE??0));
            r.AveragePrice=entities.Any(e=>(e.OIL_VOLUME??0)>0)
                ?entities.Where(e=>(e.OIL_VOLUME??0)>0).Average(e=>(e.GROSS_REVENUE??0)/(e.OIL_VOLUME??1)):0;
            var ordered=entities.OrderBy(e=>e.TRANSACTION_DATE).ToList();
            var mid=ordered.Count/2;
            var fh=ordered.Take(mid).Sum(e=>e.GROSS_REVENUE??0);
            var sh=ordered.Skip(mid).Sum(e=>e.GROSS_REVENUE??0);
            r.Trend=fh>0?(sh-fh)/fh*100:0;
            return r;
        }

        public async Task<CostSummary> GetCostSummaryAsync(string? fieldId=null,DateTime? start=null,DateTime? end=null)
        {
            var r=new CostSummary();
            var repo=GetRepo<COST_TRANSACTION>("COST_TRANSACTION");
            var f=new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            if(!string.IsNullOrWhiteSpace(fieldId))f.Add(new(){FieldName="FIELD_ID",Operator="=",FilterValue=fieldId});
            if(start.HasValue)f.Add(new(){FieldName="TRANSACTION_DATE",Operator=">=",FilterValue=start.Value.ToString("yyyy-MM-dd")});
            if(end.HasValue)f.Add(new(){FieldName="TRANSACTION_DATE",Operator="<=",FilterValue=end.Value.ToString("yyyy-MM-dd")});
            var entities=(await repo.GetAsync(f)).OfType<COST_TRANSACTION>().ToList();
            if(!entities.Any())return r;
            r.TotalCost=entities.Sum(e=>e.AMOUNT??0);
            r.ByCostCenter=entities.GroupBy(e=>e.COST_CENTER_ID??"Unknown").ToDictionary(g=>g.Key,g=>g.Sum(e=>e.AMOUNT??0));
            r.LOE=entities.Where(e=>string.Equals(e.IS_EXPENSED,"Y",StringComparison.OrdinalIgnoreCase)).Sum(e=>e.AMOUNT??0);
            r.CAPEX=entities.Where(e=>string.Equals(e.IS_CAPITALIZED,"Y",StringComparison.OrdinalIgnoreCase)).Sum(e=>e.AMOUNT??0);
            r.TransactionCount=entities.Count;
            return r;
        }

        public async Task<RoyaltySummary> GetRoyaltySummaryAsync(string? fieldId=null,DateTime? start=null,DateTime? end=null)
        {
            var r=new RoyaltySummary();
            var repo=GetRepo<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");
            var f=new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            if(!string.IsNullOrWhiteSpace(fieldId))f.Add(new(){FieldName="PROPERTY_OR_LEASE_ID",Operator="=",FilterValue=fieldId});
            if(start.HasValue)f.Add(new(){FieldName="CALCULATION_DATE",Operator=">=",FilterValue=start.Value.ToString("yyyy-MM-dd")});
            if(end.HasValue)f.Add(new(){FieldName="CALCULATION_DATE",Operator="<=",FilterValue=end.Value.ToString("yyyy-MM-dd")});
            var entities=(await repo.GetAsync(f)).OfType<ROYALTY_CALCULATION>().ToList();
            if(!entities.Any())return r;
            r.TotalRoyalties=entities.Sum(e=>e.ROYALTY_AMOUNT??0);
            r.CalculationCount=entities.Count;
            r.PendingPayments=entities.Count(e=>string.Equals(e.PAYMENT_STATUS,"PENDING",StringComparison.OrdinalIgnoreCase));
            return r;
        }

        public async Task<List<AfeSummary>> GetAFESummaryAsync(string? fieldId=null)
        {
            var r=new List<AfeSummary>();
            var repo=GetRepo<AFE>("AFE");
            var f=new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            if(!string.IsNullOrWhiteSpace(fieldId))f.Add(new(){FieldName="FIELD_ID",Operator="=",FilterValue=fieldId});
            var entities=(await repo.GetAsync(f)).OfType<AFE>().ToList();
            foreach(var afe in entities)
                r.Add(new AfeSummary{AfeId=afe.AFE_ID??"N/A",Description=afe.DESCRIPTION??"",Budget=afe.ESTIMATED_COST??0,Spent=afe.ACTUAL_COST??0,Remaining=(afe.ESTIMATED_COST??0)-(afe.ACTUAL_COST??0),Status=afe.STATUS??"UNKNOWN",ApprovedDate=afe.APPROVAL_DATE});
            return r.OrderByDescending(a=>a.Budget).ToList();
        }

        public async Task<PeriodCloseStatus> GetPeriodCloseStatusAsync(string? fieldId=null)
        {
            var r=new PeriodCloseStatus();
            var repo=GetRepo<JOURNAL_ENTRY>("JOURNAL_ENTRY");
            var f=new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"},new(){FieldName="ENTRY_TYPE",Operator="=",FilterValue="PERIOD_CLOSE"}};
            var entries=(await repo.GetAsync(f)).OfType<JOURNAL_ENTRY>().OrderByDescending(e=>e.ENTRY_DATE).ToList();
            r.LastCloseDate=entries.FirstOrDefault()?.ENTRY_DATE;
            r.DaysSinceLastClose=r.LastCloseDate.HasValue?(int)(DateTime.UtcNow-r.LastCloseDate.Value).TotalDays:365;
            r.ClosedPeriods=entries.Count(e=>string.Equals(e.STATUS,"POSTED",StringComparison.OrdinalIgnoreCase));
            r.OpenPeriods=entries.Count(e=>string.Equals(e.STATUS,"OPEN",StringComparison.OrdinalIgnoreCase)||string.Equals(e.STATUS,"DRAFT",StringComparison.OrdinalIgnoreCase));
            return r;
        }
    }

    public class RevenueSummary{public decimal TotalRevenue{get;set;}public int TransactionCount{get;set;}public Dictionary<string,decimal> ByProduct{get;set;}=new();public decimal AveragePrice{get;set;}public decimal Trend{get;set;}}
    public class CostSummary{public decimal TotalCost{get;set;}public decimal LOE{get;set;}public decimal CAPEX{get;set;}public Dictionary<string,decimal> ByCostCenter{get;set;}=new();public int TransactionCount{get;set;}}
    public class RoyaltySummary{public decimal TotalRoyalties{get;set;}public int CalculationCount{get;set;}public int PendingPayments{get;set;}}
    public class AfeSummary{public string AfeId{get;set;}="";public string Description{get;set;}="";public decimal Budget{get;set;}public decimal Spent{get;set;}public decimal Remaining{get;set;}public string Status{get;set;}="";public DateTime? ApprovedDate{get;set;}}
    public class PeriodCloseStatus{public DateTime? LastCloseDate{get;set;}public int DaysSinceLastClose{get;set;}public int OpenPeriods{get;set;}public int ClosedPeriods{get;set;}}
}
