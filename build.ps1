function Exec  
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$NULL -ne $env:APPVEYOR_BUILD_NUMBER];
$revision = "beta-{0:D4}" -f [convert]::ToInt32($revision, 10)

exec { & dotnet test .\src\GlobalExceptionHandler.Tests\GlobalExceptionHandler.Tests.csproj }

exec { & dotnet pack .\src\GlobalExceptionHandler -c Release -o .\artifacts --version-suffix=$revision }
