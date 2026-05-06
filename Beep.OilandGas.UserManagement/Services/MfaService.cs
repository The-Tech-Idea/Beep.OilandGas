using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Multi-Factor Authentication service implementing TOTP (RFC 6238).
    /// Provides QR code setup, backup codes, and verification.
    /// </summary>
    public class MfaService : IMfaService
    {
        private const int BackupCodeCount = 10;
        private const int BackupCodeLength = 8;
        private const int TotpDigits = 6;
        private const int TotpPeriod = 30;

        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<MfaService> _logger;
        private readonly IUserService _userService;

        public MfaService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            ILogger<MfaService> logger,
            IUserService userService)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
            _userService = userService;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName)
        {
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), _connectionName, tableName, null);
        }

        public async Task<MfaSetupResult> EnableMfaAsync(string userId)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return new MfaSetupResult { Success = false, ErrorMessage = "User not found." };
                }

                var secretKey = GenerateSecretKey();
                var qrCodeUri = GenerateQrCodeUri(user.USER_NAME ?? user.USER_ID, secretKey);

                var mfaConfig = new UserMfaConfig
                {
                    USER_MFA_ID = Guid.NewGuid().ToString(),
                    USER_ID = userId,
                    MFA_SECRET_KEY = secretKey,
                    MFA_METHOD = "TOTP",
                    MFA_ENABLED_IND = "Y",
                    MFA_VERIFIED_IND = "N",
                    REMAINING_BACKUP_CODES = 0,
                    MFA_ENABLED_DATE_UTC = DateTime.UtcNow,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CHANGED_BY = userId,
                    ROW_CHANGED_DATE = DateTime.UtcNow,
                    ROW_EFFECTIVE_DATE = DateTime.UtcNow
                };

                var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
                await repo.InsertAsync(mfaConfig, userId);

                return new MfaSetupResult
                {
                    Success = true,
                    SecretKey = secretKey,
                    QrCodeUri = qrCodeUri
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enable MFA for user {UserId}", userId);
                return new MfaSetupResult { Success = false, ErrorMessage = "Failed to enable MFA." };
            }
        }

        public async Task<bool> VerifyMfaSetupAsync(string userId, string totpCode)
        {
            try
            {
                var mfaConfig = await GetMfaConfigAsync(userId);
                if (mfaConfig == null || string.IsNullOrEmpty(mfaConfig.MFA_SECRET_KEY))
                {
                    return false;
                }

                var isValid = VerifyTotpCode(mfaConfig.MFA_SECRET_KEY, totpCode);
                if (!isValid) return false;

                mfaConfig.MFA_VERIFIED_IND = "Y";
                mfaConfig.BACKUP_CODES_JSON = GenerateBackupCodesJson(BackupCodeCount);
                mfaConfig.REMAINING_BACKUP_CODES = BackupCodeCount;
                mfaConfig.ROW_CHANGED_BY = userId;
                mfaConfig.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
                await repo.UpdateAsync(mfaConfig, userId);

                _logger.LogInformation("MFA setup verified for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify MFA setup for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DisableMfaAsync(string userId, string currentPassword)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null) return false;

                var passwordValid = await _userService.CheckPasswordAsync(user, currentPassword);
                if (!passwordValid) return false;

                var mfaConfig = await GetMfaConfigAsync(userId);
                if (mfaConfig == null) return true;

                var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
                await repo.SoftDeleteAsync(mfaConfig.USER_MFA_ID, userId);

                _logger.LogInformation("MFA disabled for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disable MFA for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> VerifyTotpAsync(string userId, string totpCode)
        {
            try
            {
                var mfaConfig = await GetMfaConfigAsync(userId);
                if (mfaConfig == null || !mfaConfig.MFA_ENABLED_IND.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(mfaConfig.MFA_SECRET_KEY)) return false;

                var isValid = VerifyTotpCode(mfaConfig.MFA_SECRET_KEY, totpCode);
                if (isValid)
                {
                    mfaConfig.LAST_MFA_USED_UTC = DateTime.UtcNow;
                    mfaConfig.ROW_CHANGED_BY = userId;
                    mfaConfig.ROW_CHANGED_DATE = DateTime.UtcNow;
                    var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
                    await repo.UpdateAsync(mfaConfig, userId);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify TOTP for user {UserId}", userId);
                return false;
            }
        }

        public async Task<BackupCodesResult> GenerateBackupCodesAsync(string userId)
        {
            try
            {
                var mfaConfig = await GetMfaConfigAsync(userId);
                if (mfaConfig == null)
                {
                    return new BackupCodesResult { Success = false, ErrorMessage = "MFA not configured." };
                }

                var codes = GenerateBackupCodes(BackupCodeCount);
                mfaConfig.BACKUP_CODES_JSON = string.Join(",", codes);
                mfaConfig.REMAINING_BACKUP_CODES = codes.Length;
                mfaConfig.ROW_CHANGED_BY = userId;
                mfaConfig.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
                await repo.UpdateAsync(mfaConfig, userId);

                return new BackupCodesResult { Success = true, BackupCodes = codes };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate backup codes for user {UserId}", userId);
                return new BackupCodesResult { Success = false, ErrorMessage = "Failed to generate backup codes." };
            }
        }

        public async Task<bool> UseBackupCodeAsync(string userId, string backupCode)
        {
            try
            {
                var mfaConfig = await GetMfaConfigAsync(userId);
                if (mfaConfig == null || string.IsNullOrEmpty(mfaConfig.BACKUP_CODES_JSON))
                {
                    return false;
                }

                var codes = mfaConfig.BACKUP_CODES_JSON.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                var normalizedCode = backupCode.Replace("-", "").ToUpperInvariant();

                var matchIndex = codes.FindIndex(c => c.Replace("-", "").ToUpperInvariant() == normalizedCode);
                if (matchIndex < 0) return false;

                codes.RemoveAt(matchIndex);
                mfaConfig.BACKUP_CODES_JSON = string.Join(",", codes);
                mfaConfig.REMAINING_BACKUP_CODES = codes.Count;
                mfaConfig.LAST_MFA_USED_UTC = DateTime.UtcNow;
                mfaConfig.ROW_CHANGED_BY = userId;
                mfaConfig.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
                await repo.UpdateAsync(mfaConfig, userId);

                _logger.LogInformation("Backup code used for user {UserId}, remaining: {Count}", userId, codes.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to use backup code for user {UserId}", userId);
                return false;
            }
        }

        public async Task<MfaStatusResult> GetMfaStatusAsync(string userId)
        {
            var mfaConfig = await GetMfaConfigAsync(userId);
            if (mfaConfig == null)
            {
                return new MfaStatusResult
                {
                    IsEnabled = false,
                    IsVerified = false,
                    RemainingBackupCodes = 0
                };
            }

            return new MfaStatusResult
            {
                IsEnabled = mfaConfig.MFA_ENABLED_IND.Equals("Y", StringComparison.OrdinalIgnoreCase),
                IsVerified = mfaConfig.MFA_VERIFIED_IND.Equals("Y", StringComparison.OrdinalIgnoreCase),
                RemainingBackupCodes = mfaConfig.REMAINING_BACKUP_CODES,
                LastUsedUtc = mfaConfig.LAST_MFA_USED_UTC,
                Method = mfaConfig.MFA_METHOD
            };
        }

        private async Task<UserMfaConfig?> GetMfaConfigAsync(string userId)
        {
            var repo = GetRepo<UserMfaConfig>("USER_MFA_CONFIG");
            var configs = (await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            })).Cast<UserMfaConfig>().ToList();

            return configs.FirstOrDefault();
        }

        private static string GenerateSecretKey()
        {
            var bytes = RandomNumberGenerator.GetBytes(20);
            return Base32Encode(bytes);
        }

        private static string GenerateQrCodeUri(string username, string secretKey)
        {
            var encodedSecret = Uri.EscapeDataString(secretKey);
            var encodedIssuer = Uri.EscapeDataString("Beep.OilandGas");
            var encodedAccount = Uri.EscapeDataString(username);
            return $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={encodedSecret}&issuer={encodedIssuer}&digits={TotpDigits}&period={TotpPeriod}";
        }

        private static bool VerifyTotpCode(string secretKey, string code)
        {
            var key = Base32Decode(secretKey);
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = epoch / TotpPeriod;

            for (int i = -1; i <= 1; i++)
            {
                var currentTimeStep = timeStep + i;
                var hash = ComputeHmacSha1(key, currentTimeStep);
                var computedCode = GenerateCodeFromHash(hash);

                if (computedCode == code.PadLeft(TotpDigits, '0'))
                {
                    return true;
                }
            }

            return false;
        }

        private static byte[] ComputeHmacSha1(byte[] key, long timeStep)
        {
            var timeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timeStep));
            using var hmac = new HMACSHA1(key);
            return hmac.ComputeHash(timeBytes);
        }

        private static string GenerateCodeFromHash(byte[] hash)
        {
            var offset = hash[hash.Length - 1] & 0x0F;
            var binary = ((hash[offset] & 0x7F) << 24)
                       | ((hash[offset + 1] & 0xFF) << 16)
                       | ((hash[offset + 2] & 0xFF) << 8)
                       | (hash[offset + 3] & 0xFF);

            var otp = binary % (int)Math.Pow(10, TotpDigits);
            return otp.ToString().PadLeft(TotpDigits, '0');
        }

        private static string[] GenerateBackupCodes(int count)
        {
            var codes = new string[count];
            for (int i = 0; i < count; i++)
            {
                var bytes = RandomNumberGenerator.GetBytes(BackupCodeLength / 2);
                codes[i] = BitConverter.ToString(bytes).Replace("-", "").Substring(0, BackupCodeLength);
            }
            return codes;
        }

        private static string GenerateBackupCodesJson(int count)
        {
            var codes = GenerateBackupCodes(count);
            return string.Join(",", codes);
        }

        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;

                while (bitsLeft >= 5)
                {
                    bitsLeft -= 5;
                    result.Append(alphabet[(buffer >> bitsLeft) & 0x1F]);
                }
            }

            if (bitsLeft > 0)
            {
                result.Append(alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
            }

            return result.ToString();
        }

        private static byte[] Base32Decode(string base32)
        {
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bytes = new List<byte>();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (var c in base32.ToUpperInvariant().TrimEnd('='))
            {
                var index = alphabet.IndexOf(c);
                if (index < 0) continue;

                buffer = (buffer << 5) | index;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    bitsLeft -= 8;
                    bytes.Add((byte)((buffer >> bitsLeft) & 0xFF));
                }
            }

            return bytes.ToArray();
        }
    }
}
