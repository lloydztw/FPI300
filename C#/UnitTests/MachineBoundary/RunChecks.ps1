param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [string]$MSBuildPath
)
$ErrorActionPreference = 'Stop'
$sourceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'Visual Studio MSBuild is required.' }
$project = Join-Path $sourceRoot 'Traveller.FPI30.Machine\Traveller.FPI30.Machine.csproj'
$arguments = @($project, '/t:Build', "/p:Configuration=$Configuration", '/p:Platform=AnyCPU', '/nologo', '/v:minimal')
& $MSBuildPath @arguments
if ($LASTEXITCODE -ne 0) { throw 'Independent Machine build failed.' }

$output = Join-Path $sourceRoot "Traveller.FPI30.Machine\bin\$Configuration"
$compiler = Join-Path (Split-Path $MSBuildPath) 'Roslyn\csc.exe'
$program = Join-Path $output 'MachineBoundaryChecks.exe'
& $compiler /nologo /langversion:7.3 /target:exe "/out:$program" /reference:System.Drawing.dll "/reference:$(Join-Path $output 'Traveller.FPI30.Config.dll')" "/reference:$(Join-Path $output 'Traveller.FPI30.Machine.dll')" "/reference:$(Join-Path $output 'JetEazy.Basics.dll')" "/reference:$(Join-Path $output 'JetEazy.VG.Devices.dll')" (Join-Path $PSScriptRoot 'MachineBoundaryChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Machine boundary check compilation failed.' }
& $program
if ($LASTEXITCODE -ne 0) { throw 'Machine boundary checks failed.' }
