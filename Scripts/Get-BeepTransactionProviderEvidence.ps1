param([Parameter(Mandatory = $true)][string[]]$AssemblyPaths)
$ErrorActionPreference = 'Stop'

# Inspect the actual binary metadata without loading a provider, opening a database, or claiming runtime conformance.
foreach ($path in $AssemblyPaths) {
    $resolved = (Resolve-Path -LiteralPath $path).Path
    $stream = [IO.File]::OpenRead($resolved)
    $reader = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        $metadata = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($reader)
        foreach ($handle in $metadata.TypeDefinitions) {
            $type = $metadata.GetTypeDefinition($handle)
            $name = $metadata.GetString($type.Name)
            if ($name -notmatch '^(RDBSource|SQLServerDataSource|SqlServerDataSource|PostgreDataSource)$') { continue }
            $properties = @()
            foreach ($propertyHandle in $type.GetProperties()) {
                $property = $metadata.GetPropertyDefinition($propertyHandle)
                $getter = $property.GetAccessors().Getter
                if (-not $getter.IsNil -and ($metadata.GetMethodDefinition($getter).Attributes -band [Reflection.MethodAttributes]::MemberAccessMask) -eq [Reflection.MethodAttributes]::Public) {
                    $properties += $metadata.GetString($property.Name)
                }
            }
            [pscustomobject]@{
                AssemblyPath = $resolved
                Sha256 = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash
                Type = $metadata.GetString($type.Namespace) + '.' + $name
                DeclaresPublicActiveTransaction = $properties -contains 'ActiveTransaction'
                EvidenceScope = 'Declared public properties only; inherited members and physical transaction/enlistment/rollback require runtime verification.'
            }
        }
    }
    finally { $reader.Dispose(); $stream.Dispose() }
}
