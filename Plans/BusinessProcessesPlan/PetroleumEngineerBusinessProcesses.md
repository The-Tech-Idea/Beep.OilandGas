# Petroleum Engineer Business Process Reference

> **Version**: 1.0 | **Date**: 2026-04-18  
> **Purpose**: Defines all petroleum engineer business processes to be simulated in the Beep Oil & Gas platform UI. This is the authoritative reference for what each page/workflow must accomplish — not what data it manages.  
> **Audience**: UI Developers, UX Designers, Domain Engineers, Copilot Agents

---

## Part 1: Asset Lifecycle Overview

An oil and gas field progresses through four major lifecycle stages. Each stage has distinct engineer roles, decisions, and workflows.

```mermaid
flowchart LR
    A([Exploration]) -->|Discovery confirmed| B([Development])
    B -->|First Oil achieved| C([Production])
    C -->|Economic limit reached| D([Decommissioning])
    
    style A fill:#2563eb,color:#fff
    style B fill:#16a34a,color:#fff
    style C fill:#ca8a04,color:#fff
    style D fill:#6b7280,color:#fff
```

### Stage Summary

| Stage | Duration | Key Decision | Main Engineer Role | Gate |
|-------|----------|-------------|-------------------|------|
| Exploration | 2–10 years | Drill / No-drill | Geoscientist, Exploration Engineer | Discovery Appraisal |
| Development | 3–7 years | FDP Approval | Development Engineer, Project Engineer | Final Investment Decision (FID) |
| Production | 10–40 years | Optimize vs Intervene | Production Engineer, Reservoir Engineer | Economic Limit |
| Decommissioning | 2–5 years | P&A vs Temporary Abandon | Integrity Engineer, Environmental Engineer | Site Restored |

---

## Part 2: Exploration Phase Business Processes

### 2.1 Prospect Maturation Workflow

The core exploration workflow: moving opportunities from raw leads to drilled wells.

```mermaid
flowchart TD
    L[Lead Identified] --> LS{Lead Screening\nCriteria Met?}
    LS -->|Yes| P[Prospect Created]
    LS -->|No| LD[Lead Dropped / Archived]
    
    P --> PR[Prospect Ranking\nRisk & Volume Assessment]
    PR --> PG{Portfolio Gate\nDrill Candidate?}
    PG -->|Yes - Top Tier| WP[Well Program\nProposed]
    PG -->|Monitor| PM[Prospect Monitoring\nAnnual Review]
    PG -->|No| PA[Prospect Abandoned]
    
    WP --> AFE[AFE Prepared\nCost Estimate]
    AFE --> AP{Management\nApproval}
    AP -->|Approved| DR[Drill Ready]
    AP -->|Rejected| PG
    
    DR --> DW[Well Drilled]
    DW --> WR{Well Result}
    WR -->|Discovery| DEV[Move to Development]
    WR -->|Dry Hole| LD2[Well Abandoned\nLessons Captured]
    WR -->|Shows| FA[Further Appraisal]
    
    style L fill:#3b82f6,color:#fff
    style DR fill:#16a34a,color:#fff
    style DEV fill:#16a34a,color:#fff
    style LD fill:#6b7280,color:#fff
    style PA fill:#6b7280,color:#fff
    style LD2 fill:#dc2626,color:#fff
```

**UI Page**: Prospect Maturation Board (Kanban)  
**Columns**: Lead | Screening | Prospect | Ranking | Drill Candidate | Well Program | Approved | Drilled  
**Key Actions**: Advance Stage, Drop Prospect, Request AFE, Record Well Result

---

### 2.2 Seismic Survey Process

```mermaid
flowchart LR
    SA[Survey\nApplication] --> SC[Contractor\nSelection]
    SC --> SQ[Acquisition\nQC] --> SP[Processing] --> SI[Interpretation]
    SI --> SR[Survey Report\n& Recommendations]
    SR --> PM[Update Prospect\nMapping]
    
    style SA fill:#2563eb,color:#fff
    style SR fill:#16a34a,color:#fff
```

**UI Page**: Seismic Survey Tracker  
**Key Actions**: Submit application, Track progress, Upload interpreted horizons, Link to prospects

