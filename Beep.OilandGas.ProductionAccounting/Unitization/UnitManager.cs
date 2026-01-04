using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Unitization
{
    /// <summary>
    /// Manages unit agreements and operations.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
    /// </summary>
    public class UnitManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<UnitManager>? _logger;
        private readonly string _connectionName;
        private const string UNIT_AGREEMENT_TABLE = "UNIT_AGREEMENT";
        private const string UNIT_OPERATING_AGREEMENT_TABLE = "UNIT_OPERATING_AGREEMENT";
        private const string PARTICIPATING_AREA_TABLE = "PARTICIPATING_AREA";

        public UnitManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<UnitManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a unit agreement.
        /// </summary>
        public async Task<UNIT_AGREEMENT> CreateUnitAgreementAsync(
            string unitName,
            DateTime effectiveDate,
            string unitOperator,
            string userId = "system",
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(unitName))
                throw new ArgumentException("Unit name cannot be null or empty.", nameof(unitName));

            if (string.IsNullOrEmpty(unitOperator))
                throw new ArgumentException("Unit operator cannot be null or empty.", nameof(unitOperator));

            var unitAgreement = new UNIT_AGREEMENT
            {
                UNIT_AGREEMENT_ID = Guid.NewGuid().ToString(),
                UNIT_NAME = unitName,
                EFFECTIVE_DATE = effectiveDate,
                UNIT_OPERATOR = unitOperator
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(unitAgreement, userId);
            var result = dataSource.InsertEntity(UNIT_AGREEMENT_TABLE, unitAgreement);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create unit agreement {UnitId}: {Error}", unitAgreement.UNIT_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save unit agreement: {errorMessage}");
            }

            _logger?.LogDebug("Created unit agreement {UnitId} in database", unitAgreement.UNIT_ID);
            return unitAgreement;
        }

        /// <summary>
        /// Creates a unit agreement (synchronous wrapper).
        /// </summary>
        public UNIT_AGREEMENT CreateUnitAgreement(
            string unitName,
            DateTime effectiveDate,
            string unitOperator)
        {
            return CreateUnitAgreementAsync(unitName, effectiveDate, unitOperator).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a unit agreement by ID.
        /// </summary>
        public async Task<UNIT_AGREEMENT?> GetUnitAgreementAsync(string unitId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(unitId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UNIT_ID", Operator = "=", FilterValue = unitId }
            };

            var results = await dataSource.GetEntityAsync(UNIT_AGREEMENT_TABLE, filters);
            return results?.FirstOrDefault() as UNIT_AGREEMENT;
        }

        /// <summary>
        /// Gets a unit agreement by ID (synchronous wrapper).
        /// </summary>
        public UNIT_AGREEMENT? GetUnitAgreement(string unitId)
        {
            return GetUnitAgreementAsync(unitId).GetAwaiter().GetResult();
        }
    }
}

//        /// <summary>
//        /// Creates a participating area.
//        /// </summary>
//        public ParticipatingArea CreateParticipatingArea(
//            string unitId,
//            string participatingAreaName,
//            DateTime effectiveDate)
//        {
//            if (string.IsNullOrEmpty(unitId))
//                throw new ArgumentException("Unit ID cannot be null or empty.", nameof(unitId));

//            if (!unitAgreements.ContainsKey(unitId))
//                throw new ArgumentException($"Unit agreement {unitId} not found.", nameof(unitId));

//            var participatingArea = new ParticipatingArea
//            {
//                ParticipatingAreaId = Guid.NewGuid().ToString(),
//                UnitId = unitId,
//                ParticipatingAreaName = participatingAreaName,
//                EffectiveDate = effectiveDate
//            };

//            participatingAreas[participatingArea.ParticipatingAreaId] = participatingArea;

//            // Link to unit agreement
//            var unitAgreement = unitAgreements[unitId];
//            unitAgreement.ParticipatingArea = participatingArea;

//            return participatingArea;
//        }

//        /// <summary>
//        /// Adds a tract to a participating area.
//        /// </summary>
//        public void AddTractToParticipatingArea(
//            string participatingAreaId,
//            string tractId,
//            decimal tractParticipation,
//            decimal workingInterest,
//            decimal netRevenueInterest)
//        {
//            if (!participatingAreas.TryGetValue(participatingAreaId, out var participatingArea))
//                throw new ArgumentException($"Participating area {participatingAreaId} not found.", nameof(participatingAreaId));

//            if (tractParticipation < 0 || tractParticipation > 100)
//                throw new ArgumentException("Tract participation must be between 0 and 100.", nameof(tractParticipation));

