using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;
using WELL = Beep.OilandGas.PPDM39.Models.WELL;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Repository for Well entities and related business logic
    /// Handles well structures (using WELL_XREF), well status, and child entities
    /// Uses PPDMGenericRepository directly for all table operations
    /// Uses AppFilter for all queries (no SQL)
    /// </summary>
    public partial class WellServices
    {
        #region Fields and Dependencies

        protected readonly PPDMGenericRepository _wellRepository;
        protected readonly PPDMGenericRepository _wellXrefRepository;
        protected readonly PPDMGenericRepository _wellStatusRepository;
        protected readonly PPDMGenericRepository _wellStatusReferenceRepository;
        protected readonly IPPDM39DefaultsRepository _defaults;
        protected readonly IPPDMMetadataRepository _metadata;
        protected readonly IDMEEditor _editor;
        protected readonly string _connectionName;

        // Well Status XREF UnitOfWork (lazy initialization)
        private IUnitOfWorkWrapper _wellStatusXrefUnitOfWork;
        private readonly object _unitOfWorkLock = new object();
        private const string WELL_STATUS_XREF_TABLE = "R_WELL_STATUS_XREF";

        // Default Well Status Types (STATUS_TYPE values from PPDM 3.9 Well Facets)
        // These are the standard facets that should be initialized for every well/wellbore
        // Based on PPDM 3.9 Well Facets documentation
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES = new List<string>
        {
            "Business Interest",           // Well/Wellbore - Only 1 value (mutually exclusive, ranked)
            "Business Life Cycle Phase",   // Well - Changes predictably, may reoccur
            "Business Intention",          // Well - Set at Drilling start, doesn't change unless reverts to Planning
            "Operatorship",                // Well - May change if accountability transfers
            "Outcome",                     // Well - Doesn't change unless Business Life Cycle reverts to Planning
            "Lahee Class",                 // Well - Doesn't change over life cycle
            "Role",                        // Wellbore - May change over life cycle
            "Play Type",                   // Well - May change if Role changes or different formation completed
            "Well Structure",               // Well - May change as new wellbores added
            "Trajectory Type",             // Wellbore - Doesn't change over life cycle
            "Fluid Direction",             // Wellhead Stream - Can change
            "Well Reporting Class",        // Well - Can change over life cycle
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion - Can change
            "Wellbore Status",              // Wellbore - Slowly changing, measures milestones
            "Well Status"                   // Well - Summary state, may change infrequently
        };

        // STATUS_TYPEs specific to Well level (not Wellbore)
        // Based on PPDM 3.9 documentation: Well Component = Well
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELL = new List<string>
        {
            "Business Interest",           // Well or Wellbore (but typically Well)
            "Business Life Cycle Phase",   // Well
            "Business Intention",          // Well
            "Operatorship",                // Well
            "Outcome",                     // Well
            "Lahee Class",                 // Well
            "Play Type",                   // Well
            "Well Structure",               // Well
            "Well Reporting Class",        // Well
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion
            "Well Status"                   // Well
        };

        // STATUS_TYPEs specific to Wellbore level
        // Based on PPDM 3.9 documentation: Well Component = Wellbore
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLBORE = new List<string>
        {
            "Business Interest",           // Well or Wellbore (can be at Wellbore level)
            "Role",                        // Wellbore
            "Trajectory Type",             // Wellbore
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion
            "Wellbore Status"              // Wellbore
        };

        // STATUS_TYPEs specific to Wellhead Stream level
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLHEAD_STREAM = new List<string>
        {
            "Fluid Direction"              // Wellhead Stream
        };

        #endregion

        #region Constructor

        public WellServices(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            // Create generic repositories for each table
            _wellRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(Beep.OilandGas.PPDM39.Models.WELL), connectionName, "WELL");

            _wellXrefRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(WELL_XREF), connectionName, "WELL_XREF");

            _wellStatusRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(WELL_STATUS), connectionName, "WELL_STATUS");

            _wellStatusReferenceRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(R_WELL_STATUS), connectionName, "R_WELL_STATUS");
        }

        #endregion

        #region Core Well Operations

        /// <summary>
        /// Gets well by UWI (Unique Well Identifier)
        /// </summary>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> GetByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellRepository.GetAsync(filters);
            return result.Cast<Beep.OilandGas.PPDM39.Models.WELL>().FirstOrDefault();
        }

        /// <summary>
        /// Creates a new well
        /// Optionally initializes default status types for the well
        /// </summary>
        /// <param name="well">Well entity to create</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="initializeDefaultStatuses">If true, creates default status records for all standard STATUS_TYPEs</param>
        /// <returns>Created well entity</returns>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> CreateWellAsync(
            Beep.OilandGas.PPDM39.Models.WELL well, 
            string userId,
            bool initializeDefaultStatuses = true)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellRepository.InsertAsync(well, userId);
            var createdWell = result as Beep.OilandGas.PPDM39.Models.WELL;

            // Initialize default status types for the well
            if (initializeDefaultStatuses && createdWell != null && !string.IsNullOrWhiteSpace(createdWell.UWI))
            {
                await InitializeDefaultWellStatusesAsync(createdWell.UWI, userId);
            }

            return createdWell;
        }

        /// <summary>
        /// Updates an existing well
        /// </summary>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> UpdateWellAsync(
            Beep.OilandGas.PPDM39.Models.WELL well, 
            string userId)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellRepository.UpdateAsync(well, userId);
            return result as Beep.OilandGas.PPDM39.Models.WELL;
        }

        /// <summary>
        /// Soft deletes a well (sets ACTIVE_IND = 'N')
        /// </summary>
        public async Task<bool> SoftDeleteWellAsync(string uwi, string userId)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            return await _wellRepository.SoftDeleteAsync(uwi, userId);
        }

        /// <summary>
        /// Hard deletes a well from the database
        /// </summary>
        public async Task<bool> DeleteWellAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            return await _wellRepository.DeleteAsync(uwi);
        }

        #endregion
    }
}