---

### 2.3 Exploration Well Program Approval

Multi-level gate review before spending on exploratory drilling.

```mermaid
stateDiagram-v2
    [*] --> Proposed: Engineer creates well program
    Proposed --> Technical_Review: Submit for technical review
    Technical_Review --> Budget_Review: Technical approved
    Technical_Review --> Proposed: Revisions required
    Budget_Review --> Management_Approval: Budget allocated
    Budget_Review --> Technical_Review: Budget concerns
    Management_Approval --> Approved: FID signed
    Management_Approval --> Deferred: Deferred to next budget cycle
    Approved --> [*]: Drill order issued
```

**UI Page**: Well Program Approval Wizard (5 Steps)  
**Steps**: 1. Well Objectives → 2. Technical Summary → 3. Risk Assessment → 4. Cost Estimate (AFE) → 5. Approval Sign-off

---

## Part 3: Development Phase Business Processes

### 3.1 Field Development Plan (FDP) Process

The FDP is the masterplan for bringing a discovery into production.

```mermaid
flowchart TD
    D[Discovery\nAppraisal Complete] --> FS[Feasibility\nStudy]
    FS --> CS{Concept\nSelection}
    
    CS --> C1[Option A:\nFixed Platform]
    CS --> C2[Option B:\nFPSO]
    CS --> C3[Option C:\nSubsea Tieback]
    
    C1 & C2 & C3 --> CA[Concept\nAssessment]
    CA --> PS[Preferred\nConcept Selected]
    
    PS --> FDP_PREP[FDP\nDocument Prepared]
    FDP_PREP --> REG[Regulatory\nSubmission]
    REG --> GOVAPP{Government\nApproval}
    GOVAPP -->|Approved| FID[Final Investment\nDecision]
    GOVAPP -->|Conditions| RC[Revise & Resubmit]
    RC --> REG
    FID --> EXEC[Project\nExecution]
    
    style D fill:#2563eb,color:#fff
    style FID fill:#16a34a,color:#fff
    style EXEC fill:#16a34a,color:#fff
```

**UI Page**: FDP Wizard (5 steps) + FDP Document Viewer  
**Key Actions**: Submit concept options, Record concept selection, Submit to regulator, Record approval, Issue FID

---

### 3.2 Development Well Design Workflow

```mermaid
flowchart LR
    WL[Well Location\nDefined] --> WD[Well Design\nGeological Target]
    WD --> CW[Casing & Cementing\nProgram]
    CW --> DD[Directional\nDrilling Plan]
    DD --> WC[Well Cost\nEstimate AFE]
    WC --> WR[Well Program\nReview]
    WR --> APP{Approved?}
    APP -->|Yes| RIG[Rig Assigned\n& Contracted]
    APP -->|Revise| WD
    RIG --> MOB[Mobilize\n& Spud]
    
    style WL fill:#2563eb,color:#fff
    style MOB fill:#16a34a,color:#fff
```

**UI Page**: Development Well Design Workflow  
**Key Forms**: Well location picker (map), Casing design table, AFE builder, Rig selection

---

### 3.3 Construction Progress Tracking

```mermaid
gantt
    title Field Development Construction Schedule
    dateFormat YYYY-MM
    
    section Platform / Facility
    Engineering & Procurement    :2024-01, 18M
    Fabrication                  :2025-04, 12M
    Installation                 :2026-04, 6M
    
    section Wells
    Well 1 Drill & Complete      :2025-10, 4M
    Well 2 Drill & Complete      :2026-02, 4M
    Well 3 Drill & Complete      :2026-06, 4M
    
    section Commissioning
    Pre-commissioning            :2026-09, 2M
    First Oil                    :milestone, 2026-11, 0M
```

**UI Page**: Project Progress Dashboard  
**Shows**: Schedule vs actual, milestone tracker, cost vs budget, open punch items

---

## Part 4: Production Phase Business Processes

### 4.1 Daily Production Operations Workflow

The core production engineer's daily routine:

