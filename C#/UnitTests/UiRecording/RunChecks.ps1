param(
    [string]$CompilerPath,
    [string]$MSBuildPath,
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug'
)
$ErrorActionPreference = 'Stop'
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'Visual Studio MSBuild is required.' }
if (-not $CompilerPath) { $CompilerPath = Join-Path (Split-Path $MSBuildPath) 'Roslyn\csc.exe' }
$output = Join-Path $PSScriptRoot 'bin'
[IO.Directory]::CreateDirectory($output) | Out-Null
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\JetEazy.WinForms.Recording'))
& $MSBuildPath (Join-Path $projectRoot 'JetEazy.WinForms.Recording.csproj') /t:Build "/p:Configuration=$Configuration" /p:Platform=AnyCPU /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Independent recording DLL build failed.' }
$library = Join-Path $projectRoot "bin\$Configuration\JetEazy.WinForms.Recording.dll"
Copy-Item -LiteralPath $library -Destination $output
$program = Join-Path $output 'UiRecordingChecks.exe'
& $CompilerPath /nologo /langversion:7.3 /target:exe "/out:$program" "/reference:$library" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.dll (Join-Path $PSScriptRoot 'UiRecordingChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'UI recording test compilation failed.' }
& $program
if ($LASTEXITCODE -ne 0) { throw 'UI recording checks failed.' }
