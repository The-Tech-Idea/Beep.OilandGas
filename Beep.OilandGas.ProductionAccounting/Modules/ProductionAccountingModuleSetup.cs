using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

namespace Beep.OilandGas.ProductionAccounting.Modules
{
    /// <summary>
    /// Module order 70 — declares Production entity types for schema migration.
    /// This module is owned by ProductionAccounting because production-specific setup behavior
    /// belongs with that project even when no shared reference-data seed is currently required.
    /// </summary>
    public sealed class ProductionAccountingModuleSetup : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            // ── Allocation tables ──────────────────────────────────────────────────────
            typeof(ALLOCATION_DETAIL),
            typeof(ALLOCATION_REPORT_SUMMARY),
            typeof(ALLOCATION_RESULT),
            typeof(AVAILS_ALLOCATION),
            typeof(JOIB_ALLOCATION),
            typeof(LEASE_ALLOCATION_DATA),
            typeof(PRODUCTION_ALLOCATION),
            typeof(REVENUE_ALLOCATION),
            typeof(TRACT_ALLOCATION_DATA),
            typeof(WELL_ALLOCATION_DATA),
            // ── Amortization & depletion tables ───────────────────────────────────────
            typeof(ACCOUNTING_AMORTIZATION),
            typeof(AMORTIZATION_RECORD),
            typeof(AMORTIZATION_SPLIT),
            typeof(DEPLETION_CALCULATION),
            typeof(DEPLETION_ROLLFORWARD),
            // ── AFE & approval tables ──────────────────────────────────────────────────
            typeof(AFE),
            typeof(AFE_LINE_ITEM),
            typeof(APPROVAL_WORKFLOW),
            // ── Cost tables ────────────────────────────────────────────────────────────
            typeof(COPAS_OVERHEAD_AUDIT),
            typeof(COPAS_OVERHEAD_SCHEDULE),
            typeof(COST_REPORT_SUMMARY),
            typeof(COST_SHARING),
            typeof(COST_TRANSACTION),
            typeof(COST_VARIANCE_REPORT),
            // ── Crude oil & inventory tables ───────────────────────────────────────────
            typeof(CRUDE_OIL_INVENTORY),
            typeof(CRUDE_OIL_INVENTORY_TRANSACTION),
            typeof(CRUDE_OIL_PROPERTIES),
            typeof(CRUDE_OIL_SPECIFICATIONS),
            typeof(INVENTORY_REPORT_SUMMARY),
            typeof(RUN_TICKET),
            typeof(RUN_TICKET_VALUATION),
            typeof(TANK_INVENTORY),
            // ── Division & royalty tables ──────────────────────────────────────────────
            typeof(CEILING_TEST_CALCULATION),
            typeof(DELAY_RENTAL),
            typeof(DIVISION_ORDER),
            typeof(ROYALTY_CALCULATION),
            typeof(ROYALTY_DEDUCTIONS),
            typeof(ROYALTY_OWNER),
            typeof(ROYALTY_PAYMENT),
            typeof(ROYALTY_PAYMENT_DETAIL),
            typeof(REVENUE_DEDUCTION),
            typeof(REVENUE_DISTRIBUTION),
            typeof(REVENUE_SHARING),
            typeof(REVENUE_TRANSACTION),
            // ── Emissions tables ───────────────────────────────────────────────────────
            typeof(EMISSIONS_ALLOWANCE),
            typeof(EMISSIONS_OBLIGATION),
            typeof(EMISSIONS_SETTLEMENT),
            // ── Exchange & transfer tables ─────────────────────────────────────────────
            typeof(EXCHANGE_COMMITMENT),
            typeof(EXCHANGE_CONTRACT),
            typeof(TRANSFER_ORDER),
            typeof(ASSET_SWAP_TRANSACTION),
            // ── Financial tables ───────────────────────────────────────────────────────
            typeof(AP_CREDIT_MEMO),
            typeof(FINANCIAL_INSTRUMENT),
            typeof(FINANCIAL_REPORT),
            typeof(HEDGE_MEASUREMENT),
            typeof(HEDGE_RELATIONSHIP),
            // ── Governmental reporting tables ──────────────────────────────────────────
            typeof(GOVERNMENTAL_PRODUCTION_DATA),
            typeof(GOVERNMENTAL_REPORT),
            typeof(GOVERNMENTAL_ROYALTY_DATA),
            typeof(GOVERNMENTAL_TAX_DATA),
            // ── Imbalance tables ───────────────────────────────────────────────────────
            typeof(IMBALANCE_ADJUSTMENT),
            typeof(IMBALANCE_RECONCILIATION),
            typeof(IMBALANCE_STATEMENT),
            typeof(IMBALANCE_SUMMARY),
            typeof(OIL_IMBALANCE),
            // ── Joint interest & JOA tables ────────────────────────────────────────────
            typeof(JIB_CHARGE),
            typeof(JIB_COST_ALLOCATION),
            typeof(JIB_CREDIT),
            typeof(JIB_PARTICIPANT),
            typeof(JOA_INTEREST),
            typeof(JOIB_LINE_ITEM),
            typeof(JOINT_INTEREST_BILL),
            typeof(JOINT_INTEREST_STATEMENT),
            typeof(JOINT_OPERATING_AGREEMENT),
            // ── Lease tables ───────────────────────────────────────────────────────────
            typeof(LEASE_EXPIRY_EVENT),
            typeof(LEASE_OPTION),
            typeof(LEASE_REPORT),
            typeof(LEASEHOLD_CARRYING_GROUP),
            // ── Measurement tables ─────────────────────────────────────────────────────
            typeof(MEASUREMENT_ACCURACY),
            typeof(MEASUREMENT_CORRECTIONS),
            typeof(MEASUREMENT_RECORD),
            typeof(MEASUREMENT_REPORT_SUMMARY),
            typeof(LOCATION_ADJUSTMENTS),
            typeof(TIME_ADJUSTMENTS),
            typeof(QUALITY_ADJUSTMENTS),
            // ── Nomination & delivery tables ────────────────────────────────────────────
            typeof(ACTUAL_DELIVERY),
            typeof(NOMINATION),
            // ── Production reporting tables ─────────────────────────────────────────────
            typeof(CONTACT_INFORMATION),
            typeof(OPERATIONAL_REPORT),
            typeof(OWNER_INFORMATION),
            typeof(PARTICIPATING_AREA),
            typeof(PRODUCTION_AVAILS),
            typeof(PRODUCTION_REPORT_SUMMARY),
            typeof(PRODUCTION_SHARING_AGREEMENT),
            typeof(PRODUCTION_SHARING_ENTITLEMENT),
            // ── Reserve & disclosure tables ─────────────────────────────────────────────
            typeof(PROVED_RESERVES),
            typeof(RESERVE_CASHFLOW),
            typeof(RESERVE_DISCLOSURE_PACKAGE),
            // ── Revenue & pricing tables ────────────────────────────────────────────────
            typeof(REGULATED_PRICE),
            typeof(SALES_CONTRACT),
            // ── Take-or-pay tables ──────────────────────────────────────────────────────
            typeof(TAKE_OR_PAY_BALANCE),
            typeof(TAKE_OR_PAY_SCHEDULE),
            // ── Tax tables ──────────────────────────────────────────────────────────────
            typeof(TAX_ADJUSTMENT),
            typeof(TAX_WITHHOLDING),
            // ── Unit & tract tables ──────────────────────────────────────────────────────
            typeof(TRACT_PARTICIPATION),
            typeof(UNIT_AGREEMENT),
            typeof(UNIT_OPERATING_AGREEMENT),
            typeof(UNIT_PARTICIPANT),
            typeof(VOTING_RIGHTS),
            // ── Internal control ─────────────────────────────────────────────────────────
            typeof(INTERNAL_CONTROL_RULE),
        };

        public ProductionAccountingModuleSetup(ModuleSetupContext context) : base(context) { }

        public override string ModuleId => "PRODUCTION";
        public override string ModuleName => "Production Accounting";
        public override int Order => 70;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success = true;
            result.SkipReason = "No reference seed data defined for Production module.";
            return Task.FromResult(result);
        }
    }
}
