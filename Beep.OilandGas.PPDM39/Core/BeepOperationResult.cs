using System;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.PPDM39.Core
{
    internal static class BeepOperationResult
    {
        public static bool IsSuccess(IErrorsInfo result) =>
            result != null && result.Flag == Errors.Ok &&
            (result.Errors == null || result.Errors.Count == 0);

        public static string Describe(IErrorsInfo result)
        {
            if (result == null)
                return "The datasource returned no operation result.";

            var details = result.Errors == null ? string.Empty :
                string.Join("; ", result.Errors.Where(e => e != null).Select(e => e.Message));
            return $"{result.Flag}: {result.Message} {details}".Trim();
        }

        public static void EnsureSuccess(IErrorsInfo result, string operation, string tableName)
        {
            if (!IsSuccess(result))
                throw new InvalidOperationException($"{operation} failed for {tableName}: {Describe(result)}", result?.Ex);
        }
    }
}
