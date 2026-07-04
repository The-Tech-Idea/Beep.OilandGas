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
    public class ProductionEngineerAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ProductionEngineerAggregationService> _logger;
        public ProductionEngineerAggregationService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,
            string connectionName="PPDM39",ILogger<ProductionEngineerAggregationService>? logger=null)
        {
            _editor = editor
            _commonColumnHandler=commonColumnHandler
            _defaults=defaults
            _metadata=metadata
            _connectionName=connectionName_logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string t)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,t);

        public async Task<ProductionEngineerKpi> GetKpiAsync(string? fieldId=null)
        {
            var kpi=new ProductionEngineerKpi();
            try{
                var af=new AppFilter{FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"};
                // Active wells
                var wells=(await GetRepo<PPDM.WELL>("WELL").GetAsync(new List<AppFilter>{af})).OfType<PPDM.WELL>().ToList();
                kpi.ActiveWells=wells.Count;
                // Daily production
                var pden=(await GetRepo<PPDM.PDEN_VOL_SUMMARY>("PDEN_VOL_SUMMARY").GetAsync(new List<AppFilter>{af})).OfType<PPDM.PDEN_VOL_SUMMARY>().ToList();
                kpi.AvgDailyBoe=pden.Sum(e=>(e.OIL_VOLUME??0)+((e.GAS_VOLUME??0)/6m));
                if(pden.Count>0)kpi.AvgDailyBoe/=Math.Max(1,pden.Count);
            }catch(Exception ex){_logger?.LogWarning(ex,"Failed to build production engineer KPI");}
            return kpi;
        }
    }

    public class ProductionEngineerKpi{public int ActiveWells{get;set;}public decimal AvgDailyBoe{get;set;}public int DowntimeEvents{get;set;}public int PendingWorkovers{get;set;}}
}
