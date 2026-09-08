using System;
using Beep.OilandGas.Models.Core.Interfaces;
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
using HseIncident=Beep.OilandGas.PPDM39.Models.HSE_INCIDENT;

namespace Beep.OilandGas.ApiService.Services
{
    public class HseAggregationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<HseAggregationService> _logger;

        public HseAggregationService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,
            string connectionName="PPDM39",ILogger<HseAggregationService>? logger=null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string t)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,t);

        public async Task<IncidentSummary> GetIncidentSummaryAsync(string? fieldId=null,DateTime? start=null,DateTime? end=null)
        {
            var r=new IncidentSummary();
            try{
                var repo=GetRepo<HseIncident>("HSE_INCIDENT");
                var f=new List<AppFilter>{new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
                if(start.HasValue)f.Add(new(){FieldName="INCIDENT_DATE",Operator=">=",FilterValue=start.Value.ToString("yyyy-MM-dd")});
                if(end.HasValue)f.Add(new(){FieldName="INCIDENT_DATE",Operator="<=",FilterValue=end.Value.ToString("yyyy-MM-dd")});
                var entities=(await repo.GetAsync(f)).OfType<HseIncident>().ToList();
                r.Total=entities.Count;
                // PPDM HSE_INCIDENT uses INCIDENT_CLASS_ID for classification
                r.Tier1=entities.Count(e=>string.Equals(e.INCIDENT_CLASS_ID,"TIER_1",StringComparison.OrdinalIgnoreCase));
                r.Tier2=entities.Count(e=>string.Equals(e.INCIDENT_CLASS_ID,"TIER_2",StringComparison.OrdinalIgnoreCase));
            }catch(Exception ex){_logger?.LogWarning(ex,"Failed to load HSE summary");}
            return r;
        }
    }

    public class IncidentSummary{public int Total{get;set;}public int Tier1{get;set;}public int Tier2{get;set;}public Dictionary<string,int> ByType{get;set;}=new();}
}
