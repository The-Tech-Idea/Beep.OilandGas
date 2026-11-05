using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
#if BEEP_SECURE_CONNECTIONS
using TheTechIdea.Beep.DataBase;
#endif

namespace Beep.OilandGas.PPDM39.Core;

/// <summary>The configured Beep driver cannot expose the evidence required to verify transaction ownership.</summary>
public sealed class DataSourceTransactionCapabilityException(string message):InvalidOperationException(message);

/// <summary>The physical Beep schema cannot preserve the native entity contract.</summary>
public sealed class NativeSchemaCompatibilityException(string message):InvalidOperationException(message);

/// <summary>One owned Beep transaction on the same cached datasource used by the canonical repository.</summary>
public static class PPDMDataSourceTransaction
{
    private sealed record Command(IDataSource Source, IDbConnection Connection, IDbTransaction Transaction, PropertyInfo Evidence, object? NativeOwner = null)
    {
        public SemaphoreSlim Operations { get; } = new(1, 1);
        private int closing;
        public void Close() => Interlocked.Exchange(ref closing, 1);
        public void RequireOwned(bool finishing = false)
        {
            if (!finishing && Volatile.Read(ref closing) != 0 || !ReferenceEquals(Evidence.GetValue(Source), Transaction) || !ReferenceEquals(Transaction.Connection, Connection) ||
                Connection.State != ConnectionState.Open || !ReferenceEquals(ConnectionOf(Source), Connection))
                throw new InvalidOperationException("The Beep command lost its owned transaction or physical connection.");
#if BEEP_SECURE_CONNECTIONS
            if (NativeOwner is not null && (Source is not IIsolationOwnedTransactionDataSource provider || !provider.OwnsTransaction(NativeOwner)))
                throw new InvalidOperationException("The Beep command lost its explicit transaction owner.");
#endif
        }
    }
    private static readonly ConditionalWeakTable<IDataSource, SemaphoreSlim> Lanes = new();
    private static readonly AsyncLocal<Command?> Current = new();

    /// <summary>Cross-owner reads must precede or follow every native command, including borrowed transactions.</summary>
    public static void RequireIdle(IDataSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var evidence=source.GetType().GetProperty("ActiveTransaction",BindingFlags.Public|BindingFlags.Instance);
        if(evidence is not {CanRead:true} || evidence.GetIndexParameters().Length!=0 || !typeof(IDbTransaction).IsAssignableFrom(evidence.PropertyType))
            throw new DataSourceTransactionCapabilityException("The selected Beep driver cannot verify transaction ownership.");
        if(Current.Value is not null || evidence.GetValue(source) is not null)
            throw new InvalidOperationException("Cross-owner source review requires an idle Beep datasource.");
    }

    // Narrow command policies must prove the same owned transaction before authorizing a mutation.
    public static void RequireOwned(IDataSource source, IsolationLevel? requiredIsolation = null)
    {
        var command = Current.Value;
        if (command is null || !ReferenceEquals(command.Source, source))
            throw new InvalidOperationException("The mutation is outside its owned Beep command transaction.");
        command.RequireOwned();
        if (requiredIsolation is not null && command.Transaction.IsolationLevel != requiredIsolation)
            throw new InvalidOperationException("The Beep command transaction does not provide its required isolation level.");
    }

    // All active repository SDK reads/writes share this lane. Provider work inside a command retains its transaction.
    public static async Task<T> WithSourceAsync<T>(IDataSource source, Func<Task<T>> operation)
    {
        var command = Current.Value;
        var lane = command is null ? Lanes.GetValue(source, _ => new(1, 1)) : command.Operations;
        if (command is not null && !ReferenceEquals(source, command.Source))
            throw new InvalidOperationException("The Beep command datasource changed during repository access.");
        await lane.WaitAsync();
        try
        {
            if (command is null && source.GetType().GetProperty("ActiveTransaction", BindingFlags.Public | BindingFlags.Instance) is { CanRead: true } evidence &&
                evidence.GetIndexParameters().Length == 0 && evidence.GetValue(source) is IDbTransaction)
                throw new InvalidOperationException("A repository operation cannot join a borrowed or unconfirmed Beep transaction.");
            command?.RequireOwned();
            var result = await operation();
            if (source.ErrorObject?.Flag == Errors.Failed) throw new InvalidOperationException("The Beep provider reported a failed repository operation.");
            command?.RequireOwned();
            return result;
        }
        finally { lane.Release(); }
    }