//            if (workingInterest < 0 || workingInterest > 1)
//                throw new ArgumentException("Working interest must be between 0 and 1.", nameof(workingInterest));

//            if (netRevenueInterest < 0 || netRevenueInterest > 1)
//                throw new ArgumentException("Net revenue interest must be between 0 and 1.", nameof(netRevenueInterest));

//            var tractParticipationObj = new TractParticipation
//            {
//                TractId = tractId,
//                UnitId = participatingArea.UnitId,
//                ParticipatingAreaId = participatingAreaId,
//                ParticipationPercentage = tractParticipation,
//                WorkingInterest = workingInterest,
//                NetRevenueInterest = netRevenueInterest
//            };

//            participatingArea.Tracts.Add(tractParticipationObj);

//            // Validate total participation
//            if (participatingArea.TotalParticipation > 100.01m)
//                throw new InvalidOperationException($"Total participation exceeds 100%: {participatingArea.TotalParticipation}%");
//        }

//        /// <summary>
//        /// Creates a unit operating agreement.
//        /// </summary>
//        public UnitOperatingAgreement CreateUnitOperatingAgreement(string unitId)
//        {
//            if (string.IsNullOrEmpty(unitId))
//                throw new ArgumentException("Unit ID cannot be null or empty.", nameof(unitId));

//            if (!unitAgreements.ContainsKey(unitId))
//                throw new ArgumentException($"Unit agreement {unitId} not found.", nameof(unitId));

//            var operatingAgreement = new UnitOperatingAgreement
//            {
//                OperatingAgreementId = Guid.NewGuid().ToString(),
//                UnitId = unitId
//            };

//            operatingAgreements[operatingAgreement.OperatingAgreementId] = operatingAgreement;
//            return operatingAgreement;
//        }

//        /// <summary>
//        /// Adds a participant to a unit operating agreement.
//        /// </summary>
//        public void AddParticipant(
//            string operatingAgreementId,
//            string companyName,
//            decimal workingInterest,
//            decimal netRevenueInterest,
//            bool isOperator = false)
//        {
//            if (!operatingAgreements.TryGetValue(operatingAgreementId, out var operatingAgreement))
//                throw new ArgumentException($"Operating agreement {operatingAgreementId} not found.", nameof(operatingAgreementId));

//            if (workingInterest < 0 || workingInterest > 1)
//                throw new ArgumentException("Working interest must be between 0 and 1.", nameof(workingInterest));

//            var participant = new UnitParticipant
//            {
//                ParticipantId = Guid.NewGuid().ToString(),
//                CompanyName = companyName,
//                WorkingInterest = workingInterest,
//                NetRevenueInterest = netRevenueInterest,
//                IsOperator = isOperator,
//                VotingPercentage = workingInterest * 100m // Default to working interest
//            };

//            operatingAgreement.Participants.Add(participant);

//            // Validate total working interest
//            decimal totalWorkingInterest = operatingAgreement.Participants.Sum(p => p.WorkingInterest);
//            if (Math.Abs(totalWorkingInterest - 1.0m) > 0.001m)
//                throw new InvalidOperationException($"Total working interest must equal 1.0, but is {totalWorkingInterest}");
//        }

//        /// <summary>
//        /// Calculates tract allocation for a unit.
//        /// </summary>
//        public Dictionary<string, decimal> CalculateTractAllocation(
//            string unitId,
//            decimal totalVolume)
//        {
//            var unitAgreement = GetUnitAgreement(unitId);
//            if (unitAgreement == null)
//                throw new ArgumentException($"Unit agreement {unitId} not found.", nameof(unitId));

//            var allocation = new Dictionary<string, decimal>();

//            if (unitAgreement.ParticipatingArea?.Tracts != null)
//            {
//                foreach (var tract in unitAgreement.ParticipatingArea.Tracts)
//                {
//                    decimal tractVolume = totalVolume * (tract.ParticipationPercentage / 100m);
//                    allocation[tract.TractId] = tractVolume;
//                }
//            }

//            return allocation;
//        }

//        /// <summary>
//        /// Gets all unit agreements.
//        /// </summary>
//        public IEnumerable<UnitAgreement> GetAllUnitAgreements()
//        {
//            return unitAgreements.Values;
//        }

//        /// <summary>
//        /// Gets active unit agreements as of a date.
//        /// </summary>
//        public IEnumerable<UnitAgreement> GetActiveUnitAgreements(DateTime asOfDate)
//        {
//            return unitAgreements.Values
//                .Where(u => u.EffectiveDate <= asOfDate &&
//                           (u.ExpirationDate == null || u.ExpirationDate >= asOfDate));
//        }
//    }
//}
