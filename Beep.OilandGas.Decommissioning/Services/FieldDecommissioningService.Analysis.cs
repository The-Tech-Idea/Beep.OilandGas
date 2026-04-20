using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.Decommissioning;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Decommissioning.Services
{
    /// <summary>
    /// Engineering analysis partial for FieldDecommissioningService.
    /// Provides well plugging requirement analysis, decommissioning cost models,
    /// environmental remediation assessment, regulatory compliance checks,
    /// salvage value estimation, and portfolio-level analysis.
    ///
    /// These methods are pure engineering calculations; they do not require
    /// PPDM database access and can be called independently of the CRUD partial.
    /// </summary>
    public partial class FieldDecommissioningService
    {
        // ---------------------------------------------------------------------------
        // WELL PLUGGING ANALYSIS
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Analyzes well plugging requirements and generates a comprehensive plugging plan.
        /// </summary>
        public async Task<WellPluggingPlan> AnalyzePluggingRequirementsAsync(
            string wellUWI,
            double wellDepth,
            List<string> zonesCased,
            double freshwaterAquiferDepth,
            double reservoirPressure)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI is required", nameof(wellUWI));
            if (wellDepth <= 0)
                throw new ArgumentException("Well depth must be positive", nameof(wellDepth));

            _logger?.LogInformation(
                "Analyzing plugging requirements: well={Well}, depth={Depth:F0} ft, zones={Zones}",
                wellUWI, wellDepth, zonesCased.Count);

            try
            {
                var result = new WellPluggingPlan
                {
                    WellUWI                  = wellUWI,
                    WellDepth                = wellDepth,
                    ZonesIdentified          = zonesCased.Count,
                    FreshwaterAquiferDepth   = freshwaterAquiferDepth,
                    AnalysisDate             = DateTime.UtcNow
                };

                result.CriticalZones       = IdentifyCriticalZones(zonesCased, freshwaterAquiferDepth, wellDepth);
                result.PluggingStrategy    = DeterminePluggingStrategy(wellDepth, freshwaterAquiferDepth, reservoirPressure, zonesCased.Count);
                result.CementRequirements  = CalculateCementRequirements(result.PluggingStrategy, wellDepth);
                result.PlugSpecifications  = GeneratePlugSpecifications(result.PluggingStrategy);
                result.EstimatedDaysRequired = EstimatePluggingDuration(wellDepth);
                result.PotentialIssues     = IdentifyPluggingIssues(reservoirPressure, wellDepth, zonesCased.Count);

                _logger?.LogInformation(
                    "Plugging analysis complete: {Zones} critical zones, est. {Days} days",
                    result.CriticalZones.Count, result.EstimatedDaysRequired);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing plugging requirements for well {Well}", wellUWI);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // DECOMMISSIONING COST ANALYSIS
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Calculates a detailed decommissioning and abandonment cost breakdown.
        /// </summary>
        public async Task<DecommissioningCostAnalysis> AnalyzeDecommissioningCostsAsync(
            string wellUWI,
            double wellDepth,
            string wellType,
            string location,
            bool requiresEnvironmentalRemediation = true)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI is required", nameof(wellUWI));

            _logger?.LogInformation(
                "Analyzing decommissioning costs: well={Well}, depth={Depth:F0} ft, type={Type}, location={Location}",
                wellUWI, wellDepth, wellType, location);

            try
            {
                var result = new DecommissioningCostAnalysis
                {
                    WellUWI      = wellUWI,
                    WellDepth    = wellDepth,
                    WellType     = wellType,
                    Location     = location,
                    AnalysisDate = DateTime.UtcNow
                };

                result.WellPluggingCost          = CalculateWellPluggingCost(wellDepth, wellType);
                result.WellheadRemovalCost        = CalculateWellheadRemovalCost(wellType);
                result.SiteRestorationCost        = CalculateSiteRestorationCost(location, wellDepth);

                if (requiresEnvironmentalRemediation)
                    result.EnvironmentalRemediationCost = CalculateEnvironmentalRemediationCost(location, wellDepth);

                result.AbandonmentBondCost        = CalculateAbandonmentBond(wellDepth, location);
                result.TotalEstimatedCost         = result.WellPluggingCost + result.WellheadRemovalCost
                                                    + result.SiteRestorationCost + result.EnvironmentalRemediationCost
                                                    + result.AbandonmentBondCost;

                double total = Math.Max(1, result.TotalEstimatedCost);
                result.PluggingCostPercentage      = result.WellPluggingCost   / total * 100;
                result.WellheadRemovalPercentage   = result.WellheadRemovalCost / total * 100;
                result.SiteRestorationPercentage   = result.SiteRestorationCost / total * 100;

                result.ContingencyAmount           = result.TotalEstimatedCost * 0.20;
                result.TotalWithContingency        = result.TotalEstimatedCost + result.ContingencyAmount;

                _logger?.LogInformation(
                    "Cost analysis complete: plugging={Plug:C}, restoration={Rest:C}, total={Total:C}",
                    result.WellPluggingCost, result.SiteRestorationCost, result.TotalWithContingency);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing decommissioning costs for well {Well}", wellUWI);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // ENVIRONMENTAL REMEDIATION ANALYSIS
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Assesses environmental remediation requirements for an abandoned well site.
        /// </summary>
        public async Task<EnvironmentalRemediationAnalysis> AnalyzeEnvironmentalRemediationAsync(
            string wellUWI,
            string location,
            List<string> potentialContaminants,
            double distanceToWaterSource)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI is required", nameof(wellUWI));

            _logger?.LogInformation(
                "Analyzing environmental remediation: well={Well}, location={Location}, dist. to water={Dist:F0} ft",
                wellUWI, location, distanceToWaterSource);

            try
            {
                var result = new EnvironmentalRemediationAnalysis
                {
                    WellUWI               = wellUWI,
                    Location              = location,
                    AnalysisDate          = DateTime.UtcNow,
                    PotentialContaminants = potentialContaminants ?? new List<string>()
                };

                result.EnvironmentalRiskLevel    = AssessEnvironmentalRisk(location, distanceToWaterSource, potentialContaminants);
                result.RemediationActivities     = DetermineRemediationActivities(result.EnvironmentalRiskLevel, potentialContaminants, location);
                result.EstimatedRemediationMonths = EstimateRemediationTimeline(result.RemediationActivities.Count, result.EnvironmentalRiskLevel);
                result.MonitoringPeriodYears     = CalculateMonitoringPeriod(result.EnvironmentalRiskLevel);
                result.LongTermLiabilityCost     = EstimateLongTermLiability(result.MonitoringPeriodYears, result.EnvironmentalRiskLevel);
                result.RegulatoryRequirements    = IdentifyRegulatoryRequirements(location, result.EnvironmentalRiskLevel, potentialContaminants);

                _logger?.LogInformation(
                    "Environmental analysis complete: risk={Risk}, months={Months}, monitoring={Years} yrs",
                    result.EnvironmentalRiskLevel, result.EstimatedRemediationMonths, result.MonitoringPeriodYears);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing environmental remediation for well {Well}", wellUWI);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // REGULATORY COMPLIANCE ANALYSIS
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Identifies applicable regulations and compliance requirements for well abandonment.
        /// Covers USA (BSEE 30 CFR 250, EPA 40 CFR 98), Canada (AER Directive), and OSPAR.
        /// </summary>
        public async Task<RegulatoryComplianceAnalysis> AnalyzeRegulatoryComplianceAsync(
            string wellUWI,
            string jurisdiction,
            string wellClass,
            DateTime abandonmentDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI is required", nameof(wellUWI));
            if (string.IsNullOrWhiteSpace(jurisdiction))
                throw new ArgumentException("Jurisdiction is required", nameof(jurisdiction));

            _logger?.LogInformation(
                "Analyzing regulatory compliance: well={Well}, jurisdiction={Jurisdiction}, class={Class}",
                wellUWI, jurisdiction, wellClass);

            try
            {
                var result = new RegulatoryComplianceAnalysis
                {
                    WellUWI         = wellUWI,
                    Jurisdiction    = jurisdiction,
                    WellClass       = wellClass,
                    AbandonmentDate = abandonmentDate,
                    AnalysisDate    = DateTime.UtcNow
                };

                result.ApplicableRegulations   = IdentifyApplicableRegulations(jurisdiction, wellClass);
                result.ComplianceRequirements  = DetermineComplianceRequirements(jurisdiction, wellClass, result.ApplicableRegulations);
                result.ComplianceDeadlineDate  = CalculateComplianceDeadline(abandonmentDate, jurisdiction);
                result.RequiredDocumentation   = IdentifyRequiredDocumentation(jurisdiction, wellClass);
                result.BondingRequirements     = AssessBondingRequirements(jurisdiction, wellClass, abandonmentDate);
                result.InspectionRequirements  = IdentifyInspectionRequirements(jurisdiction, wellClass);
                result.ComplianceCostEstimate  = EstimateComplianceCost(result.ComplianceRequirements.Count, jurisdiction);

                _logger?.LogInformation(
                    "Compliance analysis complete: jurisdiction={Jurisdiction}, requirements={Count}, deadline={Deadline:yyyy-MM-dd}",
                    jurisdiction, result.ComplianceRequirements.Count, result.ComplianceDeadlineDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing regulatory compliance for well {Well}", wellUWI);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // PORTFOLIO DECOMMISSIONING ANALYSIS
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Analyses decommissioning requirements across a portfolio of wells for a field.
        /// </summary>
        public async Task<PortfolioDecommissioningAnalysis> AnalyzeWellPortfolioDecommissioningAsync(
            string fieldId,
            List<string> wellUWIs,
            Dictionary<string, double> wellDepths)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID is required", nameof(fieldId));
            if (wellUWIs == null || wellUWIs.Count == 0)
                throw new ArgumentException("Well UWI list is required", nameof(wellUWIs));

            _logger?.LogInformation(
                "Analyzing portfolio decommissioning: field={Field}, wells={Count}", fieldId, wellUWIs.Count);

            try
            {
                var result = new PortfolioDecommissioningAnalysis
                {
                    FieldId              = fieldId,
                    WellsToDecommission  = wellUWIs.Count,
                    AnalysisDate         = DateTime.UtcNow,
                    WellAnalyses         = new List<PortfolioWellDecommissioning>()
                };

                double totalCost = 0;
                int    totalDays = 0;

                foreach (var uwi in wellUWIs)
                {
                    double depth = wellDepths.TryGetValue(uwi, out var d) ? d : 5000;
                    double cost  = CalculateWellPluggingCost(depth, "Oil");
                    int    days  = EstimatePluggingDuration(depth);

                    result.WellAnalyses.Add(new PortfolioWellDecommissioning
                    {
                        WellUWI       = uwi,
                        WellDepth     = depth,
                        EstimatedCost = cost,
                        EstimatedDays = days
                    });

                    totalCost += cost;
                    totalDays += days;
                }

                result.TotalEstimatedCost  = totalCost;
                result.TotalEstimatedDays  = totalDays;
                result.AverageCostPerWell  = totalCost / Math.Max(1, wellUWIs.Count);
                result.AverageDaysPerWell  = totalDays / Math.Max(1, wellUWIs.Count);
                result.PhaseCount          = RecommendDecommissioningPhases(wellUWIs.Count);
                result.WellsPerPhase       = (int)Math.Ceiling(wellUWIs.Count / (double)result.PhaseCount);
                result.ContingencyPercentage = 20.0;
                result.TotalWithContingency = result.TotalEstimatedCost * 1.20;

                _logger?.LogInformation(
                    "Portfolio analysis complete: {Count} wells, total={Total:C}, phases={Phases}",
                    wellUWIs.Count, result.TotalWithContingency, result.PhaseCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing portfolio decommissioning for field {Field}", fieldId);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // PRIVATE CALCULATION HELPERS
        // ---------------------------------------------------------------------------

        private List<string> IdentifyCriticalZones(List<string> zones, double fwAquifer, double wellDepth)
        {
            var critical = new List<string> { "Surface casing shoe" };
            if (fwAquifer > 0 && fwAquifer < wellDepth)
                critical.Add("Freshwater aquifer zone");
            critical.Add("Intermediate zone");
            if (zones.Count > 2)
                critical.Add("Production zone");
            return critical;
        }

        private string DeterminePluggingStrategy(double wellDepth, double fwAquifer, double pressure, int zoneCount)
        {
            if (wellDepth < 3_000) return "Balanced Cement Plugs";
            if (wellDepth < 10_000) return "Staged Cement Plugs with Bridge Plugs";
            return "Multi-stage Plugging with Expandable Packers";
        }

        private double CalculateCementRequirements(string strategy, double wellDepth)
            => (wellDepth / 75.0) * 94; // sacks × 94 lb = lbs of cement

        private List<string> GeneratePlugSpecifications(string strategy) =>
            new List<string>
            {
                "API Class G Cement",
                "Density: 15.8 ppg",
                "Slurry volume calculations per zone",
                "WOC times per API RP 65 Part 2"
            };

        private int EstimatePluggingDuration(double wellDepth)
            => (int)Math.Ceiling(wellDepth / 5_000.0 * 10);

        private List<string> IdentifyPluggingIssues(double pressure, double depth, int zoneCount)
        {
            var issues = new List<string>();
            if (pressure > 5_000) issues.Add("High reservoir pressure — kick prevention required");
            if (depth > 15_000) issues.Add("Extended reach — increased cement job complexity");
            if (zoneCount > 4) issues.Add("Multiple zone isolation required");
            return issues;
        }

        private double CalculateWellPluggingCost(double depth, string wellType)
        {
            double baseRate = wellType?.ToUpperInvariant() == "GAS" ? 180 : 150; // $USD per foot
            return depth * baseRate;
        }

        private double CalculateWellheadRemovalCost(string wellType)
            => wellType?.ToUpperInvariant() == "OFFSHORE" ? 250_000 : 35_000;

        private double CalculateSiteRestorationCost(string location, double depth)
        {
            bool isOffshore = location?.ToUpperInvariant().Contains("OFFSHORE") == true;
            return isOffshore ? depth * 50 : depth * 20;
        }

        private double CalculateEnvironmentalRemediationCost(string location, double depth)
            => depth * 10;

        private double CalculateAbandonmentBond(double depth, string location)
            => depth * 5;

        private string AssessEnvironmentalRisk(string location, double distanceToWater, List<string>? contaminants)
        {
            int score = 0;
            if (distanceToWater < 500) score += 3;
            else if (distanceToWater < 2_000) score += 1;
            if (contaminants?.Count > 3) score += 2;
            return score >= 4 ? "HIGH" : score >= 2 ? "MEDIUM" : "LOW";
        }

        private List<string> DetermineRemediationActivities(string riskLevel, List<string>? contaminants, string location)
        {
            var activities = new List<string> { "Site assessment", "Soil sampling" };
            if (riskLevel == "HIGH") activities.Add("Contaminated soil excavation");
            if (riskLevel != "LOW") activities.Add("Groundwater monitoring installation");
            activities.Add("Revegetation");
            return activities;
        }

        private int EstimateRemediationTimeline(int activityCount, string riskLevel)
            => riskLevel == "HIGH" ? activityCount * 4 : activityCount * 2;

        private int CalculateMonitoringPeriod(string riskLevel)
            => riskLevel == "HIGH" ? 5 : riskLevel == "MEDIUM" ? 3 : 1;

        private double EstimateLongTermLiability(int monitoringYears, string riskLevel)
            => monitoringYears * (riskLevel == "HIGH" ? 50_000 : 20_000);

        private List<string> IdentifyRegulatoryRequirements(string location, string riskLevel, List<string>? contaminants)
        {
            var reqs = new List<string> { "EPA closure report", "State agency notification" };
            if (riskLevel == "HIGH") reqs.Add("Corrective action plan submission");
            return reqs;
        }

        private List<string> IdentifyApplicableRegulations(string jurisdiction, string wellClass)
        {
            var regs = new List<string>();
            switch (jurisdiction.ToUpperInvariant())
            {
                case "USA":
                    regs.Add("BSEE 30 CFR Part 250 (offshore)");
                    regs.Add("API RP 65 Part 1 & 2 — cementing");
                    regs.Add("EPA 40 CFR 98 Subpart W — GHG");
                    break;
                case "CANADA":
                    regs.Add("AER Directive 020 — Well Abandonment");
                    regs.Add("ECCC NIR — GHG reporting");
                    break;
                default:
                    regs.Add("OSPAR 2022 guidance — offshore abandonment");
                    regs.Add("IOGP 454 — well integrity during abandonment");
                    break;
            }
            return regs;
        }

        private List<string> DetermineComplianceRequirements(string jurisdiction, string wellClass, List<string> regs)
        {
            var reqs = new List<string>
            {
                "Submit abandonment notification to regulator",
                "Conduct final wellbore survey",
                "File P&A completion report within 30 days"
            };
            if (jurisdiction.ToUpperInvariant() == "USA" && wellClass == "OFFSHORE")
                reqs.Add("BSEE inspector witness required for abandonment operations");
            return reqs;
        }

        private DateTime CalculateComplianceDeadline(DateTime abandonmentDate, string jurisdiction)
            => jurisdiction.ToUpperInvariant() == "CANADA"
                ? abandonmentDate.AddDays(30)
                : abandonmentDate.AddDays(60);

        private List<string> IdentifyRequiredDocumentation(string jurisdiction, string wellClass)
            => new List<string>
            {
                "P&A Program",
                "Well Status Change Form",
                "Final Cement Evaluation Report",
                "Environmental Site Assessment"
            };

        private string AssessBondingRequirements(string jurisdiction, string wellClass, DateTime abandonmentDate)
            => jurisdiction.ToUpperInvariant() == "USA"
                ? "BSEE performance bond — offshore wells"
                : "Provincial abandonment security deposit";

        private List<string> IdentifyInspectionRequirements(string jurisdiction, string wellClass)
            => new List<string> { "Pre-abandonment wellbore inspection", "Post-abandonment site inspection" };

        private double EstimateComplianceCost(int requirementCount, string jurisdiction)
            => requirementCount * 5_000;

        private int RecommendDecommissioningPhases(int wellCount)
            => wellCount <= 5 ? 1 : wellCount <= 20 ? 2 : 3;
    }
}
