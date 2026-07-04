using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using PPDM=Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Services
{
    public class ExecutiveAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ExecutiveAggregationService> _logger;

        public ExecutiveAggregationService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,
            string connectionName="PPDM39",ILogger<ExecutiveAggregationService>? logger=null)
        {
            _editor = editor
            _commonColumnHandler=commonColumnHandler
            _defaults=defaults
            _metadata=metadata
            _connectionName=connectionName_logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string t)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,t);

        public async Task<ExecutiveKpi> GetExecutiveKpiAsync()
        {
            var kpi=new ExecutiveKpi();
            try{
                // Production from PDEN_VOL_SUMMARY
                var pdenRepo=GetRepo<PPDM.PDEN_VOL_SUMMARY>("PDEN_VOL_SUMMARY");
                var activeFilter=new AppFilter{FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"};
                var monthStart=new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month,1);
                // PDEN uses EFFECTIVE_DATE for production period tracking (no PRODUCTION_DATE column)
                var pdenFilters=new List<AppFilter>{activeFilter,new(){FieldName="EFFECTIVE_DATE",Operator=">=",FilterValue=monthStart.ToString("yyyy-MM-dd")}};
                var pdenEntities=(await pdenRepo.GetAsync(pdenFilters)).OfType<PPDM.PDEN_VOL_SUMMARY>().ToList();
                kpi.TotalProductionBoe=pdenEntities.Sum(e=>(e.OIL_VOLUME??0)+((e.GAS_VOLUME??0)/6m));

                // Previous month trend
                var prevMonth=monthStart.AddMonths(-1);
                pdenFilters[1]=new AppFilter{FieldName="EFFECTIVE_DATE",Operator=">=",FilterValue=prevMonth.ToString("yyyy-MM-dd")};
                pdenFilters.Add(new AppFilter{FieldName="EFFECTIVE_DATE",Operator="<",FilterValue=monthStart.ToString("yyyy-MM-dd")});
                var prevEntities=(await pdenRepo.GetAsync(pdenFilters)).OfType<PPDM.PDEN_VOL_SUMMARY>().ToList();
                decimal prevBoe=prevEntities.Sum(e=>(e.OIL_VOLUME??0)+((e.GAS_VOLUME??0)/6m));
                kpi.ProductionTrend=prevBoe>0?((kpi.TotalProductionBoe-prevBoe)/prevBoe)*100:0;

                // Well counts
                var wellRepo=GetRepo<PPDM.WELL>("WELL");
                var wells=(await wellRepo.GetAsync(new List<AppFilter>{activeFilter})).OfType<PPDM.WELL>().ToList();
                kpi.TotalWells=wells.Count;

                // HSE incidents YTD
                var hseRepo=GetRepo<PPDM.HSE_INCIDENT>("HSE_INCIDENT");
                var yearStart=new DateTime(DateTime.UtcNow.Year,1,1);
                var hseFilters=new List<AppFilter>{activeFilter,new(){FieldName="INCIDENT_DATE",Operator=">=",FilterValue=yearStart.ToString("yyyy-MM-dd")}};
                var incidents=(await hseRepo.GetAsync(hseFilters)).OfType<PPDM.HSE_INCIDENT>().ToList();
                kpi.TotalIncidentsYtd=incidents.Count;
            }catch(Exception ex){_logger?.LogWarning(ex,"Failed to build executive KPI");}
            return kpi;
        }

        public async Task<List<AssetPerformance>> GetAssetPerformanceAsync()
        {
            var r=new List<AssetPerformance>();
            try{
                var repo=GetRepo<PPDM.FIELD>("FIELD");
                var fields=(await repo.GetAsync(new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}})).OfType<PPDM.FIELD>().ToList();
                foreach(var f in fields)
                    r.Add(new AssetPerformance{FieldId=f.FIELD_ID??"N/A",FieldName=f.FIELD_NAME??f.FIELD_ID??"Unknown"});
            }catch(Exception ex){_logger?.LogWarning(ex,"Failed to load asset performance");}
            return r.OrderByDescending(a=>a.FieldName).ToList();
        }
    }

    public class ExecutiveKpi{public decimal TotalProductionBoe{get;set;}public decimal ProductionTrend{get;set;}public int TotalWells{get;set;}public int ActiveWells{get;set;}public int Tier1Events{get;set;}public int TotalIncidentsYtd{get;set;}}
    public class AssetPerformance{public string FieldId{get;set;}="";public string FieldName{get;set;}="";public string LifecyclePhase{get;set;}="";public int ActiveWells{get;set;}}
}
