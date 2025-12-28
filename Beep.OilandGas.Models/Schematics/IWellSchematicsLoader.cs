using System.Collections.Generic;

namespace Beep.OilandGas.Models
{
    /// <summary>
    /// Interface for loading well schematics data.
    /// </summary>
    public interface IWellSchematicsLoader
    {
        /// <summary>
        /// Gets or sets the well data.
        /// </summary>
        WellData Welldata { get; set; }

        /// <summary>
        /// Gets borehole equipment data for a specific UBHI.
        /// </summary>
        /// <param name="ubhi">The UBHI (Unique Borehole Identifier).</param>
        /// <returns>List of equipment data.</returns>
        List<WellData_Equip> GetboreholeEquipmentData(string ubhi);

        /// <summary>
        /// Gets casing data for a specific UBHI.
        /// </summary>
        /// <param name="ubhi">The UBHI (Unique Borehole Identifier).</param>
        /// <returns>List of casing data.</returns>
        List<WellData_Casing> GetcasingData(string ubhi);

        /// <summary>
        /// Gets multiple wellbores data for a specific UBHI.
        /// </summary>
        /// <param name="ubhi">The UBHI (Unique Borehole Identifier).</param>
        /// <returns>List of borehole data.</returns>
        List<WellData_Borehole> GetmultipleWellboresData(string ubhi);

        /// <summary>
        /// Gets perforation data for a specific UBHI.
        /// </summary>
        /// <param name="ubhi">The UBHI (Unique Borehole Identifier).</param>
        /// <returns>List of perforation data.</returns>
        List<WellData_Perf> GetperforationData(string ubhi);

        /// <summary>
        /// Gets tubing data lists for a specific UBHI.
        /// </summary>
        /// <param name="ubhi">The UBHI (Unique Borehole Identifier).</param>
        /// <returns>List of tubing data.</returns>
        List<WellData_Tubing> GettubeDataLists(string ubhi);

        /// <summary>
        /// Gets tube equipment data for a specific UBHI and tube index.
        /// </summary>
        /// <param name="ubhi">The UBHI (Unique Borehole Identifier).</param>
        /// <param name="tubeindex">The tube index.</param>
        /// <returns>List of equipment data.</returns>
        List<WellData_Equip> GetTubeEquipmentData(string ubhi, int tubeindex);

        /// <summary>
        /// Gets well data for a specific UWI.
        /// </summary>
        /// <param name="UWI">The UWI (Unique Well Identifier).</param>
        /// <returns>The well data.</returns>
        WellData GetWellData(string UWI);
    }
}

