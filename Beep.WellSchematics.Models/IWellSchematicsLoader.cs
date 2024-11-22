

namespace Beep.WellSchematics
{
    public interface IWellSchematicsLoader
    {
        WellData Welldata { get; set; }
        List<WellData_Equip> GetboreholeEquipmentData(string ubhi);
        List<WellData_Casing> GetcasingData(string ubhi);
        List<WellData_Borehole> GetmultipleWellboresData(string ubhi);
        List<WellData_Perf> GetperforationData(string ubhi);
        List<WellData_Tubing> GettubeDataLists(string ubhi);
        List<WellData_Equip> GetTubeEquipmentData(string ubhi, int tubeindex);
        WellData GetWellData(string UWI);
     
    }
}