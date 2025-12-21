using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PPDM39.Core.DTOs
{
    /// <summary>
    /// Standard role definitions and permissions for oil & gas companies
    /// </summary>
    public static class RoleDefinitions
    {
        #region Role Constants

        public const string PetroleumEngineer = "PetroleumEngineer";
        public const string ReservoirEngineer = "ReservoirEngineer";
        public const string ProcessEngineer = "ProcessEngineer";
        public const string Accountant = "Accountant";
        public const string Manager = "Manager";
        public const string Administrator = "Administrator";
        public const string Geologist = "Geologist";
        public const string DataManager = "DataManager";
        public const string Viewer = "Viewer";

        #endregion

        #region Permission Constants

        // Production permissions
        public const string ViewProduction = "ViewProduction";
        public const string EditProduction = "EditProduction";
        public const string DeleteProduction = "DeleteProduction";

        // Reserves permissions
        public const string ViewReserves = "ViewReserves";
        public const string EditReserves = "EditReserves";
        public const string DeleteReserves = "DeleteReserves";

        // Well permissions
        public const string ViewWells = "ViewWells";
        public const string EditWells = "EditWells";
        public const string DeleteWells = "DeleteWells";

        // Forecast permissions
        public const string ViewForecasts = "ViewForecasts";
        public const string RunDCA = "RunDCA";
        public const string RunForecast = "RunForecast";

        // Simulation permissions
        public const string ViewSimulation = "ViewSimulation";
        public const string RunSimulation = "RunSimulation";

        // Accounting permissions
        public const string ViewAccounting = "ViewAccounting";
        public const string EditAccounting = "EditAccounting";
        public const string ViewCostAllocation = "ViewCostAllocation";
        public const string EditCostAllocation = "EditCostAllocation";

        // Facilities permissions
        public const string ViewFacilities = "ViewFacilities";
        public const string EditFacilities = "EditFacilities";

        // Field permissions
        public const string ViewFields = "ViewFields";
        public const string EditFields = "EditFields";

        // Exploration permissions
        public const string ViewExploration = "ViewExploration";
        public const string EditExploration = "EditExploration";

        // Seismic permissions
        public const string ViewSeismic = "ViewSeismic";
        public const string EditSeismic = "EditSeismic";

        // Access control permissions
        public const string ManageUsers = "ManageUsers";
        public const string ManageRoles = "ManageRoles";
        public const string ManageAssetAccess = "ManageAssetAccess";

        // Administration permissions
        public const string ManageSystem = "ManageSystem";
        public const string ManageOrganization = "ManageOrganization";

        #endregion

        #region Role Permission Mappings

        /// <summary>
        /// Gets the permissions for a given role
        /// </summary>
        public static List<string> GetPermissionsForRole(string roleId)
        {
            return roleId switch
            {
                PetroleumEngineer => new List<string>
                {
                    ViewProduction, EditProduction,
                    ViewWells, EditWells,
                    ViewForecasts, RunDCA, RunForecast,
                    ViewFacilities,
                    ViewFields
                },
                ReservoirEngineer => new List<string>
                {
                    ViewReserves, EditReserves,
                    ViewWells,
                    ViewForecasts, RunForecast,
                    ViewSimulation, RunSimulation,
                    ViewFields
                },
                ProcessEngineer => new List<string>
                {
                    ViewProduction,
                    ViewFacilities, EditFacilities,
                    ViewWells,
                    ViewFields
                },
                Accountant => new List<string>
                {
                    ViewAccounting, EditAccounting,
                    ViewCostAllocation, EditCostAllocation,
                    ViewProduction,
                    ViewFields
                },
                Manager => new List<string>
                {
                    ViewProduction, ViewReserves, ViewWells, ViewForecasts, ViewSimulation,
                    ViewAccounting, ViewCostAllocation, ViewFacilities, ViewFields,
                    ViewExploration, ViewSeismic
                },
                Administrator => new List<string>
                {
                    // Administrators have all permissions
                    ViewProduction, EditProduction, DeleteProduction,
                    ViewReserves, EditReserves, DeleteReserves,
                    ViewWells, EditWells, DeleteWells,
                    ViewForecasts, RunDCA, RunForecast,
                    ViewSimulation, RunSimulation,
                    ViewAccounting, EditAccounting,
                    ViewCostAllocation, EditCostAllocation,
                    ViewFacilities, EditFacilities,
                    ViewFields, EditFields,
                    ViewExploration, EditExploration,
                    ViewSeismic, EditSeismic,
                    ManageUsers, ManageRoles, ManageAssetAccess,
                    ManageSystem, ManageOrganization
                },
                Geologist => new List<string>
                {
                    ViewExploration, EditExploration,
                    ViewSeismic, EditSeismic,
                    ViewWells,
                    ViewFields
                },
                DataManager => new List<string>
                {
                    ViewProduction, EditProduction,
                    ViewReserves, EditReserves,
                    ViewWells, EditWells,
                    ViewFacilities, EditFacilities,
                    ViewFields, EditFields,
                    ViewExploration, EditExploration,
                    ViewSeismic, EditSeismic
                },
                Viewer => new List<string>
                {
                    // View-only permissions
                    ViewProduction, ViewReserves, ViewWells, ViewForecasts,
                    ViewFacilities, ViewFields, ViewExploration, ViewSeismic
                },
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Gets all available roles
        /// </summary>
        public static List<string> GetAllRoles()
        {
            return new List<string>
            {
                PetroleumEngineer,
                ReservoirEngineer,
                ProcessEngineer,
                Accountant,
                Manager,
                Administrator,
                Geologist,
                DataManager,
                Viewer
            };
        }

        /// <summary>
        /// Gets all available permissions
        /// </summary>
        public static List<string> GetAllPermissions()
        {
            return new List<string>
            {
                // Production
                ViewProduction, EditProduction, DeleteProduction,
                // Reserves
                ViewReserves, EditReserves, DeleteReserves,
                // Wells
                ViewWells, EditWells, DeleteWells,
                // Forecasts
                ViewForecasts, RunDCA, RunForecast,
                // Simulation
                ViewSimulation, RunSimulation,
                // Accounting
                ViewAccounting, EditAccounting, ViewCostAllocation, EditCostAllocation,
                // Facilities
                ViewFacilities, EditFacilities,
                // Fields
                ViewFields, EditFields,
                // Exploration
                ViewExploration, EditExploration,
                // Seismic
                ViewSeismic, EditSeismic,
                // Access Control
                ManageUsers, ManageRoles, ManageAssetAccess,
                // Administration
                ManageSystem, ManageOrganization
            };
        }

        /// <summary>
        /// Checks if a role has a specific permission
        /// </summary>
        public static bool RoleHasPermission(string roleId, string permissionId)
        {
            var permissions = GetPermissionsForRole(roleId);
            return permissions.Contains(permissionId);
        }

        /// <summary>
        /// Gets the default layout for a role
        /// </summary>
        public static string GetDefaultLayoutForRole(string roleId)
        {
            return roleId switch
            {
                PetroleumEngineer => "PetroleumEngineerLayout",
                ReservoirEngineer => "ReservoirEngineerLayout",
                ProcessEngineer => "ProcessEngineerLayout",
                Accountant => "AccountantLayout",
                Manager => "ManagerLayout",
                Administrator => "ManagerLayout", // Administrators can use manager layout
                _ => "DefaultLayout"
            };
        }

        /// <summary>
        /// Gets a display name for a role
        /// </summary>
        public static string GetRoleDisplayName(string roleId)
        {
            return roleId switch
            {
                PetroleumEngineer => "Petroleum Engineer",
                ReservoirEngineer => "Reservoir Engineer",
                ProcessEngineer => "Process Engineer",
                Accountant => "Accountant",
                Manager => "Manager",
                Administrator => "Administrator",
                Geologist => "Geologist",
                DataManager => "Data Manager",
                Viewer => "Viewer",
                _ => roleId
            };
        }

        /// <summary>
        /// Gets a display name for a permission
        /// </summary>
        public static string GetPermissionDisplayName(string permissionId)
        {
            return permissionId.Replace("View", "View ")
                              .Replace("Edit", "Edit ")
                              .Replace("Delete", "Delete ")
                              .Replace("Run", "Run ")
                              .Replace("Manage", "Manage ");
        }

        #endregion
    }
}
