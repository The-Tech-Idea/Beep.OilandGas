using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Centralized service for converting between DTOs and PPDM model classes
    /// All conversions use strongly-typed classes - no Dictionary
    /// </summary>
    public class PPDMMappingService
    {
        /// <summary>
        /// Converts a DTO Request to a PPDM model class instance
        /// </summary>
        /// <typeparam name="TPPDM">PPDM model class type (e.g., PROSPECT, POOL)</typeparam>
        /// <typeparam name="TDTO">DTO Request type (e.g., ProspectRequest, PoolRequest)</typeparam>
        /// <param name="dto">DTO Request instance</param>
        /// <returns>PPDM model class instance with properties mapped from DTO</returns>
        public TPPDM ConvertDTOToPPDMModel<TPPDM, TDTO>(TDTO dto) 
            where TPPDM : class, new() 
            where TDTO : class
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var ppdmEntity = new TPPDM();
            var dtoType = typeof(TDTO);
            var ppdmType = typeof(TPPDM);
            var dtoProperties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var dtoProp in dtoProperties)
            {
                var value = dtoProp.GetValue(dto);
                if (value == null)
                    continue;

                // Try to find matching property in PPDM model
                // First try PPDM field name (e.g., PROSPECT_ID), then try property name (e.g., ProspectId)
                var ppdmPropName = ConvertPropertyNameToPPDM(dtoProp.Name); // ProspectId -> PROSPECT_ID
                var ppdmProp = ppdmType.GetProperty(ppdmPropName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                             ?? ppdmType.GetProperty(dtoProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (ppdmProp != null && ppdmProp.CanWrite)
                {
                    try
                    {
                        var convertedValue = ConvertValue(value, ppdmProp.PropertyType);
                        ppdmProp.SetValue(ppdmEntity, convertedValue);
                    }
                    catch
                    {
                        // Skip properties that can't be converted - this is expected for some property mismatches
                    }
                }
            }

            return ppdmEntity;
        }

        /// <summary>
        /// Converts a PPDM model class instance to a DTO Response
        /// </summary>
        /// <typeparam name="TDTO">DTO Response type (e.g., ProspectResponse, PoolResponse)</typeparam>
        /// <typeparam name="TPPDM">PPDM model class type (e.g., PROSPECT, POOL)</typeparam>
        /// <param name="ppdmEntity">PPDM model class instance</param>
        /// <returns>DTO Response instance with properties mapped from PPDM model</returns>
        public TDTO ConvertPPDMModelToDTO<TDTO, TPPDM>(TPPDM ppdmEntity) 
            where TDTO : class, new() 
            where TPPDM : class
        {
            if (ppdmEntity == null)
                return new TDTO();

            var dto = new TDTO();
            var ppdmType = typeof(TPPDM);
            var dtoType = typeof(TDTO);
            var dtoProperties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var dtoProp in dtoProperties)
            {
                // Try to find property in PPDM model by both PPDM field name and property name
                var ppdmPropName = ConvertPropertyNameToPPDM(dtoProp.Name);
                var ppdmProp = ppdmType.GetProperty(ppdmPropName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                             ?? ppdmType.GetProperty(dtoProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (ppdmProp != null && dtoProp.CanWrite)
                {
                    try
                    {
                        var value = ppdmProp.GetValue(ppdmEntity);
                        if (value != null && value != DBNull.Value)
                        {
                            var convertedValue = ConvertValue(value, dtoProp.PropertyType);
                            dtoProp.SetValue(dto, convertedValue);
                        }
                    }
                    catch
                    {
                        // Skip properties that can't be converted
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Converts a list of PPDM model classes to a list of DTO Responses
        /// </summary>
        /// <typeparam name="TDTO">DTO Response type</typeparam>
        /// <typeparam name="TPPDM">PPDM model class type</typeparam>
        /// <param name="ppdmEntities">List of PPDM model instances</param>
        /// <returns>List of DTO Response instances</returns>
        public List<TDTO> ConvertPPDMModelListToDTOList<TDTO, TPPDM>(IEnumerable<TPPDM> ppdmEntities) 
            where TDTO : class, new() 
            where TPPDM : class
        {
            if (ppdmEntities == null)
                return new List<TDTO>();

            return ppdmEntities
                .Select(ppdm => ConvertPPDMModelToDTO<TDTO, TPPDM>(ppdm))
                .ToList();
        }

        /// <summary>
        /// Converts a DTO Request to a PPDM model class instance using runtime types
        /// </summary>
        /// <param name="dto">DTO Request instance</param>
        /// <param name="dtoType">Type of the DTO</param>
        /// <param name="ppdmType">Type of the PPDM model</param>
        /// <returns>PPDM model class instance as object</returns>
        public object ConvertDTOToPPDMModelRuntime(object dto, Type dtoType, Type ppdmType)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (ppdmType == null)
                throw new ArgumentNullException(nameof(ppdmType));
            if (dtoType == null)
                throw new ArgumentNullException(nameof(dtoType));

            var method = typeof(PPDMMappingService).GetMethod(nameof(ConvertDTOToPPDMModel), BindingFlags.Public | BindingFlags.Instance);
            var genericMethod = method!.MakeGenericMethod(ppdmType, dtoType);
            return genericMethod.Invoke(this, new[] { dto })!;
        }

        /// <summary>
        /// Converts a PPDM model class instance to a DTO Response using runtime types
        /// </summary>
        /// <param name="ppdmEntity">PPDM model class instance</param>
        /// <param name="dtoType">Type of the DTO Response</param>
        /// <param name="ppdmType">Type of the PPDM model</param>
        /// <returns>DTO Response instance as object</returns>
        public object ConvertPPDMModelToDTORuntime(object ppdmEntity, Type dtoType, Type ppdmType)
        {
            if (ppdmEntity == null)
            {
                return Activator.CreateInstance(dtoType)!;
            }
            if (ppdmType == null)
                throw new ArgumentNullException(nameof(ppdmType));
            if (dtoType == null)
                throw new ArgumentNullException(nameof(dtoType));

            var method = typeof(PPDMMappingService).GetMethod(nameof(ConvertPPDMModelToDTO), BindingFlags.Public | BindingFlags.Instance);
            var genericMethod = method!.MakeGenericMethod(dtoType, ppdmType);
            return genericMethod.Invoke(this, new[] { ppdmEntity })!;
        }

        /// <summary>
        /// Converts a list of PPDM model classes to a list of DTO Responses using runtime types
        /// </summary>
        /// <param name="ppdmEntities">Enumerable of PPDM model instances (non-generic IEnumerable)</param>
        /// <param name="dtoType">Type of the DTO Response</param>
        /// <param name="ppdmType">Type of the PPDM model</param>
        /// <returns>List of DTO Response instances</returns>
        public List<object> ConvertPPDMModelListToDTOListRuntime(IEnumerable ppdmEntities, Type dtoType, Type ppdmType)
        {
            if (ppdmEntities == null)
                return new List<object>();

            // Convert IEnumerable to List<TPPDM> using reflection
            var toListMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "ToList" && m.GetParameters().Length == 1)
                .MakeGenericMethod(ppdmType);
            var typedList = toListMethod.Invoke(null, new object[] { ppdmEntities });

            // Now invoke the generic ConvertPPDMModelListToDTOList method
            var method = typeof(PPDMMappingService).GetMethod(nameof(ConvertPPDMModelListToDTOList), BindingFlags.Public | BindingFlags.Instance);
            var genericMethod = method!.MakeGenericMethod(dtoType, ppdmType);
            var result = genericMethod.Invoke(this, new[] { typedList });
            
            // Convert result to List<object>
            var resultList = (System.Collections.IList)result!;
            return resultList.Cast<object>().ToList();
        }

        /// <summary>
        /// Converts a property name from C# PascalCase to PPDM UPPER_SNAKE_CASE
        /// Examples: ProspectId -> PROSPECT_ID, TotalDepthOuom -> TOTAL_DEPTH_OUOM
        /// </summary>
        private string ConvertPropertyNameToPPDM(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            // Handle special cases
            if (propertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            {
                var baseName = propertyName.Substring(0, propertyName.Length - 2);
                return $"{baseName.ToUpper()}_ID";
            }

            if (propertyName.EndsWith("Ouom", StringComparison.OrdinalIgnoreCase))
            {
                var baseName = propertyName.Substring(0, propertyName.Length - 4);
                return $"{baseName.ToUpper()}_OUOM";
            }

            // Convert PascalCase to UPPER_SNAKE_CASE
            var result = string.Empty;
            for (int i = 0; i < propertyName.Length; i++)
            {
                if (i > 0 && char.IsUpper(propertyName[i]))
                {
                    result += "_";
                }
                result += char.ToUpper(propertyName[i]);
            }

            return result;
        }

        /// <summary>
        /// Converts a value to the target type with null handling for nullable types
        /// </summary>
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
            {
                return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null
                    ? Activator.CreateInstance(targetType)!
                    : null!;
            }

            // If types match or value is assignable, return as-is
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle DateTime conversions
            if (underlyingType == typeof(DateTime) && value is string dateString)
            {
                if (DateTime.TryParse(dateString, out var dateValue))
                    return dateValue;
            }

            // Handle decimal conversions (common in PPDM - many numeric fields are decimal)
            if (underlyingType == typeof(decimal))
            {
                if (decimal.TryParse(value.ToString(), out var decValue))
                    return decValue;
            }

            // Handle numeric conversions
            if (underlyingType == typeof(int) || underlyingType == typeof(int?))
            {
                if (int.TryParse(value.ToString(), out var intValue))
                    return intValue;
            }

            if (underlyingType == typeof(double) || underlyingType == typeof(double?))
            {
                if (double.TryParse(value.ToString(), out var doubleValue))
                    return doubleValue;
            }

            // Default: try Convert.ChangeType
            try
            {
                return Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                // If conversion fails, return original value (may cause issues, but preserves data)
                return value;
            }
        }
    }
}