    public static async Task ExecuteAsync(IDataSource source, Func<Task> operation)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(operation);
        if (Current.Value is not null) throw new InvalidOperationException("Nested Beep commands cannot borrow or commit another transaction.");
        var lane = Lanes.GetValue(source, _ => new(1, 1));
        await lane.WaitAsync(); Command? owned = null;
#if BEEP_SECURE_CONNECTIONS
        IIsolationOwnedTransactionDataSource? isolated = null; object? owner = null;
#endif
        try
        {
            // The installed driver must expose its actual held transaction. A no-op Ok result cannot establish ownership.
            var evidence = source.GetType().GetProperty("ActiveTransaction", BindingFlags.Public | BindingFlags.Instance);
            if (evidence is null || !evidence.CanRead || !typeof(IDbTransaction).IsAssignableFrom(evidence.PropertyType) || evidence.GetIndexParameters().Length != 0)
                throw new DataSourceTransactionCapabilityException("The selected Beep driver cannot verify transaction ownership.");
            var connection = ConnectionOf(source);
            if (connection.State != ConnectionState.Open || evidence.GetValue(source) is not null)
                throw new InvalidOperationException("Lease changes need an open Beep connection with no borrowed transaction.");
#if BEEP_SECURE_CONNECTIONS
            isolated = source as IIsolationOwnedTransactionDataSource;
            if (isolated is not null && !isolated.SupportsOwnedTransactions)
                throw new DataSourceTransactionCapabilityException("The selected Beep driver cannot own an isolated transaction.");
            if (isolated is not null) owner = new object();
            var begin = isolated is not null ? isolated.BeginOwnedTransaction(owner!, IsolationLevel.Serializable) : source.BeginTransaction(new PassedArgs());
#else
            var begin = source.BeginTransaction(new PassedArgs());
#endif
#if BEEP_SECURE_CONNECTIONS
            if (evidence.GetValue(source) is IDbTransaction transaction) owned = new(source, connection, transaction, evidence, owner);
#else
            if (evidence.GetValue(source) is IDbTransaction transaction) owned = new(source, connection, transaction, evidence);
#endif
            BeepOperationResult.EnsureSuccess(begin, "Begin transaction", source.DatasourceName);
            if (owned is null) throw new InvalidOperationException("The Beep provider did not open a verifiable transaction.");
            owned.RequireOwned(); Current.Value = owned;
            await operation();
            owned.Close(); await owned.Operations.WaitAsync();
            try {
                owned.RequireOwned(finishing: true);
#if BEEP_SECURE_CONNECTIONS
                BeepOperationResult.EnsureSuccess(isolated is not null ? isolated.CommitOwnedTransaction(owner!) : source.Commit(new PassedArgs()), "Commit transaction (outcome requires confirmation)", source.DatasourceName);
#else
                BeepOperationResult.EnsureSuccess(source.Commit(new PassedArgs()), "Commit transaction (outcome requires confirmation)", source.DatasourceName);
#endif
                if (evidence.GetValue(source) is not null) throw new InvalidOperationException("The Beep commit did not release its owned transaction; its outcome is unconfirmed.");
            }
            finally { owned.Operations.Release(); }
        }
        catch (Exception failure)
        {
            if (owned is not null && ReferenceEquals(owned.Evidence.GetValue(source), owned.Transaction))
            {
                try
                {
                    owned.Close(); await owned.Operations.WaitAsync();
                    try {
                        owned.RequireOwned(finishing: true);
#if BEEP_SECURE_CONNECTIONS
                        BeepOperationResult.EnsureSuccess(isolated is not null ? isolated.RollbackOwnedTransaction(owner!) : source.EndTransaction(new PassedArgs()), "Rollback transaction", source.DatasourceName);
#else
                        BeepOperationResult.EnsureSuccess(source.EndTransaction(new PassedArgs()), "Rollback transaction", source.DatasourceName);
#endif
                        if (owned.Evidence.GetValue(source) is not null) throw new InvalidOperationException("The Beep rollback did not release its transaction.");
                    }
                    finally { owned.Operations.Release(); }
                }
                catch (Exception rollback) { throw new AggregateException("The lease command failed and rollback was not confirmed. Refresh before further changes.", failure, rollback); }
            }
            throw;
        }
        finally { owned?.Close(); Current.Value = null; lane.Release(); }
    }

    private static IDbConnection ConnectionOf(IDataSource source)
    {
        var connection = source.Dataconnection ?? throw new InvalidOperationException("The Beep command has no physical connection.");
        var property = connection.GetType().GetProperty("DbConn", BindingFlags.Public | BindingFlags.Instance);
        return property is { CanRead: true } && property.GetIndexParameters().Length == 0 && property.GetValue(connection) is IDbConnection physical
            ? physical : throw new InvalidOperationException("The Beep provider does not expose its command connection.");
    }
}
