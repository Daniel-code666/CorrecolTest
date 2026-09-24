$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Push-Location $root
try {
    if (-not $env:TEST_SQLSERVER_CONNECTION) {
        $values = @{}
        Get-Content -LiteralPath .env | ForEach-Object {
            if ($_ -match '^([^#=]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
        }
        if (-not $values['MSSQL_SA_PASSWORD']) { throw 'Configure .env o TEST_SQLSERVER_CONNECTION.' }
        $port = if ($values['SQLSERVER_PORT']) { $values['SQLSERVER_PORT'] } else { '14333' }
        $env:TEST_SQLSERVER_CONNECTION = "Server=127.0.0.1,$port;Database=master;User Id=sa;Password=$($values['MSSQL_SA_PASSWORD']);Encrypt=true;TrustServerCertificate=true"
    }
    dotnet test CorrecolTest.slnx -c Release --logger trx --results-directory artifacts/TestResults
    if ($LASTEXITCODE -ne 0) { throw 'Las pruebas fallaron.' }
}
finally { Pop-Location }
