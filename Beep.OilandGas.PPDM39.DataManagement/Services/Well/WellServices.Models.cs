using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Partial class for nested classes and DTOs
    /// Contains data transfer objects and result classes used by WellServices
    /// </summary>
    public partial class WellServices
    {
        #region Nested Classes and DTOs

        /// <summary>
        /// Comprehensive information about a well status
        /// </summary>
        public class WellStatusInfo
        {
            public WELL_STATUS WellStatus { get; set; }
            public R_WELL_STATUS StatusDescription { get; set; }
            public Dictionary<string, object> Facets { get; set; }
            public string StatusType { get; set; }
            public string StatusName { get; set; }
        }

        #endregion

        #region Well Status Info Methods

        /// <summary>
        /// Gets well status with all related information (description, facets, etc.)
        /// </summary>
        public async Task<WellStatusInfo> GetWellStatusInfoAsync(WELL_STATUS wellStatus)
        {
            if (wellStatus == null)
                throw new System.ArgumentNullException(nameof(wellStatus));

            var statusDesc = await GetWellStatusDescriptionByStatusIdAsync(wellStatus.STATUS_ID);
            var facets = await GetWellStatusFacetsAsync(wellStatus.STATUS_ID);

            return new WellStatusInfo
            {
                WellStatus = wellStatus,
                StatusDescription = statusDesc,
                Facets = facets,
                StatusType = statusDesc?.STATUS_TYPE,
                StatusName = statusDesc?.LONG_NAME ?? statusDesc?.STATUS
            };
        }

        #endregion
    }
}
