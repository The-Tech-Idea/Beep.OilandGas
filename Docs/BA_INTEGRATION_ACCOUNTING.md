# BUSINESS_ASSOCIATE (BA) Integration for Accounting Workflows

> All accounting workflows use PPDM 3.9 BA tables for party master data. No separate vendor/customer/partner tables needed.

## BA Table Catalog

| Table | Accounting Use |
|-------|---------------|
| `BUSINESS_ASSOCIATE` | Master record: legal name, tax ID (EIN/SSN), BA_CATEGORY, BA_TYPE, credit rating, active status |
| `BA_ADDRESS` | Physical, mailing, remittance, shipping addresses per BA |
| `BA_CONTACT_INFO` | Phone, email, primary contact per BA |
| `BA_PREFERENCE` | Payment terms (Net 30), credit limit ($), currency (USD), tax withholding, ACH/banking info |
| `BA_ORGANIZATION` | Parent/subsidiary relationships, org hierarchy for consolidated billing |
| `BA_XREF` | Cross-references: DBA names, legacy IDs, links to DIVISION_ORDER, JOA, SALES_AGREEMENT |
| `BA_ALIAS` | Alternate names / DBAs |

## BA Categories for Accounting

| BA_CATEGORY | BA_TYPE | Used By |
|-------------|---------|---------|
| `Vendor` | `Supplier` | Procure-to-Pay |
| `Vendor` | `Service Provider` | P2P, Expense Management |
| `Customer` | `Purchaser` | Order-to-Cash, ASC 606 Revenue |
| `Customer` | `Operator` | JIB billing |
| `Working Interest Owner` | `Partner` | JIB, Non-Consent, Imbalance |
| `Working Interest Owner` | `Non-Consenting Party` | Non-Consent Penalty |
| `Royalty Owner` | `Mineral Owner` | Royalty Calculation |
| `Royalty Owner` | `ORRI Holder` | Royalty |
| `Royalty Owner` | `NPI Holder` | Royalty |
| `Government` | `Tax Authority` | Tax Filing |
| `Government` | `Regulator` | Regulatory Filing |
| `Government` | `Host Government` | PSC Accounting |
| `Employee` | `Staff` | Expense Management |
| `Financial Institution` | `Bank` | Bank Rec, Cash Management |
| `Financial Institution` | `Lender` | RBL Redetermination |

## Workflow → BA Mapping

| Workflow | Party Role | BA_CATEGORY | Child Tables Used |
|----------|-----------|-------------|-------------------|
| ACCT_PROCURE_TO_PAY | Vendor | Vendor | BA_ADDRESS (remit-to), BA_PREFERENCE (payment terms, ACH) |
| ACCT_ORDER_TO_CASH | Customer | Customer | BA_ADDRESS (bill-to, ship-to), BA_PREFERENCE (credit limit, payment terms) |
| ACCT_VENDOR_MANAGEMENT | Vendor | Vendor | All child tables |
| ACCT_CUSTOMER_MANAGEMENT | Customer | Customer | All child tables |
| ACCT_EXPENSE_MANAGEMENT | Employee | Employee | BA_PREFERENCE (reimbursement method) |
| ACCT_BANK_RECONCILIATION | Bank | Financial Institution | BA_ADDRESS, BA_CONTACT_INFO |
| CRW_ASC606_REVENUE | Customer | Customer | BA_XREF→SALES_AGREEMENT |
| CRW_JIB_COPAS_OVERHEAD | WI Owner | Working Interest Owner | BA_XREF→JOA, BA_ADDRESS (bill-to) |
| CRW_NONCONSENT | Non-Consenting Party | Working Interest Owner | BA_XREF→JOA |
| CRW_ROYALTY_CALCULATION | Royalty Owner | Royalty Owner | BA_XREF→DIVISION_ORDER, BA_ADDRESS (payment) |
| CRW_PRODUCTION_IMBALANCE | WI Owner | Working Interest Owner | BA_XREF→DIVISION_ORDER |
| CRW_COPAS_AUDIT | Operator | Customer | BA_XREF→JOA |
| CRW_AFE_COST_TRACKING | WI Owner | Working Interest Owner | BA_XREF→JOA |
| CRW_PSC_ACCOUNTING | Host Government | Government | BA_PREFERENCE (PSC terms) |
| CRW_PRODUCTION_TAX_FILING | Tax Authority | Government | BA_ADDRESS |
| CRW_RBL_REDETERMINATION | Lender | Financial Institution | BA_CONTACT_INFO |
| CRW_TAKE_OR_PAY | Customer | Customer | BA_XREF→CONTRACT |
| CRW_DECOM_ESTIMATE_REVISION | Regulator | Government | BA_CONTACT_INFO |

## Reference Values (LOV) — Database-Driven, User-Modifiable

All BA_CATEGORY, BA_TYPE, and other reference values are stored in PPDM R_* tables. These are **NOT hardcoded constants** — users can modify them via the setup wizard or admin UI.

| Table | Purpose | User Can |
|-------|---------|----------|
| `R_BA_CATEGORY` | BA classifications | Add new categories (e.g., "TRANSPORTER"), deactivate unused ones |
| `R_BA_TYPE` | Sub-types per category | Add new types, change descriptions |
| `R_BA_PREF_TYPE` | Preference/configuration keys | Add new preference types |
| `R_BA_STATUS` | Lifecycle states | Add new statuses, mark obsolete |

**Initial seed values** are provided by `AccountingModuleSetup` (Order 51) using `AccountingReferenceCodes` as defaults. After setup, the database is the source of truth.

## How BA Sync Works

1. **Create BA** via `ACCT_VENDOR_MANAGEMENT` or `ACCT_CUSTOMER_MANAGEMENT`
2. **Link BA to contracts** via `BA_XREF` (e.g., BA→JOA, BA→SALES_AGREEMENT, BA→DIVISION_ORDER)
3. **Resolve BA at workflow start** — lookup BA by contract reference
4. **Use BA child tables** for:
   - Billing address → `BA_ADDRESS` with ADDRESS_TYPE='Billing'
   - Payment terms → `BA_PREFERENCE` with PREFERENCE_TYPE='PaymentTerms'
   - Contact → `BA_CONTACT_INFO`
5. **Audit trail** — all BA changes logged to PROCESS_HISTORY

## No Duplicate Tables

The system does NOT maintain separate Vendor, Customer, or Partner tables. All parties are stored in BUSINESS_ASSOCIATE and differentiated by BA_CATEGORY + BA_TYPE. This follows PPDM 3.9 best practice.
