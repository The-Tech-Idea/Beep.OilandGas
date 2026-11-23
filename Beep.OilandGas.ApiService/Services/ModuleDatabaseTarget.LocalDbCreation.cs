using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;

namespace Beep.OilandGas.ApiService.Services;

public static partial class ModuleDatabaseTarget
{
    /// <summary>Binds the existing LocalDB native capability; no application SQL or connection registry write.</summary>
    public static DatabaseTargetMigration BindLocalDbCreation(
        LocalDbDatabaseTargetRequest request, IDMEEditor editor, RepositoryDbContext repository, IConfiguration configuration)
    {
        if (!BeepConnectionName.IsValid(request.ConnectionName) || string.IsNullOrWhiteSpace(request.Server) ||
            request.Server.Length > 256 || request.Server != request.Server.Trim() ||
            string.IsNullOrWhiteSpace(request.Database) || request.Database != request.Database.Trim())
            throw RefusalException.Invalid("Enter a valid connection name, explicit LocalDB server and database name.");
        if (request.Database.Equals("identityserver", StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Invalid("The permanent identity server database cannot be a new module target.");

        if (repository.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
            RequireSeparateDatabase(repository.Database.GetConnectionString(), request.Database);
        if (configuration["Diagnostics:Provider"]?.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
            RequireSeparateDatabase(configuration["Diagnostics:ConnectionString"], request.Database);

        var connection = new SqlConnectionStringBuilder
        {
            DataSource = request.Server, InitialCatalog = request.Database,
            IntegratedSecurity = true, TrustServerCertificate = true
        };
        SQLServerDatabaseTargetMigrationProvider provider;
        try
        {
            provider = new(SqlClientFactory.Instance, connection.ConnectionString, "Beep.OilandGas.Modules");
        }
        catch (ArgumentException invalid)
        {
            throw new RefusalException(RefusalKind.Invalid,
                "Use an integrated-security LocalDB user database name with letters, digits and underscores, starting with a letter (maximum 80 characters).", invalid);
        }

        var root = editor.ConfigEditor?.ExePath;
        if (string.IsNullOrWhiteSpace(root) || !Path.IsPathFullyQualified(root))
            throw RefusalException.Conflict("Configure an absolute Beep storage root before creating database targets.");
        var identity = JsonSerializer.Serialize(new
        {
            provider.Identity.Provider,
            Server = provider.Identity.Server.ToUpperInvariant(),
            Database = provider.Identity.Database.ToUpperInvariant(),
            provider.Identity.OwnershipScope
        });
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
        return MigrationManager.BindDatabaseTarget(provider, Path.Combine(root, "DatabaseTargets", key + ".json"));
    }

    private static void RequireSeparateDatabase(string? connectionString, string proposedDatabase)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;
        var protectedTarget = new SqlConnectionStringBuilder(connectionString);
        if (proposedDatabase.Equals(protectedTarget.InitialCatalog, StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Invalid("Choose a module database separate from the default administration and diagnostics repositories.");
    }
}
