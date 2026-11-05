using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Cryptography;
using System.Text;
namespace Beep.OilandGas.Models.Data.Process;
public static class ProcessDefinitionRevision
{
    public static bool IsValid(string? value)=>value is {Length:64} && value.All(char.IsAsciiHexDigit);
    public static string FromJson(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }
}
