using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for well test analysis operations.
    /// Provides methods for pressure transient analysis, reservoir characterization, and well performance evaluation.
    /// </summary>
    public interface IWellTestAnalysisService
    {
        #region Build-up Analysis Methods

        /// <summary>
        /// Analyzes a build-up test using the Horner method.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testData">Well test data for analysis</param>
        /// <param name="userId">User performing the analysis</param>
        /// <returns>Analysis result containing permeability, skin factor, and reservoir pressure</returns>
        Task<WellTestAnalysisResult> AnalyzeBuildUpHornerAsync(string wellUWI, WellTestData testData, string userId);

        /// <summary>
        /// Analyzes a build-up test using the Miller-Dyes-Hutchinson (MDH) method.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testData">Well test data for analysis</param>
        /// <param name="userId">User performing the analysis</param>
        /// <returns>Analysis result with alternative method comparison</returns>
        Task<WellTestAnalysisResult> AnalyzeBuildUpMDHAsync(string wellUWI, WellTestData testData, string userId);

        #endregion

        #region Drawdown Analysis Methods

        /// <summary>
        /// Analyzes a drawdown test using semi-log analysis.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testData">Well test data for analysis</param>
        /// <param name="userId">User performing the analysis</param>
        /// <returns>Analysis result with drawdown parameters</returns>
        Task<WellTestAnalysisResult> AnalyzeDrawdownAsync(string wellUWI, WellTestData testData, string userId);

        #endregion

        #region Derivative Analysis Methods

        /// <summary>
        /// Calculates pressure derivative for diagnostic plots.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="pressureData">Time-pressure data points</param>
        /// <param name="smoothingFactor">Smoothing factor for derivative calculation (optional)</param>
        /// <returns>List of pressure-time points with calculated derivatives</returns>
        Task<List<PressureTimePoint>> CalculateDerivativeAsync(string wellUWI, List<PressureTimePoint> pressureData, double smoothingFactor = 0.05);

        /// <summary>
        /// Identifies reservoir model from derivative signature.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="derivativeData">Derivative analysis data</param>
        /// <returns>Identified reservoir model and characteristics</returns>
        Task<ReservoirModel> IdentifyReservoirModelAsync(string wellUWI, List<PressureTimePoint> derivativeData);

        #endregion

        #region Type Curve Matching Methods

        /// <summary>
        /// Performs automated type curve matching on test data.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testData">Well test data for matching</param>
        /// <param name="userId">User performing the analysis</param>
        /// <returns>Type curve matching results with model parameters</returns>
        Task<TypeCurveMatchResult> PerformTypeCurveMatchingAsync(string wellUWI, WellTestData testData, string userId);

        #endregion

        #region Multi-rate Analysis Methods

        /// <summary>
        /// Performs multi-rate test analysis using superposition.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="multiRateData">Multi-rate test data</param>
        /// <param name="userId">User performing the analysis</param>
        /// <returns>Multi-rate analysis results</returns>
        Task<WellTestAnalysisResult> AnalyzeMultiRateAsync(string wellUWI, MultiRateTestData multiRateData, string userId);

        /// <summary>
        /// Performs deconvolution analysis on variable rate data.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="variableRateData">Variable rate production data</param>
        /// <param name="userId">User performing the analysis</param>
        /// <returns>Deconvolved constant rate response</returns>
        Task<WellTestAnalysisResult> PerformDeconvolutionAsync(string wellUWI, VariableRateData variableRateData, string userId);

        #endregion

        #region Boundary Detection Methods

        /// <summary>
        /// Detects reservoir boundaries and faults from test data.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testData">Well test data for boundary detection</param>
        /// <returns>List of detected boundaries and their characteristics</returns>
        Task<List<ReservoirBoundary>> DetectBoundariesAsync(string wellUWI, WellTestData testData);

        #endregion

        #region Data Management Methods

        /// <summary>
        /// Saves well test analysis results to persistent storage.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="analysisResult">Analysis results to save</param>
        /// <param name="userId">User performing the save</param>
        Task SaveAnalysisResultAsync(string wellUWI, WellTestAnalysisResult analysisResult, string userId);

        /// <summary>
        /// Retrieves well test analysis results from persistent storage.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testDate">Date of the test (optional)</param>
        /// <returns>List of analysis results for the well</returns>
        Task<List<WellTestAnalysisResult>> GetAnalysisResultsAsync(string wellUWI, DateTime? testDate = null);

        /// <summary>
        /// Retrieves analysis history for a well.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="startDate">Start date for history</param>
        /// <param name="endDate">End date for history</param>
        /// <returns>List of historical analysis results</returns>
        Task<List<WellTestAnalysisResult>> GetAnalysisHistoryAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates previously saved analysis results.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="analysisId">ID of analysis to update</param>
        /// <param name="updatedResult">Updated analysis results</param>
        /// <param name="userId">User performing the update</param>
        Task UpdateAnalysisResultAsync(string wellUWI, string analysisId, WellTestAnalysisResult updatedResult, string userId);

        #endregion

        #region Reporting Methods

        /// <summary>
        /// Generates a comprehensive well test analysis report.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="analysisIds">List of analysis IDs to include in report</param>
        /// <param name="userId">User generating the report</param>
        /// <returns>Generated report data</returns>
        Task<WellTestAnalysisReport> GenerateAnalysisReportAsync(string wellUWI, List<string> analysisIds, string userId);

        /// <summary>
        /// Exports analysis results in specified format.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="analysisId">Analysis ID to export</param>
        /// <param name="format">Export format (CSV, JSON, Excel, PDF)</param>
        /// <returns>Exported data as byte array</returns>
        Task<byte[]> ExportAnalysisResultAsync(string wellUWI, string analysisId, string format);

        #endregion

        #region Validation and Quality Methods

        /// <summary>
        /// Validates well test data quality.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="testData">Test data to validate</param>
        /// <returns>Validation result with quality metrics</returns>
        Task<TestDataValidationResult> ValidateTestDataAsync(string wellUWI, WellTestData testData);

        /// <summary>
        /// Compares analysis results from different methods.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="analysisIds">IDs of analyses to compare</param>
        /// <returns>Comparison results showing differences and confidence levels</returns>
        Task<AnalysisComparisonResult> CompareAnalysisMethodsAsync(string wellUWI, List<string> analysisIds);

        #endregion

        #region Visualization Methods

        /// <summary>
        /// Generates a diagnostic plot (log-log or semi-log) for visualization.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="analysisId">Analysis ID to plot</param>
        /// <param name="plotType">Type of plot (LogLog, SemiLog, Derivative)</param>
        /// <returns>Plot image data as byte array</returns>
        Task<byte[]> GeneratePlotAsync(string wellUWI, string analysisId, string plotType);

        /// <summary>
        /// Generates a type curve match plot showing data and matched type curve.
        /// </summary>
        /// <param name="wellUWI">Well unique well identifier</param>
        /// <param name="matchResultId">Type curve match result ID</param>
        /// <returns>Plot image data as byte array</returns>
        Task<byte[]> GenerateTypeCurvePlotAsync(string wellUWI, string matchResultId);

        #endregion
    }

    #region Supporting DTOs

    /// <summary>DTO for multi-rate test data.</summary>
    

    /// <summary>DTO for rate change event.</summary>
    

    /// <summary>DTO for variable rate data.</summary>
    

    /// <summary>DTO for production history event.</summary>
    

    /// <summary>DTO for type curve match result.</summary>
    

    /// <summary>DTO for well test analysis report.</summary>
    

    /// <summary>DTO for test data validation result.</summary>
    

    /// <summary>DTO for analysis comparison result.</summary>
    

    /// <summary>DTO for individual comparison entry.</summary>
    

    #endregion
}

