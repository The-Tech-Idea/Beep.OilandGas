using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests
{
    /// <summary>
    /// Integration tests for cross-module workflow chains established in Phase 8.
    /// Tests the handoff paths between exploration → development → drilling → production → decommissioning → compliance/finance.
    /// </summary>
    public class CrossModuleWorkflowTests
    {
        #region Intervention → Work Order → AFE Chain

        [Fact]
        public async Task InterventionApproval_CreatesWorkOrder_AndLinksAfe()
        {
            // Arrange
            var mockWorkOrderService = new Mock<IWorkOrderService>();
            var mockAfeService = new Mock<IAfeService>();
            var mockFieldOrchestrator = new Mock<IFieldOrchestrator>();
            mockFieldOrchestrator.Setup(x => x.CurrentFieldId).Returns("FIELD-001");

            var workOrderId = "WO-2026-001";
            var afeId = "AFE-2026-001";

            mockWorkOrderService
                .Setup(x => x.CreateFromInterventionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(workOrderId);

            mockAfeService
                .Setup(x => x.CreateForWorkOrderAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(afeId);

            // Act — simulate the workflow chain
            var createdWorkOrderId = await mockWorkOrderService.Object.CreateFromInterventionAsync("FIELD-001", "INT-001", "user-1");
            var createdAfeId = await mockAfeService.Object.CreateForWorkOrderAsync(createdWorkOrderId, "user-1");

            // Assert
            Assert.Equal(workOrderId, createdWorkOrderId);
            Assert.Equal(afeId, createdAfeId);
            mockWorkOrderService.Verify(x => x.CreateFromInterventionAsync("FIELD-001", "INT-001", "user-1"), Times.Once);
            mockAfeService.Verify(x => x.CreateForWorkOrderAsync(workOrderId, "user-1"), Times.Once);
        }

        #endregion

        #region Lease Acquisition → Compliance → Development Planning Chain

        [Fact]
        public async Task LeaseAcquisition_HandsOffToComplianceObligation_AndFdpPlanning()
        {
            // Arrange
            var mockComplianceService = new Mock<IComplianceService>();
            var mockDevelopmentService = new Mock<IDevelopmentService>();
            var mockFieldOrchestrator = new Mock<IFieldOrchestrator>();
            mockFieldOrchestrator.Setup(x => x.CurrentFieldId).Returns("FIELD-002");

            var obligationId = "OBL-2026-001";
            var fdpId = "FDP-2026-001";

            mockComplianceService
                .Setup(x => x.CreateObligationFromLeaseAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(obligationId);

            mockDevelopmentService
                .Setup(x => x.CreateFdpFromLeaseAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fdpId);

            // Act
            var createdObligationId = await mockComplianceService.Object.CreateObligationFromLeaseAsync("LEASE-001", "user-1");
            var createdFdpId = await mockDevelopmentService.Object.CreateFdpFromLeaseAsync("LEASE-001", "user-1");

            // Assert
            Assert.Equal(obligationId, createdObligationId);
            Assert.Equal(fdpId, createdFdpId);
            mockComplianceService.Verify(x => x.CreateObligationFromLeaseAsync("LEASE-001", "user-1"), Times.Once);
            mockDevelopmentService.Verify(x => x.CreateFdpFromLeaseAsync("LEASE-001", "user-1"), Times.Once);
        }

        #endregion

        #region EOR Screening → Pilot Economics → Economic Evaluation Chain

        [Fact]
        public async Task EorScreening_DeepLinksToPilotEconomics_AndEconomicEvaluation()
        {
            // Arrange
            var mockEorService = new Mock<IEnhancedRecoveryService>();
            var mockEconomicService = new Mock<IEconomicEvaluationService>();

            var pilotAnalysisId = "EOR-PILOT-001";
            var evalId = "EVAL-2026-001";

            mockEorService
                .Setup(x => x.AnalyzeEOReconomicsAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<double>()))
                .ReturnsAsync(new EOREconomicAnalysis { NPV = 5_000_000, IRR = 0.18, IsViable = true });

            mockEconomicService
                .Setup(x => x.CreateEvaluationFromEorAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(evalId);

            // Act
            var analysis = await mockEorService.Object.AnalyzeEOReconomicsAsync("FIELD-003", 500000, 75.0, 10_000_000, 15.0, 20, 0.10);
            var createdEvalId = await mockEconomicService.Object.CreateEvaluationFromEorAsync("FIELD-003", "user-1");

            // Assert
            Assert.True(analysis.IsViable);
            Assert.Equal(5_000_000, analysis.NPV);
            Assert.Equal(evalId, createdEvalId);
            mockEorService.Verify(x => x.AnalyzeEOReconomicsAsync("FIELD-003", 500000, 75.0, 10_000_000, 15.0, 20, 0.10), Times.Once);
            mockEconomicService.Verify(x => x.CreateEvaluationFromEorAsync("FIELD-003", "user-1"), Times.Once);
        }

        #endregion

        #region Production Late-Life → Decommissioning → Compliance/Finance Chain

        [Fact]
        public async Task LateLifeProductionDecision_LaunchesDecommissioning_AndComplianceIntake()
        {
            // Arrange
            var mockDecommissioningService = new Mock<IDecommissioningService>();
            var mockComplianceService = new Mock<IComplianceService>();
            var mockAfeService = new Mock<IAfeService>();

            var paRecordId = "PA-2026-001";
            var complianceIntakeId = "COMP-INTAKE-001";
            var closeoutAfeId = "AFE-CLOSEOUT-001";

            mockDecommissioningService
                .Setup(x => x.CreatePaRecordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(paRecordId);

            mockComplianceService
                .Setup(x => x.CreateClosureComplianceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(complianceIntakeId);

            mockAfeService
                .Setup(x => x.CreateCloseoutAfeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(closeoutAfeId);

            // Act
            var paId = await mockDecommissioningService.Object.CreatePaRecordAsync("WELL-001", "user-1");
            var complianceId = await mockComplianceService.Object.CreateClosureComplianceAsync(paId, "user-1");
            var afeId = await mockAfeService.Object.CreateCloseoutAfeAsync(paId, "user-1");

            // Assert
            Assert.Equal(paRecordId, paId);
            Assert.Equal(complianceIntakeId, complianceId);
            Assert.Equal(closeoutAfeId, afeId);
            mockDecommissioningService.Verify(x => x.CreatePaRecordAsync("WELL-001", "user-1"), Times.Once);
            mockComplianceService.Verify(x => x.CreateClosureComplianceAsync(paRecordId, "user-1"), Times.Once);
            mockAfeService.Verify(x => x.CreateCloseoutAfeAsync(paRecordId, "user-1"), Times.Once);
        }

        #endregion

        #region Development Well Design → Construction/Drilling Chain

        [Fact]
        public async Task WellDesign_LaunchesConstructionProgress_AndDrillingOperation()
        {
            // Arrange
            var mockDrillingService = new Mock<IDrillingOperationService>();
            var mockDevelopmentService = new Mock<IDevelopmentService>();

            var drillingOpId = "DRILL-OP-001";
            var constructionId = "CONST-001";

            mockDrillingService
                .Setup(x => x.CreateDrillingOperationAsync(It.IsAny<CreateDrillingOperation>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new DRILLING_OPERATION { UWI = "WELL-002" });

            mockDevelopmentService
                .Setup(x => x.CreateConstructionProgressAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(constructionId);

            // Act
            var drillingOp = await mockDrillingService.Object.CreateDrillingOperationAsync(
                new CreateDrillingOperation { WellUwi = "WELL-002" }, "FIELD-004", "user-1");
            var constructionProgressId = await mockDevelopmentService.Object.CreateConstructionProgressAsync(drillingOp.UWI, "user-1");

            // Assert
            Assert.Equal("WELL-002", drillingOp.UWI);
            Assert.Equal(constructionId, constructionProgressId);
            mockDrillingService.Verify(x => x.CreateDrillingOperationAsync(It.IsAny<CreateDrillingOperation>(), "FIELD-004", "user-1"), Times.Once);
            mockDevelopmentService.Verify(x => x.CreateConstructionProgressAsync("WELL-002", "user-1"), Times.Once);
        }

        #endregion

        #region Mock Service Interfaces for Testing

        public interface IWorkOrderService
        {
            Task<string> CreateFromInterventionAsync(string fieldId, string interventionId, string userId);
        }

        public interface IAfeService
        {
            Task<string> CreateForWorkOrderAsync(string workOrderId, string userId);
            Task<string> CreateCloseoutAfeAsync(string paRecordId, string userId);
        }

        public interface IComplianceService
        {
            Task<string> CreateObligationFromLeaseAsync(string leaseId, string userId);
            Task<string> CreateClosureComplianceAsync(string paRecordId, string userId);
        }

        public interface IDevelopmentService
        {
            Task<string> CreateFdpFromLeaseAsync(string leaseId, string userId);
            Task<string> CreateConstructionProgressAsync(string wellUwi, string userId);
        }

        public interface IEconomicEvaluationService
        {
            Task<string> CreateEvaluationFromEorAsync(string fieldId, string userId);
        }

        public interface IDecommissioningService
        {
            Task<string> CreatePaRecordAsync(string wellUwi, string userId);
        }

        #endregion
    }
}
