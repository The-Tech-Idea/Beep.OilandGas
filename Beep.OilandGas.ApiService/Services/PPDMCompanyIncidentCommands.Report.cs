using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyIncidentCommands
{
    /// <summary>Records one original report and its PPDM incident graph in an owned serializable transaction.</summary>
    public async Task<string> CreateReportAsync(IncidentReportIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent); intent.Require();
        var review = intent.Context; var actor = review.ActorUserId;
        var authority = await Canonical.CaptureHseReportAuthorityAsync(selections, review, actor);
        async Task RequireCurrentAsync()
        {
            if (await Canonical.CaptureHseReportAuthorityAsync(selections, review, actor) != authority)
                throw RefusalException.Conflict("Incident reporting authority changed.");
            var options = await ReviewReportOptionsAsync(review);
            if (options.Revision != intent.OptionsRevision || !options.Options.Contains(intent.Option))
                throw RefusalException.Conflict("Recorded incident options changed. Review the report again.");
        }
        return await new PPDMIncidentReportWriter(company, editor, columns, defaults, metadata, policy)
            .CreateAsync(intent, RequireCurrentAsync);
    }
}