```mermaid
flowchart TD
    A[Morning: Review\nOvernight Alarms] --> B[Production\nAllocation Check]
    B --> C{Any Well\nAnomalies?}
    C -->|Yes| D[Investigate\nAnomaly]
    D --> E{Root Cause\nDetermined?}
    E -->|Simple fix| F[Field Operator\nAction]
    E -->|Workover needed| WO[Raise Work\nOrder]
    E -->|Need more data| WT[Schedule\nWell Test]
    C -->|No| G[Update Daily\nProduction Report]
    F & WO & WT --> G
    G --> H[Forecast vs Actual\nVariance Check]
    H --> I{Production\nDeferred?}
    I -->|Yes| J[Log Deferment\nReason & Duration]
    I -->|No| K[Close Daily\nWorkflow]
    J --> K
    
    style A fill:#2563eb,color:#fff
    style K fill:#16a34a,color:#fff
    style WO fill:#dc2626,color:#fff
```

**UI Page**: Production Daily Operations Center  
**Key Widgets**: Active well status grid, alarm list, daily vs target comparison, deferment logger

---

### 4.2 Well Performance Monitoring & Optimization

```mermaid
flowchart LR
    subgraph MONITOR[Monitoring]
        WD[Well Data\nCollection]
        PI[Performance\nIndicators]
        DCA[Decline Curve\nAnalysis]
        WD --> PI --> DCA
    end
    
    subgraph DIAGNOSE[Diagnosis]
        PA[Performance\nAlert]
        IN[Investigation\nWorkflow]
        CA[Root Cause\nAnalysis]
        PA --> IN --> CA
    end
    
    subgraph OPTIMIZE[Optimization]
        INT[Intervention\nOptions]
        ECO[Economic\nEvaluation]
        DEC[Decision &\nWork Order]
        INT --> ECO --> DEC
    end
    
    DCA -->|Below threshold| PA
    CA --> INT
    DEC --> WD
```

**UI Page**: Well Performance Optimizer  
**Key Features**: Multi-well comparison, decline curve overlay, intervention history, NPV-ranked recommendations

---

### 4.3 Well Intervention Decision Workflow

```mermaid
stateDiagram-v2
    [*] --> Performance_Issue: Well underperforming
    Performance_Issue --> Data_Collection: Gather diagnostic data
    Data_Collection --> Candidate_Screening: Apply screening criteria
    Candidate_Screening --> Workover_Justified: NPV positive
    Candidate_Screening --> Monitor: NPV marginal
    Candidate_Screening --> P_and_A: NPV negative
    Workover_Justified --> AFE_Raised: Prepare AFE
    AFE_Raised --> Approved: Management approval
    Approved --> Scheduled: Rig scheduled
    Scheduled --> Executed: Work completed
    Executed --> Post_Job_Review: Evaluate success
    Post_Job_Review --> [*]: Close work order
```

**UI Page**: Well Intervention Decision Tool  
**Key Sections**: Candidate list (ranked by NPV uplift), decision matrix, AFE builder, work order tracker

---

### 4.4 Production Allocation & Reporting

```mermaid
flowchart TD
    F[Field Production\nMeasurement] --> ME[Metering\nData Input]
    ME --> SC[Shrinkage &\nCorrections Applied]
    SC --> AL{Allocation\nMethod}
    AL --> PRO[Proportional\nAllocation]
    AL --> TEST[Well Test\nBased Allocation]
    AL --> SIM[Simulation\nBased Allocation]
    PRO & TEST & SIM --> WA[Well-Level\nAllocated Volumes]
    WA --> VLID[Validation &\nReconciliation]
    VLID --> {Balance check}
    --> REP[Monthly Production\nReport Generated]
    REP --> REG_SUB[Regulatory\nSubmission]
    REP --> REV_CALC[Revenue\nCalculation Input]
    
    style F fill:#2563eb,color:#fff
    style REP fill:#16a34a,color:#fff
    style REG_SUB fill:#16a34a,color:#fff
```

**UI Page**: Production Allocation Workbench  
**Key Features**: Metering data entry, allocation method selector, volume reconciliation check, report generator

---

### 4.5 Production Forecasting Process

