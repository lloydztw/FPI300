param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Debug',
    [string]$MSBuildPath
)
$ErrorActionPreference = 'Stop'
if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw 'MSBuild was not found.' }
$projectDirectory = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\Traveller.FPI30.Model'))
& $MSBuildPath (Join-Path $projectDirectory 'Traveller.FPI30.Model.csproj') /t:Build "/p:Configuration=$Configuration" /p:Platform=AnyCPU /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Standalone model build failed.' }
$output = Join-Path $PSScriptRoot "bin\$Configuration"
[IO.Directory]::CreateDirectory($output) | Out-Null
Get-ChildItem (Join-Path $projectDirectory "bin\$Configuration") -Filter '*.dll' | Copy-Item -Destination $output
$nativeDirectory = Join-Path $projectDirectory "bin\$Configuration\dll"
if (Test-Path -LiteralPath $nativeDirectory) {
    Copy-Item -LiteralPath $nativeDirectory -Destination $output -Recurse -Force
}
$references = @('Traveller.FPI30.Model', 'Traveller.FPI30.Config', 'JetEazy.QUtility', 'JetEazy.QxCollections', 'JetEazy.OpenCv3', 'JetEazy.Transform', 'EzAoiEmptyTrayInspector.Lib', 'MVDCore.Net', 'MVDImage.Net', 'MVDShape.Net', 'MVDBlobFind.Net', 'OpenCvSharp', 'LeTian.JxProps') | ForEach-Object { '/reference:' + (Join-Path $output ($_ + '.dll')) }
$compiler = Join-Path (Split-Path $MSBuildPath) 'Roslyn\csc.exe'
$exe = Join-Path $output 'ModelBoundaryChecks.exe'
& $compiler /nologo /langversion:7.3 /target:exe /platform:x64 "/out:$exe" /reference:System.Drawing.dll /reference:System.Windows.Forms.dll $references (Join-Path $PSScriptRoot 'ModelBoundaryChecks.cs') (Join-Path $PSScriptRoot 'AoiBoundaryChecks.cs') (Join-Path $PSScriptRoot 'SystemModelChecks.cs') (Join-Path $PSScriptRoot 'DataModelChecks.cs') (Join-Path $PSScriptRoot 'FlyRunnerChecks.cs') (Join-Path $PSScriptRoot '..\..\LaserAlignDX\App\AppCoordinateWriter.cs') (Join-Path $PSScriptRoot '..\..\LaserAlignDX\App\AppAoiServices.cs') (Join-Path $PSScriptRoot '..\..\LaserAlignDX\App\AppModelBindings.cs')
if ($LASTEXITCODE -ne 0) { throw 'Model boundary test compilation failed.' }
& $exe
if ($LASTEXITCODE -ne 0) { throw 'Model boundary checks failed.' }
