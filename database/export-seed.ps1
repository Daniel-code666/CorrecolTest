$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
& docker compose cp database/sqlcmd.sh db:/tmp/sqlcmd.sh
if ($LASTEXITCODE -ne 0) { throw 'No se pudo preparar sqlcmd en el contenedor.' }
$destination = Join-Path $PSScriptRoot '../CorrecolTest.Infrastructure/Persistence/SeedData'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
$queries = @{
    'paises' = 'SELECT (SELECT PaisCodigo AS Codigo, PaisIso1 AS Iso1, PaisIso2 AS Iso2, PaisNombre AS Nombre, PaisCapital AS Capital FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) FROM dbo.Pais ORDER BY PaisCodigo'
    'departamentos' = 'SELECT (SELECT DptColCodigoDane AS Codigo, DptColNombredelDepartamento AS Nombre, DptColPaisCodigo AS PaisCodigo FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) FROM dbo.DepartamentosColombia ORDER BY DptColCodigoDane'
    'ciudades' = 'SELECT (SELECT DvsPltColCodigoDane AS Codigo, DvsPltColNombreMunicipio AS Nombre, DvsPltColDptColCodigoDane AS DepartamentoCodigo FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) FROM dbo.DivisionPoliticaColombia ORDER BY DvsPltColCodigoDane'
}
foreach ($entry in $queries.GetEnumerator()) {
    $lines = & docker compose exec -T db sh /tmp/sqlcmd.sh -d CorrecolTest -h -1 -y 4000 -Q "SET NOCOUNT ON; $($entry.Value)" -f 65001
    if ($LASTEXITCODE -ne 0) { throw "Export failed: $($entry.Key)" }
    $records = @($lines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { $_ | ConvertFrom-Json })
    $json = ConvertTo-Json -InputObject $records -Depth 5
    [IO.File]::WriteAllText((Join-Path $destination "$($entry.Key).json"), $json, [Text.UTF8Encoding]::new($false))
    Write-Output "$($entry.Key): $($records.Count) registros"
}
}
finally { Pop-Location }

