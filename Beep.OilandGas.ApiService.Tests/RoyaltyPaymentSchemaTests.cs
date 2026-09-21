using Microsoft.Data.Sqlite;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RoyaltyPaymentSchemaTests
{
    [Theory]
    [InlineData("ROYALTY_PAYMENT")]
    [InlineData("ROYALTY_CALCULATION")]
    public void SQLiteBootstrapRejectsDuplicateReservations(string table)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.Models", "Scripts"))) root = root.Parent;
        Assert.NotNull(root);
        var script = File.ReadAllText(Path.Combine(root!.FullName, "Beep.OilandGas.Models", "Scripts", "SQLite", "ProductionAccounting", table + "_TAB.sql"));
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();
        command.CommandText = $"INSERT INTO {table} ({table}_ID) VALUES ('reservation')";
        command.ExecuteNonQuery();
        var error = Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());
        Assert.Equal(19, error.SqliteErrorCode);
        if (table == "ROYALTY_PAYMENT")
        {
            command.CommandText = "SELECT ROYALTY_CALCULATION_ID, PAYMENT_REQUEST_ID, JOURNAL_ENTRY_ID FROM ROYALTY_PAYMENT";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
        }
    }
}
