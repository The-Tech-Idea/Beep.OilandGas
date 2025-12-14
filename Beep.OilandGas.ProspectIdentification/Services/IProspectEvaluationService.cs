using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Service for evaluating geological prospects.
    /// </summary>
    public interface IProspectEvaluationService
    {
        /// <summary>
        /// Evaluates a prospect and returns evaluation results.
        /// </summary>
        Task<ProspectEvaluationDto> EvaluateProspectAsync(string prospectId);

        /// <summary>
        /// Gets all prospects.
        /// </summary>
        Task<List<ProspectDto>> GetProspectsAsync(string? fieldId = null);

        /// <summary>
        /// Gets a prospect by ID.
        /// </summary>
        Task<ProspectDto?> GetProspectAsync(string prospectId);

        /// <summary>
        /// Creates a new prospect.
        /// </summary>
        Task<ProspectDto> CreateProspectAsync(CreateProspectDto createDto);

        /// <summary>
        /// Updates a prospect.
        /// </summary>
        Task<ProspectDto> UpdateProspectAsync(string prospectId, UpdateProspectDto updateDto);

        /// <summary>
        /// Deletes a prospect.
        /// </summary>
        Task DeleteProspectAsync(string prospectId);
    }
}

