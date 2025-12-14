using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.LeaseAcquisition.Services
{
    /// <summary>
    /// Service for managing lease agreements.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public class LeaseManagementService : ILeaseManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;

        public LeaseManagementService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
        }

        private List<T> ConvertToList<T>(dynamic units) where T : class
        {
            var result = new List<T>();
            if (units == null) return result;
            
            if (units is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is T entity)
                    {
                        result.Add(entity);
                    }
                }
            }
            return result;
        }

        private IUnitOfWorkWrapper GetLandRightUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(LAND_RIGHT), _editor, _connectionName, "LAND_RIGHT", "LAND_RIGHT_ID");
        }

        private IUnitOfWorkWrapper GetLandAgreementUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(LAND_AGREEMENT), _editor, _connectionName, "LAND_AGREEMENT", "LAND_RIGHT_ID");
        }

        public async Task<List<LeaseDto>> GetLeasesAsync(string? fieldId = null)
        {
            var landRightUow = GetLandRightUnitOfWork();
            List<LAND_RIGHT> landRights;

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "AREA_ID", FilterValue = fieldId, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var units = await landRightUow.Get(filters);
                landRights = ConvertToList<LAND_RIGHT>(units);
            }
            else
            {
                var units = await landRightUow.Get();
                landRights = ConvertToList<LAND_RIGHT>(units).Where(lr => lr.ACTIVE_IND == "Y").ToList();
            }

            var leases = new List<LeaseDto>();
            var agreementUow = GetLandAgreementUnitOfWork();

            foreach (var landRight in landRights)
            {
                var agreementFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = landRight.LAND_RIGHT_ID, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var agreementUnits = await agreementUow.Get(agreementFilters);
                var agreements = ConvertToList<LAND_AGREEMENT>(agreementUnits);

                var lease = MapToLeaseDto(landRight, agreements.FirstOrDefault());
                leases.Add(lease);
            }

            return leases;
        }

        public async Task<LeaseDto?> GetLeaseAsync(string leaseId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                return null;

            var landRightUow = GetLandRightUnitOfWork();
            var landRight = landRightUow.Read(leaseId) as LAND_RIGHT;
            if (landRight == null || landRight.ACTIVE_IND != "Y")
                return null;

            var agreementUow = GetLandAgreementUnitOfWork();
            var agreementFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = leaseId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var agreementUnits = await agreementUow.Get(agreementFilters);
            var agreements = ConvertToList<LAND_AGREEMENT>(agreementUnits);

            return MapToLeaseDto(landRight, agreements.FirstOrDefault());
        }

        public async Task<LeaseDto> CreateLeaseAsync(CreateLeaseDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var landRightUow = GetLandRightUnitOfWork();
            var landRight = new LAND_RIGHT
            {
                LAND_RIGHT_ID = Guid.NewGuid().ToString(),
                LAND_RIGHT_SUBTYPE = "LEASE",
                ACTIVE_IND = "Y",
                ACQTN_DATE = createDto.LeaseDate ?? DateTime.UtcNow,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await landRightUow.InsertDoc(landRight);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create lease: {result.Message}");

            var agreementUow = GetLandAgreementUnitOfWork();
            var agreement = new LAND_AGREEMENT
            {
                LAND_RIGHT_ID = landRight.LAND_RIGHT_ID,
                LAND_AGREE_TYPE = "LEASE",
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.EffectiveDate ?? DateTime.UtcNow,
                EXPIRY_DATE = (DateTime)createDto.ExpirationDate,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var agreementResult = await agreementUow.InsertDoc(agreement);
            if (agreementResult.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create lease agreement: {agreementResult.Message}");

            await landRightUow.Commit();
            await agreementUow.Commit();

            return MapToLeaseDto(landRight, agreement);
        }

        public async Task<LeaseDto> UpdateLeaseAsync(string leaseId, UpdateLeaseDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentException("Lease ID cannot be null or empty.", nameof(leaseId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var landRightUow = GetLandRightUnitOfWork();
            var landRight = landRightUow.Read(leaseId) as LAND_RIGHT;
            if (landRight == null)
                throw new KeyNotFoundException($"Lease with ID {leaseId} not found.");

            var agreementUow = GetLandAgreementUnitOfWork();
            var agreementFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = leaseId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var agreementUnits = await agreementUow.Get(agreementFilters);
            var agreements = ConvertToList<LAND_AGREEMENT>(agreementUnits);
            var agreement = agreements.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
            {
                landRight.ACTIVE_IND = updateDto.Status == "Active" ? "Y" : "N";
                if (agreement != null)
                    agreement.ACTIVE_IND = updateDto.Status == "Active" ? "Y" : "N";
            }

            if (updateDto.ExpirationDate.HasValue && agreement != null)
            {
                agreement.EXPIRY_DATE = updateDto.ExpirationDate.Value;
            }

            landRight.ROW_CHANGED_DATE = DateTime.UtcNow;
            if (agreement != null)
                agreement.ROW_CHANGED_DATE = DateTime.UtcNow;

            var result = await landRightUow.UpdateDoc(landRight);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to update lease: {result.Message}");

            if (agreement != null)
            {
                var agreementResult = await agreementUow.UpdateDoc(agreement);
                if (agreementResult.Flag != Errors.Ok)
                    throw new InvalidOperationException($"Failed to update lease agreement: {agreementResult.Message}");
            }

            await landRightUow.Commit();
            if (agreement != null)
                await agreementUow.Commit();

            return MapToLeaseDto(landRight, agreement);
        }

        public async Task<LeaseDto> RenewLeaseAsync(string leaseId, DateTime newExpirationDate)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentException("Lease ID cannot be null or empty.", nameof(leaseId));

            var lease = await GetLeaseAsync(leaseId);
            if (lease == null)
                throw new KeyNotFoundException($"Lease with ID {leaseId} not found.");

            lease.ExpirationDate = newExpirationDate;
            
            return await UpdateLeaseAsync(leaseId, new UpdateLeaseDto
            {
                ExpirationDate = newExpirationDate
            });
        }

        public async Task<List<LeaseDto>> GetExpiringLeasesAsync(int days)
        {
            var allLeases = await GetLeasesAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(days);

            return allLeases
                .Where(l => l.ExpirationDate.HasValue && 
                           l.ExpirationDate.Value <= cutoffDate && 
                           l.ExpirationDate.Value > DateTime.UtcNow)
                .ToList();
        }

        public async Task<List<LandRightDto>> GetLandRightsAsync(string leaseId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                return new List<LandRightDto>();

            var landRightUow = GetLandRightUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = leaseId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await landRightUow.Get(filters);
            var landRights = ConvertToList<LAND_RIGHT>(units);

            return landRights.Select(lr => new LandRightDto
            {
                LandRightId = lr.LAND_RIGHT_ID ?? string.Empty,
                LeaseId = leaseId,
                RightType = lr.LAND_RIGHT_SUBTYPE ?? string.Empty,
                Description = lr.REMARK,
                EffectiveDate = lr.ACQTN_DATE,
                Status = lr.ACTIVE_IND == "Y" ? "Active" : "Inactive",
                Remarks = lr.REMARK
            }).ToList();
        }

        public async Task<List<MineralRightDto>> GetMineralRightsAsync(string leaseId)
        {
            // Note: OBLIGATION entity would need to be checked if it exists in PPDM39
            // For now, returning empty list as placeholder
            await Task.CompletedTask;
            return new List<MineralRightDto>();
        }

        public async Task<List<SurfaceAgreementDto>> GetSurfaceAgreementsAsync(string leaseId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                return new List<SurfaceAgreementDto>();

            var agreementUow = GetLandAgreementUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = leaseId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await agreementUow.Get(filters);
            var agreements = ConvertToList<LAND_AGREEMENT>(units);

            return agreements.Select(ag => new SurfaceAgreementDto
            {
                AgreementId = ag.LAND_RIGHT_ID ?? string.Empty,
                LeaseId = leaseId,
                AgreementType = ag.LAND_AGREE_TYPE ?? string.Empty,
                AgreementDate = ag.EFFECTIVE_DATE,
                EffectiveDate = ag.EFFECTIVE_DATE,
                ExpirationDate = ag.EXPIRY_DATE,
                Status = ag.ACTIVE_IND == "Y" ? "Active" : "Inactive",
                Remarks = ag.REMARK
            }).ToList();
        }

        public async Task<List<RoyaltyDto>> GetRoyaltiesAsync(string leaseId)
        {
            // Note: OBLIGATION entity would need to be checked if it exists in PPDM39
            // For now, returning empty list as placeholder
            await Task.CompletedTask;
            return new List<RoyaltyDto>();
        }

        private LeaseDto MapToLeaseDto(LAND_RIGHT landRight, LAND_AGREEMENT? agreement)
        {
            return new LeaseDto
            {
                LeaseId = landRight.LAND_RIGHT_ID ?? string.Empty,
                LeaseNumber = landRight.CASE_SERIAL_NUM ?? string.Empty,
                LeaseDate = landRight.ACQTN_DATE,
                EffectiveDate = agreement?.EFFECTIVE_DATE,
                ExpirationDate = agreement?.EXPIRY_DATE,
                Status = landRight.ACTIVE_IND == "Y" ? "Active" : "Inactive",
                Remarks = landRight.REMARK
            };
        }
    }
}

