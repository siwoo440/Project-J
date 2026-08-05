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

$resultDirectory = Join-Path $ProjectPath "Library\ProjectJTestResults"
New-Item -ItemType Directory -Path $resultDirectory -Force | Out-Null

function Invoke-UnityTest
{
    param(
        [string]$TestPlatform,
        [string]$ResultFileName,
        [string]$LogFileName
    )

    $resultPath = Join-Path $resultDirectory $ResultFileName
    $logPath = Join-Path $resultDirectory $LogFileName

    Write-Host ""
    Write-Host "Project J $TestPlatform 테스트 실행"
    Write-Host "결과: $resultPath"
    Write-Host "로그: $logPath"

    $quotedProjectPath = '"' + $ProjectPath + '"'
    $quotedResultPath = '"' + $resultPath + '"'
    $quotedLogPath = '"' + $logPath + '"'

    $arguments = @(
        "-batchmode",
        "-nographics",
        "-projectPath", $quotedProjectPath,
        "-runTests",
        "-testPlatform", $TestPlatform,
        "-testResults", $quotedResultPath,
        "-logFile", $quotedLogPath
    )

    $process = Start-Process `
        -FilePath $UnityPath `
        -ArgumentList $arguments `
        -Wait `
        -PassThru `
        -NoNewWindow

    Write-Host "$TestPlatform 종료 코드: $($process.ExitCode)"
    return $process.ExitCode
}

Write-Host "Project J 자동 테스트 시작"
Write-Host "Unity: $UnityPath"
Write-Host "Project: $ProjectPath"

$editModeExitCode = Invoke-UnityTest `
    -TestPlatform "editmode" `
    -ResultFileName "EditModeResults.xml" `
    -LogFileName "EditMode.log"

$playModeExitCode = Invoke-UnityTest `
    -TestPlatform "playmode" `
    -ResultFileName "PlayModeResults.xml" `
    -LogFileName "PlayMode.log"

Write-Host ""
Write-Host "Project J 자동 테스트 결과"
Write-Host "EditMode 종료 코드: $editModeExitCode"
Write-Host "PlayMode 종료 코드: $playModeExitCode"
Write-Host "상세 결과 폴더: $resultDirectory"

if ($editModeExitCode -ne 0 -or $playModeExitCode -ne 0)
{
    Write-Error "하나 이상의 테스트 실행이 실패했습니다."
    exit 1
}

Write-Host "EditMode와 PlayMode 테스트가 모두 성공했습니다."
exit 0
