using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Decommissioning.Services
{
    /// <summary>
    /// Advanced Decommissioning and Well Plugging Analysis Service.
    /// Provides specialized analysis for well abandonment, decommissioning planning,
    /// environmental remediation, and compliance tracking.
    /// </summary>
    public partial class WellPluggingService
    {
        private readonly ILogger<WellPluggingService>? _logger;

        /// <summary>
        /// Analyzes well plugging requirements and generates comprehensive plugging plan.
        /// </summary>
        public async Task<WellPluggingPlanDto> AnalyzePluggingRequirementsAsync(
            string wellUWI,
            double wellDepth,
            List<string> zonesCased,
            double freshwaterAquiferDepth,
            double reservoirPressure)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (wellDepth <= 0)
                throw new ArgumentException("Well depth must be positive", nameof(wellDepth));

            _logger?.LogInformation("Analyzing plugging requirements: well={Well}, depth={Depth:F0} ft, " +
                "zones={Zones}, FW aquifer={FWAq:F0} ft", wellUWI, wellDepth, zonesCased.Count, freshwaterAquiferDepth);

            try
            {
                var result = new WellPluggingPlanDto
                {
                    WellUWI = wellUWI,
                    WellDepth = wellDepth,
                    ZonesIdentified = zonesCased.Count,
                    FreshwaterAquiferDepth = freshwaterAquiferDepth,
                    AnalysisDate = DateTime.UtcNow
                };

                // Identify critical zones requiring plugging
                result.CriticalZones = IdentifyCriticalZones(zonesCased, freshwaterAquiferDepth, wellDepth);

                // Determine plug requirements by zone
                result.PluggingStrategy = DeterminePluggingStrategy(
                    wellDepth, freshwaterAquiferDepth, reservoirPressure, zonesCased.Count);

                // Calculate cement requirements
                result.CementRequirements = CalculateCementRequirements(
                    result.PluggingStrategy, wellDepth);

                // Generate plug specifications
                result.PlugSpecifications = GeneratePlugSpecifications(result.PluggingStrategy);

                // Estimate plugging time
                result.EstimatedDaysRequired = EstimatePluggingDuration(wellDepth);

                // Identify potential issues
                result.PotentialIssues = IdentifyPluggingIssues(
                    reservoirPressure, wellDepth, zonesCased.Count);

                _logger?.LogInformation("Plugging analysis complete: {Zones} critical zones identified, " +
                    "estimated duration {Days} days", result.CriticalZones.Count, result.EstimatedDaysRequired);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing plugging requirements for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Calculates decommissioning and abandonment costs.
        /// </summary>
        public async Task<DecommissioningCostAnalysisDto> AnalyzeDecommissioningCostsAsync(
            string wellUWI,
            double wellDepth,
            string wellType,
            string location,
            bool requiresEnvironmentalRemediation = true)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Analyzing decommissioning costs: well={Well}, depth={Depth:F0} ft, " +
                "type={Type}, location={Location}", wellUWI, wellDepth, wellType, location);

            try
            {
                var result = new DecommissioningCostAnalysisDto
                {
                    WellUWI = wellUWI,
                    WellDepth = wellDepth,
                    WellType = wellType,
                    Location = location,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate well plugging cost
                result.WellPluggingCost = CalculateWellPluggingCost(wellDepth, wellType);

                // Calculate wellhead removal cost
                result.WellheadRemovalCost = CalculateWellheadRemovalCost(wellType);

                // Calculate site restoration cost
                result.SiteRestorationCost = CalculateSiteRestorationCost(location, wellDepth);

                // Calculate environmental remediation cost
                if (requiresEnvironmentalRemediation)
                    result.EnvironmentalRemediationCost = CalculateEnvironmentalRemediationCost(location, wellDepth);

                // Calculate abandonment cost (regulatory)
                result.AbandonmentBondCost = CalculateAbandonmentBond(wellDepth, location);

                // Total costs
                result.TotalEstimatedCost = result.WellPluggingCost + result.WellheadRemovalCost +
                    result.SiteRestorationCost + result.EnvironmentalRemediationCost + result.AbandonmentBondCost;

                // Cost breakdown percentages
                result.PluggingCostPercentage = (result.WellPluggingCost / Math.Max(1, result.TotalEstimatedCost)) * 100;
                result.WellheadRemovalPercentage = (result.WellheadRemovalCost / Math.Max(1, result.TotalEstimatedCost)) * 100;
                result.SiteRestorationPercentage = (result.SiteRestorationCost / Math.Max(1, result.TotalEstimatedCost)) * 100;

                // Cost contingency (20% typical)
                result.ContingencyAmount = result.TotalEstimatedCost * 0.20;
                result.TotalWithContingency = result.TotalEstimatedCost + result.ContingencyAmount;

                _logger?.LogInformation("Cost analysis complete: plugging=${Plug:F0}, restoration=${Rest:F0}, " +
                    "total=${Total:F0}", result.WellPluggingCost, result.SiteRestorationCost, result.TotalWithContingency);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing decommissioning costs for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Analyzes environmental remediation requirements for abandoned wells.
        /// </summary>
        public async Task<EnvironmentalRemediationAnalysisDto> AnalyzeEnvironmentalRemediationAsync(
            string wellUWI,
            string location,
            List<string> potentialContaminants,
            double distanceToWaterSource)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Analyzing environmental remediation: well={Well}, location={Location}, " +
                "distance to water={Distance:F0} ft, contaminants={Count}",
                wellUWI, location, distanceToWaterSource, potentialContaminants?.Count ?? 0);

            try
            {
                var result = new EnvironmentalRemediationAnalysisDto
                {
                    WellUWI = wellUWI,
                    Location = location,
                    AnalysisDate = DateTime.UtcNow,
                    PotentialContaminants = potentialContaminants ?? new List<string>()
                };

                // Assess environmental risk
                result.EnvironmentalRiskLevel = AssessEnvironmentalRisk(
                    location, distanceToWaterSource, potentialContaminants);

                // Determine remediation activities
                result.RemediationActivities = DetermineRemediationActivities(
                    result.EnvironmentalRiskLevel, potentialContaminants, location);

                // Estimate remediation timeline
                result.EstimatedRemediationMonths = EstimateRemediationTimeline(
                    result.RemediationActivities.Count, result.EnvironmentalRiskLevel);

                // Calculate monitoring requirements
                result.MonitoringPeriodYears = CalculateMonitoringPeriod(result.EnvironmentalRiskLevel);

                // Estimate long-term liability
                result.LongTermLiabilityCost = EstimateLongTermLiability(
                    result.MonitoringPeriodYears, result.EnvironmentalRiskLevel);

                // Identify regulatory requirements
                result.RegulatoryRequirements = IdentifyRegulatoryRequirements(
                    location, result.EnvironmentalRiskLevel, potentialContaminants);

                _logger?.LogInformation("Environmental analysis complete: risk={Risk}, " +
                    "remediation months={Months}, monitoring years={Monitoring}",
                    result.EnvironmentalRiskLevel, result.EstimatedRemediationMonths, result.MonitoringPeriodYears);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing environmental remediation for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Analyzes regulatory compliance requirements for well abandonment.
        /// </summary>
        public async Task<RegulatoryComplianceAnalysisDto> AnalyzeRegulatoryComplianceAsync(
            string wellUWI,
            string jurisdiction,
            string wellClass,
            DateTime abandonmentDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (string.IsNullOrWhiteSpace(jurisdiction))
                throw new ArgumentException("Jurisdiction cannot be null or empty", nameof(jurisdiction));

            _logger?.LogInformation("Analyzing regulatory compliance: well={Well}, jurisdiction={Jurisdiction}, " +
                "class={Class}, abandonment date={Date:yyyy-MM-dd}", wellUWI, jurisdiction, wellClass, abandonmentDate);

            try
            {
                var result = new RegulatoryComplianceAnalysisDto
                {
                    WellUWI = wellUWI,
                    Jurisdiction = jurisdiction,
                    WellClass = wellClass,
                    AbandonmentDate = abandonmentDate,
                    AnalysisDate = DateTime.UtcNow
                };

                // Identify applicable regulations
                result.ApplicableRegulations = IdentifyApplicableRegulations(jurisdiction, wellClass);

                // Determine compliance requirements
                result.ComplianceRequirements = DetermineComplianceRequirements(
                    jurisdiction, wellClass, result.ApplicableRegulations);

                // Calculate compliance timeline
                result.ComplianceDeadlineDate = CalculateComplianceDeadline(
                    abandonmentDate, jurisdiction);

                // Identify required documentation
                result.RequiredDocumentation = IdentifyRequiredDocumentation(jurisdiction, wellClass);

                // Assess bonding requirements
                result.BondingRequirements = AssessBondingRequirements(
                    jurisdiction, wellClass, abandonmentDate);

                // Identify inspection requirements
                result.InspectionRequirements = IdentifyInspectionRequirements(jurisdiction, wellClass);

                // Estimate compliance cost
                result.ComplianceCostEstimate = EstimateComplianceCost(
                    result.ComplianceRequirements.Count, jurisdiction);

                _logger?.LogInformation("Compliance analysis complete: jurisdiction={Jurisdiction}, " +
                    "requirements={Count}, deadline={Deadline:yyyy-MM-dd}",
                    jurisdiction, result.ComplianceRequirements.Count, result.ComplianceDeadlineDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing regulatory compliance for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Analyzes salvage value and asset recovery from decommissioned wells.
        /// </summary>
        public async Task<SalvageValueAnalysisDto> AnalyzeSalvageValueAsync(
            string wellUWI,
            string wellType,
            double wellDepth,
            bool includesDownholeEquipment = true)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Analyzing salvage value: well={Well}, type={Type}, depth={Depth:F0} ft",
                wellUWI, wellType, wellDepth);

            try
            {
                var result = new SalvageValueAnalysisDto
                {
                    WellUWI = wellUWI,
                    WellType = wellType,
                    WellDepth = wellDepth,
                    AnalysisDate = DateTime.UtcNow
                };

                // Estimate equipment salvage value
                result.EquipmentSalvageValue = EstimateEquipmentSalvageValue(wellType, wellDepth);

                // Estimate tubing/casing scrap value
                result.MetalScrapValue = EstimateMetalScrapValue(wellDepth, wellType);

                // Estimate wellhead/BOP value
                result.WellheadEquipmentValue = EstimateWellheadEquipmentValue(wellType);

                // Total salvage value
                result.TotalSalvageValue = result.EquipmentSalvageValue +
                    result.MetalScrapValue + result.WellheadEquipmentValue;

                // Salvage value as offset to decommissioning cost
                result.SalvageValuePercentageOfDecomCost = 15.0; // Typical 10-20%

                // Items to salvage
                result.SalvageableItems = IdentifySalvageableItems(wellType, includesDownholeEquipment);

                // Transportation and logistics cost
                result.TransportationCost = EstimateTransportationCost(wellDepth, result.SalvageableItems.Count);

                // Net salvage value (after costs)
                result.NetSalvageValue = result.TotalSalvageValue - result.TransportationCost;

                _logger?.LogInformation("Salvage analysis complete: equipment=${Equip:F0}, " +
                    "metal scrap=${Metal:F0}, net salvage=${Net:F0}",
                    result.EquipmentSalvageValue, result.MetalScrapValue, result.NetSalvageValue);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing salvage value for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Generates comprehensive decommissioning project schedule.
        /// </summary>
        public async Task<DecommissioningScheduleDto> GenerateDecommissioningScheduleAsync(
            string wellUWI,
            double wellDepth,
            int priorityLevel,
            DateTime plannedStartDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Generating decommissioning schedule: well={Well}, depth={Depth:F0} ft, " +
                "priority={Priority}, start={Start:yyyy-MM-dd}", wellUWI, wellDepth, priorityLevel, plannedStartDate);

            try
            {
                var result = new DecommissioningScheduleDto
                {
                    WellUWI = wellUWI,
                    WellDepth = wellDepth,
                    PriorityLevel = priorityLevel,
                    PlannedStartDate = plannedStartDate,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate project phases
                result.ProjectPhases = GenerateProjectPhases(wellDepth, plannedStartDate);

                // Estimate total duration
                result.EstimatedTotalDays = CalculateTotalProjectDuration(wellDepth);

                // Calculate completion date
                result.EstimatedCompletionDate = plannedStartDate.AddDays(result.EstimatedTotalDays);

                // Identify critical path items
                result.CriticalPathItems = IdentifyCriticalPathItems(result.ProjectPhases);

                // Risk assessment for schedule
                result.ScheduleRiskLevel = AssessScheduleRisk(wellDepth, result.EstimatedTotalDays);

                // Estimate schedule buffer (contingency days)
                result.ContingencyDays = CalculateScheduleContingency(result.EstimatedTotalDays);

                result.FinalEstimatedDate = result.EstimatedCompletionDate.AddDays(result.ContingencyDays);

                // Resource requirements
                result.EstimatedCrewSize = EstimateCrewSize(wellDepth);
                result.EstimatedEquipmentNeeds = EstimateEquipmentNeeds(wellDepth);

                _logger?.LogInformation("Schedule generated: total days={Days}, completion={Completion:yyyy-MM-dd}, " +
                    "crew size={Crew}, phases={Phases}",
                    result.EstimatedTotalDays, result.FinalEstimatedDate,
                    result.EstimatedCrewSize, result.ProjectPhases.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating decommissioning schedule for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Performs comprehensive well abandonment feasibility assessment.
        /// </summary>
        public async Task<AbandonmentFeasibilityDto> AssessAbandonmentFeasibilityAsync(
            string wellUWI,
            double wellDepth,
            string wellStatus,
            DateTime lastProductionDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Assessing abandonment feasibility: well={Well}, depth={Depth:F0} ft, " +
                "status={Status}, last production={Date:yyyy-MM-dd}",
                wellUWI, wellDepth, wellStatus, lastProductionDate);

            try
            {
                var result = new AbandonmentFeasibilityDto
                {
                    WellUWI = wellUWI,
                    WellDepth = wellDepth,
                    WellStatus = wellStatus,
                    LastProductionDate = lastProductionDate,
                    AnalysisDate = DateTime.UtcNow
                };

                // Assess well condition
                result.WellConditionStatus = AssessWellCondition(wellStatus, wellDepth);

                // Determine abandonment feasibility
                result.AbandonmentFeasible = DetermineAbandonmentFeasibility(
                    result.WellConditionStatus, wellStatus);

                // Identify challenges
                result.AbandonmentChallenges = IdentifyAbandonmentChallenges(
                    wellStatus, wellDepth, result.WellConditionStatus);

                // Recommend abandonment approach
                result.RecommendedApproach = RecommendAbandonmentApproach(
                    wellStatus, result.WellConditionStatus);

                // Timeline feasibility
                result.CanAbandonWithin12Months = result.AbandonmentFeasible &&
                    result.AbandonmentChallenges.Count < 3;

                // Cost-benefit analysis
                result.AbandonmentBenefit = CalculateAbandonmentBenefit(wellDepth);
                result.AbandonmentCost = CalculateAbandonmentCostEstimate(wellDepth);
                result.NetBenefit = result.AbandonmentBenefit - result.AbandonmentCost;

                // Risk assessment
                result.AbandonmentRiskLevel = AssessAbandonmentRisk(
                    result.WellConditionStatus, result.AbandonmentChallenges.Count);

                _logger?.LogInformation("Feasibility assessment complete: feasible={Feasible}, " +
                    "condition={Condition}, challenges={Challenges}, risk={Risk}",
                    result.AbandonmentFeasible, result.WellConditionStatus,
                    result.AbandonmentChallenges.Count, result.AbandonmentRiskLevel);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assessing abandonment feasibility for well {Well}", wellUWI);
                throw;
            }
        }

        /// <summary>
        /// Analyzes decommissioning requirements across a portfolio of wells.
        /// </summary>
        public async Task<PortfolioDecommissioningAnalysisDto> AnalyzeWellPortfolioDecommissioningAsync(
            string fieldId,
            List<string> wellUWIs,
            Dictionary<string, double> wellDepths)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));
            if (wellUWIs == null || wellUWIs.Count == 0)
                throw new ArgumentException("Well UWIs cannot be null or empty", nameof(wellUWIs));

            _logger?.LogInformation("Analyzing portfolio decommissioning: field={Field}, wells={Count}",
                fieldId, wellUWIs.Count);

            try
            {
                var result = new PortfolioDecommissioningAnalysisDto
                {
                    FieldId = fieldId,
                    WellsToDecommission = wellUWIs.Count,
                    AnalysisDate = DateTime.UtcNow
                };

                // Analyze each well
                result.WellAnalyses = new List<PortfolioWellDecommissioningDto>();
                double totalCost = 0;
                int totalDays = 0;

                foreach (var uwi in wellUWIs)
                {
                    if (wellDepths.ContainsKey(uwi))
                    {
                        var depth = wellDepths[uwi];
                        var cost = CalculateWellPluggingCost(depth, "Oil");
                        var days = EstimatePluggingDuration(depth);

                        result.WellAnalyses.Add(new PortfolioWellDecommissioningDto
                        {
                            WellUWI = uwi,
                            WellDepth = depth,
                            EstimatedCost = cost,
                            EstimatedDays = days
                        });

                        totalCost += cost;
                        totalDays += days;
                    }
                }

                // Portfolio totals
                result.TotalEstimatedCost = totalCost;
                result.TotalEstimatedDays = totalDays;
                result.AverageCostPerWell = totalCost / Math.Max(1, wellUWIs.Count);
                result.AverageDaysPerWell = totalDays / Math.Max(1, wellUWIs.Count);

                // Phased approach recommendation
                result.PhaseCount = RecommendDecommissioningPhases(wellUWIs.Count);
                result.WellsPerPhase = (int)Math.Ceiling(wellUWIs.Count / (double)result.PhaseCount);

                // Total with contingency
                result.ContingencyPercentage = 20.0;
                result.TotalWithContingency = result.TotalEstimatedCost * (1 + result.ContingencyPercentage / 100);

                _logger?.LogInformation("Portfolio analysis complete: wells={Count}, total cost=${Cost:F0}, " +
                    "total days={Days}, phases={Phases}",
                    wellUWIs.Count, result.TotalWithContingency, result.TotalEstimatedDays, result.PhaseCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing portfolio decommissioning for field {Field}", fieldId);
                throw;
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        private List<string> IdentifyCriticalZones(List<string> zones, double fwAquifer, double wellDepth)
        {
            var critical = new List<string>();
            critical.Add("Surface casing shoe"); // Always critical
            if (fwAquifer > 0 && fwAquifer < wellDepth)
                critical.Add("Freshwater aquifer zone");
            critical.Add("Intermediate zone");
            if (zones.Count > 2)
                critical.Add("Production zone");
            return critical;
        }

        private string DeterminePluggingStrategy(double wellDepth, double fwAquifer, double pressure, int zoneCount)
        {
            if (wellDepth < 3000)
                return "Balanced Cement Plugs";
            else if (wellDepth < 10000)
                return "Staged Cement Plugs with Bridge Plugs";
            else
                return "Multi-stage Plugging with Expandable Packers";
        }

        private double CalculateCementRequirements(string strategy, double wellDepth)
        {
            // Rough estimate: 1 sack per 50-100 feet depending on strategy
            double sacks = wellDepth / 75;
            return sacks * 94; // Weight in pounds
        }

        private List<string> GeneratePlugSpecifications(string strategy)
        {
            var specs = new List<string>
            {
                "API Class G Cement",
                "Density: 15.8 ppg",
                "Slurry volume calculations per zone",
                "Waiting on Cement (WOC) times per API RP 65"
            };
            return specs;
        }

        private int EstimatePluggingDuration(double wellDepth)
        {
            // Typical: 1-2 days per 1000 feet
            return (int)Math.Ceiling(wellDepth / 5000.0 * 10);
        }

        private List<string> IdentifyPluggingIssues(double pressure, double wellDepth, int zones)
        {
            var issues = new List<string>();
            if (pressure > 5000)
                issues.Add("High pressure zone - may require specialized equipment");
            if (wellDepth > 15000)
                issues.Add("Ultra-deepwater plugging complexity");
            if (zones > 5)
                issues.Add("Multiple zones increase plugging time and cost");
            return issues;
        }

        private double CalculateWellPluggingCost(double wellDepth, string wellType)
        {
            // Base cost + depth multiplier
            double baseCost = wellType == "Oil" ? 50000 : 35000;
            double depthCost = wellDepth / 1000 * 15000; // $15k per 1000 ft
            return baseCost + depthCost;
        }

        private double CalculateWellheadRemovalCost(string wellType)
        {
            return wellType == "Oil" ? 25000 : 15000;
        }

        private double CalculateSiteRestorationCost(string location, double wellDepth)
        {
            // Location-dependent: onshore/offshore
            double baseCost = location.Contains("offshore") ? 75000 : 25000;
            double depthFactor = wellDepth > 10000 ? 1.5 : 1.0;
            return baseCost * depthFactor;
        }

        private double CalculateEnvironmentalRemediationCost(string location, double wellDepth)
        {
            // Risk-based: deeper wells = higher remediation
            return (wellDepth / 10000) * 50000 + 10000;
        }

        private double CalculateAbandonmentBond(double wellDepth, string location)
        {
            // Regulatory bond requirement (varies by jurisdiction)
            return wellDepth / 1000 * 5000 + 10000;
        }

        private string AssessEnvironmentalRisk(string location, double distanceToWater, List<string> contaminants)
        {
            if (distanceToWater < 500 && contaminants.Count > 2)
                return "High";
            else if (distanceToWater < 2000 || contaminants.Count > 0)
                return "Moderate";
            else
                return "Low";
        }

        private List<string> DetermineRemediationActivities(string riskLevel, List<string> contaminants, string location)
        {
            var activities = new List<string> { "Site survey", "Sampling analysis" };
            if (riskLevel == "High")
            {
                activities.Add("Soil remediation");
                activities.Add("Groundwater treatment");
            }
            else if (riskLevel == "Moderate")
                activities.Add("Enhanced monitoring");
            return activities;
        }

        private int EstimateRemediationTimeline(int activityCount, string riskLevel)
        {
            int baseMonths = riskLevel == "High" ? 12 : 6;
            return baseMonths + (activityCount * 2);
        }

        private int CalculateMonitoringPeriod(string riskLevel)
        {
            return riskLevel == "High" ? 5 : (riskLevel == "Moderate" ? 3 : 1);
        }

        private double EstimateLongTermLiability(int monitoringYears, string riskLevel)
        {
            double yearlyMonitoringCost = riskLevel == "High" ? 50000 : 20000;
            return yearlyMonitoringCost * monitoringYears;
        }

        private List<string> IdentifyRegulatoryRequirements(string location, string riskLevel, List<string> contaminants)
        {
            var reqs = new List<string> { "Well abandonment permit", "Environmental assessment" };
            if (riskLevel == "High")
                reqs.Add("State environmental review");
            return reqs;
        }

        private List<string> IdentifyApplicableRegulations(string jurisdiction, string wellClass)
        {
            var regs = new List<string> { "State well abandonment regulations", "API RP 65" };
            if (jurisdiction.Contains("federal") || jurisdiction.Contains("federal water"))
                regs.Add("Federal compliance requirements");
            return regs;
        }

        private List<string> DetermineComplianceRequirements(string jurisdiction, string wellClass, List<string> regulations)
        {
            return new List<string> { "Well plugging plan approval", "Rig schedule notification", "Abandonment report" };
        }

        private DateTime CalculateComplianceDeadline(DateTime abandonmentDate, string jurisdiction)
        {
            // Typical: 1-2 years from abandonment
            return abandonmentDate.AddYears(1);
        }

        private List<string> IdentifyRequiredDocumentation(string jurisdiction, string wellClass)
        {
            return new List<string> { "As-built well schematic", "Plugging procedure", "Pressure test results" };
        }

        private string AssessBondingRequirements(string jurisdiction, string wellClass, DateTime abandonmentDate)
        {
            return "Bond required - see cost analysis";
        }

        private List<string> IdentifyInspectionRequirements(string jurisdiction, string wellClass)
        {
            return new List<string> { "Pre-abandonment inspection", "Final abandonment inspection" };
        }

        private double EstimateComplianceCost(int requirementCount, string jurisdiction)
        {
            return 5000 + (requirementCount * 2000); // $5k base + $2k per requirement
        }

        private double EstimateEquipmentSalvageValue(string wellType, double wellDepth)
        {
            double baseValue = wellType == "Oil" ? 15000 : 10000;
            return baseValue + (wellDepth / 2000 * 5000);
        }

        private double EstimateMetalScrapValue(double wellDepth, string wellType)
        {
            // Tubing/casing scrap: ~$1-2 per pound
            double poundageEstimate = wellDepth / 2; // Rough estimate
            return poundageEstimate * 1.50;
        }

        private double EstimateWellheadEquipmentValue(string wellType)
        {
            return wellType == "Oil" ? 8000 : 5000;
        }

        private List<string> IdentifySalvageableItems(string wellType, bool includesDownhole)
        {
            var items = new List<string> { "Tubing", "Casing", "Wellhead equipment", "BOP components" };
            if (includesDownhole)
                items.Add("Downhole tools");
            return items;
        }

        private double EstimateTransportationCost(double wellDepth, int itemCount)
        {
            return 5000 + (itemCount * 500); // Base + per-item cost
        }

        private List<DecommissioningPhaseDto> GenerateProjectPhases(double wellDepth, DateTime startDate)
        {
            var phases = new List<DecommissioningPhaseDto>
            {
                new DecommissioningPhaseDto { Phase = "Mobilization", Duration = 3 },
                new DecommissioningPhaseDto { Phase = "Well Plugging", Duration = (int)Math.Ceiling(wellDepth / 5000) },
                new DecommissioningPhaseDto { Phase = "Wellhead Removal", Duration = 2 },
                new DecommissioningPhaseDto { Phase = "Site Restoration", Duration = 5 }
            };
            return phases;
        }

        private int CalculateTotalProjectDuration(double wellDepth)
        {
            return 3 + (int)Math.Ceiling(wellDepth / 5000) + 2 + 5;
        }

        private List<string> IdentifyCriticalPathItems(List<DecommissioningPhaseDto> phases)
        {
            return phases.OrderByDescending(p => p.Duration).Take(2).Select(p => p.Phase).ToList();
        }

        private string AssessScheduleRisk(double wellDepth, int estimatedDays)
        {
            if (wellDepth > 15000 || estimatedDays > 45)
                return "High";
            if (wellDepth > 10000 || estimatedDays > 30)
                return "Moderate";
            return "Low";
        }

        private int CalculateScheduleContingency(int estimatedDays)
        {
            return (int)Math.Ceiling(estimatedDays * 0.15); // 15% contingency
        }

        private int EstimateCrewSize(double wellDepth)
        {
            return wellDepth > 10000 ? 15 : 8;
        }

        private List<string> EstimateEquipmentNeeds(double wellDepth)
        {
            var equipment = new List<string> { "Drilling rig or workover rig", "Pump truck" };
            if (wellDepth > 10000)
                equipment.Add("High-pressure equipment");
            return equipment;
        }

        private string AssessWellCondition(string wellStatus, double wellDepth)
        {
            if (wellStatus.Contains("plugged") || wellStatus.Contains("abandoned"))
                return "Already abandoned";
            if (wellStatus.Contains("idle") || wellStatus.Contains("inactive"))
                return "Suitable for abandonment";
            return "Active - requires special consideration";
        }

        private bool DetermineAbandonmentFeasibility(string condition, string status)
        {
            return condition != "Already abandoned" && !status.Contains("producing");
        }

        private List<string> IdentifyAbandonmentChallenges(string status, double wellDepth, string condition)
        {
            var challenges = new List<string>();
            if (wellDepth > 15000)
                challenges.Add("Deep well complexity");
            if (status.Contains("stuck") || status.Contains("problem"))
                challenges.Add("Well condition issues");
            return challenges;
        }

        private string RecommendAbandonmentApproach(string status, string condition)
        {
            return condition == "Suitable for abandonment" ? "Standard abandonment" : "Specialized abandonment";
        }

        private double CalculateAbandonmentBenefit(double wellDepth)
        {
            // Benefit: liability elimination, maintenance cost savings
            double maintCostSavings = wellDepth / 1000 * 5000; // $5k per 1000 ft annual
            return maintCostSavings * 5; // 5-year benefit
        }

        private double CalculateAbandonmentCostEstimate(double wellDepth)
        {
            return CalculateWellPluggingCost(wellDepth, "Oil") + 50000; // Plugging + misc
        }

        private string AssessAbandonmentRisk(string condition, int challengeCount)
        {
            if (challengeCount > 3 || condition == "Already abandoned")
                return "High";
            if (challengeCount > 1)
                return "Moderate";
            return "Low";
        }

        private int RecommendDecommissioningPhases(int wellCount)
        {
            if (wellCount > 20)
                return 4;
            if (wellCount > 10)
                return 3;
            if (wellCount > 5)
                return 2;
            return 1;
        }
    }

    // ===== SUPPORTING RESULT CLASSES (in Models project) =====
    // These will be moved to DecommissioningDTOs.cs in the Models project
}