```mermaid
flowchart LR
    HR[Historical\nProduction Data] --> DCA[Decline Curve\nAnalysis]
    RES[Reservoir\nSimulation] --> FCST[Production\nForecast Model]
    DCA --> FCST
    FCST --> SC{Scenario\nAnalysis}
    SC --> P90[P90\nConservative]
    SC --> P50[P50\nBase Case]
    SC --> P10[P10\nOptimistic]
    P90 & P50 & P10 --> PF[Portfolio\nForecast]
    PF --> ECO[Economic\nModel Input]
    PF --> BUDGET[Budget\nPlanning Input]
    
    style HR fill:#2563eb,color:#fff
    style PF fill:#16a34a,color:#fff
```

**UI Page**: Production Forecasting Workbench  
**Key Features**: DCA parameter fitting, P10/P50/P90 scenario builder, forecast chart, budget integration

---

## Part 5: Reservoir Management Processes

### 5.1 Reservoir Characterization Workflow

```mermaid
flowchart TD
    CORE[Core &\nFluid Samples] --> LAB[Laboratory\nAnalysis]
    LOG[Well Logs] --> INTERP[Log\nInterpretation]
    SEIS[Seismic\nData] --> ATTR[Attribute\nAnalysis]
    
    LAB & INTERP & ATTR --> GEO[Geological\nModel Build]
    GEO --> PROP[Property\nPopulation]
    PROP --> HIST[History\nMatching]
    HIST --> SIM[Reservoir\nSimulation Runs]
    SIM --> FOR[Performance\nForecast]
    
    style CORE fill:#2563eb,color:#fff
    style FOR fill:#16a34a,color:#fff
```

**UI Page**: Reservoir Characterization Summary  
**Key Features**: Rock & fluid property viewer, log correlation display, model status tracker

---

### 5.2 EOR (Enhanced Oil Recovery) Screening

```mermaid
flowchart LR
    PROP[Reservoir\nProperties] --> SCREEN[EOR Screening\nCriteria Check]
    SCREEN --> METHODS{Applicable\nMethods}
    METHODS --> WAG[Water-Alternating-\nGas WAG]
    METHODS --> POLY[Polymer\nFlooding]
    METHODS --> STEAM[Steam\nInjection]
    METHODS --> MISC[Miscible\nGas Injection]
    WAG & POLY & STEAM & MISC --> RANK[Technical &\nEconomic Ranking]
    RANK --> PILOT[Pilot\nDesign]
    PILOT --> FULL[Full-Field\nImplementation]
    
    style PROP fill:#2563eb,color:#fff
    style FULL fill:#16a34a,color:#fff
```

**UI Page**: EOR Screening & Evaluation Tool  
**Key Features**: Screening criteria matrix, EOR method comparison, pilot design parameters

---

### 5.3 Reserves Estimation & Classification

```mermaid
flowchart TD
    RAW[Raw\nVolumes] --> CLASS{Reserves\nClassification}
    CLASS --> P1[1P Proved\nReserves]
    CLASS --> P2[2P Proved+Probable]
    CLASS --> P3[3P Proved+Probable\n+Possible]
    
    P1 & P2 & P3 --> SEC[SEC /\nSPE Compliant\nReport]
    SEC --> REG_BOOK[Regulatory\nReserves Booking]
    SEC --> FIN[Financial\nReporting]
    SEC --> PORT[Portfolio\nManagement]
    
    style RAW fill:#2563eb,color:#fff
    style SEC fill:#16a34a,color:#fff
```

**UI Page**: Reserves Classification & Reporting  
**Key Features**: Volume entry by category, classification workflow, compliance checker, certification record

---

## Part 6: HSE & Compliance Processes

### 6.1 HSE Incident Management

```mermaid
flowchart TD
    INC[Incident\nOccurs] --> REP[Immediate\nReport Filed]
    REP --> TRIAGE{Severity\nClassification}
    
    TRIAGE --> T1[Tier 1:\nFatality / Major]
    TRIAGE --> T2[Tier 2:\nSerious Injury]
    TRIAGE --> T3[Tier 3:\nRecordable]
    TRIAGE --> T4[Tier 4:\nNear Miss]
    
    T1 --> REGNOTIFY[Regulatory\nNotification]
    T1 & T2 --> RCA[Root Cause\nAnalysis]
    T3 & T4 --> INVEST[Investigation]
    
    RCA & INVEST --> CA[Corrective\nActions]
    CA --> IMPL[Actions\nImplemented]
    IMPL --> VERIFY[Verification &\nClose-out]
    VERIFY --> LEARN[Lessons\nLearned Shared]
    
    REGNOTIFY --> RCA
    
    style INC fill:#dc2626,color:#fff
    style T1 fill:#dc2626,color:#fff
    style LEARN fill:#16a34a,color:#fff
```

