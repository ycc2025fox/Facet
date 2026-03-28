[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [switch]$Push,
    [switch]$SkipPushPrompt
)

$ErrorActionPreference = "Stop"

$solutionRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $solutionRoot "Facet.sln"
$packagesDir = Join-Path $solutionRoot "packages"
$directoryPackagesProps = Join-Path $solutionRoot "Directory.Packages.props"
$pushSource = "https://nuget.myservices.site:9998/v3/index.json"
$pushApiKey = "7266fc29b15d75ec7b027adb89eb23f3a2baff66aa28738bdb873cbc20ed581c"

if (-not (Test-Path $solutionPath)) {
    throw "未找到解决方案文件: $solutionPath"
}

if (-not (Test-Path $directoryPackagesProps)) {
    throw "未找到 Directory.Packages.props: $directoryPackagesProps"
}

[xml]$buildProps = Get-Content -Path (Join-Path $solutionRoot "Directory.Build.props")
$version = $buildProps.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "unknown"
}

Write-Host "解决方案: $solutionPath"
Write-Host "集中依赖: $directoryPackagesProps"
Write-Host "统一版本: $version"
Write-Host "输出目录: $packagesDir"
Write-Host "构建配置: $Configuration"

if (Test-Path $packagesDir) {
    Remove-Item -LiteralPath $packagesDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packagesDir | Out-Null

$packableProjects = Get-ChildItem -Path (Join-Path $solutionRoot "src") -Recurse -Filter *.csproj |
    Where-Object {
        $content = Get-Content -Path $_.FullName -Raw
        $content -match "<IsPackable>\s*true\s*</IsPackable>"
    } |
    Sort-Object FullName

if (-not $packableProjects) {
    throw "未找到可打包项目。"
}

Write-Host ""
Write-Host "将打包以下项目:"
$packableProjects | ForEach-Object { Write-Host " - $($_.FullName)" }

Write-Host ""
Write-Host "开始还原..."
dotnet restore $solutionPath

Write-Host ""
Write-Host "开始构建..."
dotnet build $solutionPath -c $Configuration --no-restore

foreach ($project in $packableProjects) {
    Write-Host ""
    Write-Host "打包 $($project.Name)..."
    dotnet pack $project.FullName `
        -c $Configuration `
        --no-build `
        --include-symbols `
        -p:SymbolPackageFormat=snupkg `
        -o $packagesDir
}

$generatedPackages = Get-ChildItem -Path $packagesDir -File | Sort-Object Name
Write-Host ""
Write-Host "已生成包文件:"
$generatedPackages | ForEach-Object { Write-Host " - $($_.Name)" }

Write-Host ""
if ($Push) {
    & pushpackages --folder .\packages --api-key $pushApiKey --source $pushSource
}
elseif ($SkipPushPrompt) {
    Write-Host "已跳过推送。"
}
else {
    $answer = Read-Host "是否推送NuGet包? (y/n)"
    if ($answer -match '^(?i)y$') {
        & pushpackages --folder .\packages --api-key $pushApiKey --source $pushSource
    }
    else {
        Write-Host "已跳过推送。"
    }
}
