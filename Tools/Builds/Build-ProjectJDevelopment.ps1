param(
    [string]$UnityPath = "$env:ProgramFiles\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe",
    [string]$ProjectPath = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ProjectPath))
{
    $ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

if (-not (Test-Path $UnityPath))
{
    Write-Error "Unity 실행 파일을 찾을 수 없습니다: $UnityPath"
    exit 10
}

if (-not (Test-Path (Join-Path $ProjectPath "Assets")))
{
    Write-Error "올바른 Unity 프로젝트 경로가 아닙니다: $ProjectPath"
    exit 11
}

$buildProfilePath = "Assets/_ProjectJ/Settings/BuildProfiles/ProjectJ_Windows_Development.asset"
$absoluteBuildProfilePath = Join-Path $ProjectPath $buildProfilePath

if (-not (Test-Path $absoluteBuildProfilePath))
{
    Write-Error "개발 Build Profile 에셋을 찾을 수 없습니다: $absoluteBuildProfilePath"
    exit 12
}

$logDirectory = Join-Path $ProjectPath "Logs\Builds\Windows"
$buildLogPath = Join-Path $logDirectory "DevelopmentBuild.log"
$buildOutputPath = Join-Path $ProjectPath "Builds\Windows\Development\ProjectJ_Development.exe"

New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null

$quotedProjectPath = '"' + $ProjectPath + '"'
$quotedBuildProfilePath = '"' + $buildProfilePath + '"'
$quotedBuildLogPath = '"' + $buildLogPath + '"'

$arguments = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $quotedProjectPath,
    "-activeBuildProfile", $quotedBuildProfilePath,
    "-executeMethod", "ProjectJ.Editor.Day10DevelopmentBuildTool.BuildDevelopmentClientFromCommandLine",
    "-logFile", $quotedBuildLogPath
)

Write-Host "Project J Windows 개발 빌드 시작"
Write-Host "Unity: $UnityPath"
Write-Host "Project: $ProjectPath"
Write-Host "Build Profile: $buildProfilePath"
Write-Host "Build Log: $buildLogPath"

$process = Start-Process `
    -FilePath $UnityPath `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -NoNewWindow

Write-Host "Unity 종료 코드: $($process.ExitCode)"

if ($process.ExitCode -ne 0)
{
    Write-Error "Windows 개발 빌드에 실패했습니다. 로그를 확인합니다: $buildLogPath"
    exit $process.ExitCode
}

if (-not (Test-Path $buildOutputPath))
{
    Write-Error "Unity는 성공 코드를 반환했지만 실행 파일을 찾을 수 없습니다: $buildOutputPath"
    exit 13
}

Write-Host "Windows 개발 빌드 성공"
Write-Host "실행 파일: $buildOutputPath"
Write-Host "빌드 로그: $buildLogPath"
Write-Host "빌드 요약: $(Join-Path $ProjectPath 'Logs\Builds\Windows\DevelopmentBuildSummary.log')"
exit 0