**UI Page**: HSE Incident Management  
**Key Features**: Incident report form (API RP 754 compliant), investigation tracker, corrective action log, regulatory notification

---

### 6.2 Permit & Regulatory Compliance

```mermaid
flowchart LR
    OB[Obligation\nIdentified] --> CAL[Calendar\nScheduled]
    CAL --> DUE{Due Date\nApproaching?}
    DUE -->|>30 days| MONITOR[Monitor]
    DUE -->|<30 days| PREP[Prepare\nSubmission]
    PREP --> SUBMIT[Submit\nto Regulator]
    SUBMIT --> RESP{Regulator\nResponse}
    RESP -->|Accepted| CLOSE[Obligation\nClosed]
    RESP -->|Conditions| REMEDY[Remediation\nRequired]
    REMEDY --> PREP
    CLOSE --> RECORD[Record\nCompliance]
    MONITOR --> DUE
    
    style OB fill:#2563eb,color:#fff
    style CLOSE fill:#16a34a,color:#fff
```

**UI Page**: Compliance Obligation Calendar  
**Key Features**: Due-date calendar, submission tracker, regulator response log, overdue alerts

---

## Part 7: Decommissioning Processes

### 7.1 Well P&A (Plug & Abandon) Workflow

```mermaid
flowchart TD
    ELIM[Economic\nLimit Reached] --> P_A_PLAN[P&A Program\nPrepared]
    P_A_PLAN --> REGAPP{Regulatory\nApproval}
    REGAPP -->|Approved| PERM_OR_TEMP{Permanent or\nTemporary?}
    PERM_OR_TEMP -->|Permanent| CEMENT[Cement\nPlug Program]
    PERM_OR_TEMP -->|Temporary| TEMP_CAP[Temporary\nCaps & Barriers]
    CEMENT --> EXEC_PA[Execute\nP&A Operation]
    EXEC_PA --> VERIFY_PA[Verify Well\nIntegrity]
    VERIFY_PA --> SITE_REST[Site\nRestoration]
    SITE_REST --> CLOSE[Well\nAbandoned &\nClosed]
    TEMP_CAP --> MONITOR_TMP[Monitor\nTemporarily\nAbandoned Well]
    
    style ELIM fill:#6b7280,color:#fff
    style CLOSE fill:#16a34a,color:#fff
```

**UI Page**: Well P&A Planning & Execution  
**Key Features**: P&A candidate list, program builder, operations tracker, integrity verification, site restoration checklist

---

### 7.2 Facility Decommissioning

```mermaid
flowchart LR
    FD_PLAN[Decommission\nPlan] --> ENV[Environmental\nAssessment]
    ENV --> REG_SUB[Regulatory\nSubmission]
    REG_SUB --> DEPC[Deprecommission\nCost Estimate OPEX]
    DEPC --> CLEAN[Cleaning &\nPurging]
    CLEAN --> DEMO[Dismantlement\n& Removal]
    DEMO --> SITE[Site Survey\n& Verification]
    SITE --> CERT[Certificate of\nCompletion]
    
    style FD_PLAN fill:#6b7280,color:#fff
    style CERT fill:#16a34a,color:#fff
```

**UI Page**: Facility Decommissioning Tracker  
**Key Features**: Decommission plan wizard, task tracker, cost tracker, regulatory milestones, site restoration progress

---

## Part 8: Economics & Decision Support

### 8.1 AFE (Authorization for Expenditure) Process

