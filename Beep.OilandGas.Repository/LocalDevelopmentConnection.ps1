function Assert-LocalDevelopmentConnection {
    param([string] $Provider, [string] $ConnectionString)

    if ($Provider -ne 'SqlServer' -or [string]::IsNullOrWhiteSpace($ConnectionString)) {
        throw 'Development repository configuration must select SQL Server LocalDB.'
    }
    $builder = [System.Data.Common.DbConnectionStringBuilder]::new()
    # Use the CLR setter: PowerShell otherwise treats this builder as a dictionary.
    $builder.set_ConnectionString($ConnectionString)
    $serverKeys = @('Server', 'Data Source', 'Address', 'Addr', 'Network Address')
    $servers = @($serverKeys | Where-Object { $builder.ContainsKey($_) } | ForEach-Object { [string]$builder[$_] })
    if ($servers.Count -ne 1 -or $servers[0] -notmatch '^\(localdb\)\\[^\\;\s]+$') {
        throw 'Development repository must specify one unambiguous LocalDB server instance.'
    }
    $databaseKeys = @('Database', 'Initial Catalog')
    $databases = @($databaseKeys | Where-Object { $builder.ContainsKey($_) } | ForEach-Object { [string]$builder[$_] })
    if ($databases.Count -ne 1 -or [string]::IsNullOrWhiteSpace($databases[0])) {
        throw 'Development repository must specify one database name.'
    }
}
