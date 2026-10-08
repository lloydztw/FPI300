param([string]$CompilerPath)
$ErrorActionPreference = 'Stop'
if (-not $CompilerPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $CompilerPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
}
if (-not $CompilerPath -or -not (Test-Path -LiteralPath $CompilerPath)) {
    throw 'Pass -CompilerPath with the path to the Visual Studio Roslyn csc.exe.'
}
$outputDirectory = Join-Path $PSScriptRoot 'bin'
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$program = Join-Path $outputDirectory 'LifecycleChecks.exe'
$appPath = Join-Path $PSScriptRoot '..\..\LaserAlignDX\App'
& $CompilerPath /nologo /langversion:7.3 /define:OPT_CALIB_V5 /target:exe "/out:$program" /reference:System.Windows.Forms.dll "/reference:System.Drawing.dll" "/main:Probe" (Join-Path $appPath 'AppBootstrapper.cs') (Join-Path $appPath 'AppResourceScope.cs') (Join-Path $PSScriptRoot 'LifecycleChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Lifecycle check compilation failed.' }
foreach ($scenario in @('scope', 'unused', 'normal', 'init-failure', 'system-failure', 'cleanup-failure')) {
    & $program $scenario
    if ($LASTEXITCODE -ne 0) { throw "Lifecycle scenario failed: $scenario" }
}

# Parse every compiled main-project source with the installed Roslyn compiler.
# This is syntax validation, not a replacement for a full vendor-SDK build.
$roslynDirectory = Split-Path $CompilerPath
$syntaxProgram = Join-Path $outputDirectory 'SyntaxChecks.exe'
$frameworkFacade = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades\netstandard.dll'
& $CompilerPath /nologo /langversion:7.3 /target:exe "/out:$syntaxProgram" /reference:System.Xml.Linq.dll "/reference:$frameworkFacade" "/reference:$(Join-Path $roslynDirectory 'Microsoft.CodeAnalysis.dll')" "/reference:$(Join-Path $roslynDirectory 'Microsoft.CodeAnalysis.CSharp.dll')" "/reference:$(Join-Path $roslynDirectory 'System.Collections.Immutable.dll')" (Join-Path $PSScriptRoot 'SyntaxChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Syntax checker compilation failed.' }
& $syntaxProgram $roslynDirectory (Join-Path $appPath '..\Traveller_FPI30_main.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Main project syntax validation failed.' }