```mermaid
stateDiagram-v2
    [*] --> Draft: Engineer creates AFE
    Draft --> Technical_Review: Submit
    Technical_Review --> Budget_Check: Tech approved
    Technical_Review --> Draft: Revisions needed
    Budget_Check --> L1_Approval: Within L1 authority
    Budget_Check --> L2_Approval: Above L1 authority
    Budget_Check --> Board_Approval: Above L2 authority
    L1_Approval --> Approved: L1 signs
    L2_Approval --> Approved: L2 signs
    Board_Approval --> Approved: Board approves
    Approved --> Committed: Work order issued
    Committed --> Closed: Work complete & invoiced
    Closed --> [*]
```

**UI Page**: AFE Management  
**Key Features**: AFE builder with cost breakdown, approval workflow, commitment tracking, actual vs. AFE variance

---

### 8.2 Economic Evaluation Process

```mermaid
flowchart TD
    INPUT[Production\nForecast + Costs] --> ECO[Economic\nModel]
    ECO --> PRICE[Price Deck\nScenarios]
    PRICE --> METRICS[NPV IRR\nPayback PI]
    METRICS --> SENS[Sensitivity\nAnalysis]
    SENS --> RISK[Risk-Adjusted\nEvaluation]
    RISK --> RANK[Project\nRanking]
    RANK --> PORT[Portfolio\nOptimization]
    
    style INPUT fill:#2563eb,color:#fff
    style PORT fill:#16a34a,color:#fff
```

**UI Page**: Economic Evaluation Workbench  
**Key Features**: Price deck input, NPV/IRR calculation, sensitivity tornado chart, scenario comparison

---

## Part 9: User Role → Process Mapping

| Role | Primary Processes | Secondary Processes |
|------|------------------|---------------------|
| Exploration Geoscientist | Prospect Maturation, Seismic Surveys | Well Program Review |
| Drilling Engineer | Well Design, AFE, P&A Planning | Construction Progress |
| Production Engineer | Daily Operations, Well Optimization, Intervention | Allocation, Reporting |
| Reservoir Engineer | Characterization, Simulation, EOR, Reserves | Forecasting |
| Development Engineer | FDP, Construction, Commissioning | Economics |
| Facilities Engineer | Facility Design, Decommissioning | Maintenance |
| HSE Officer | Incident Management, Permits | Compliance Reporting |
| Asset Manager | AFE Approvals, Gate Reviews, Portfolio | All dashboards |
| Financial Analyst | Production Accounting, Economics | Reserves Reporting |
| Data Manager | PPDM Data Management (TreeView) | Quality Checks |

---

## Part 10: Page-to-Process Mapping Matrix

| UI Page | Business Process | Stage | Priority |
|---------|-----------------|-------|----------|
| Field Dashboard | Daily status overview | All | P0 |
| Prospect Maturation Board | Prospect lifecycle | Exploration | P1 |
| Well Program Approval | Drill authorization | Exploration | P1 |
| Seismic Survey Tracker | Seismic acquisition | Exploration | P2 |
| FDP Wizard | Field development plan | Development | P1 |
| Development Well Design | Well engineering | Development | P1 |
| Construction Progress | Project execution | Development | P2 |
| Production Daily Operations | Daily monitoring | Production | P0 |
| Well Performance Optimizer | Well optimization | Production | P0 |
| Well Intervention Decision | Intervention planning | Production | P1 |
| Production Allocation Workbench | Volume allocation | Production | P1 |
| Production Forecasting Workbench | Forecasting | Production | P2 |
| Reservoir Characterization Summary | Reservoir data | Reservoir | P2 |
| EOR Screening Tool | EOR evaluation | Reservoir | P3 |
| Reserves Classification | Reserves reporting | All | P1 |
| HSE Incident Management | Safety management | All | P0 |
| Compliance Obligation Calendar | Regulatory compliance | All | P1 |
| Well P&A Workflow | Abandonment | Decommissioning | P2 |
| Facility Decommissioning Tracker | Site restoration | Decommissioning | P2 |
| AFE Management | Cost authorization | All | P1 |
| Economic Evaluation Workbench | Economics | All | P2 |
| PPDM Data Management (TreeView) | Data administration | All | P0* |

*P0 = Data management priority (separate from engineer UX)

---

*This document is the authoritative reference for UI business process design. Engineers should recognize their daily work in these process flows. If a new page is proposed that does not map to a process in this document, the process must be added here first.*
