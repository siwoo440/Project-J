$ErrorActionPreference = "Stop"

$ProjectRoot = $PSScriptRoot
$ExternalPath = Join-Path $ProjectRoot "Assets\ProjectJ\Network\Fusion\Player\ProjectJNetworkExternalGameplay.cs"
$InventoryPath = Join-Path $ProjectRoot "Assets\ProjectJ\Network\Fusion\Player\ProjectJNetworkItemInventory.cs"

function Read-NormalizedFile([string]$Path)
{
    if (-not (Test-Path $Path))
    {
        throw "파일을 찾을 수 없습니다: $Path"
    }

    $Text = [System.IO.File]::ReadAllText($Path)
    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Write-Utf8File([string]$Path, [string]$Content)
{
    $Utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $Utf8NoBom)
}

function Replace-OnceOrSkip(
    [string]$Content,
    [string]$OldText,
    [string]$NewText,
    [string]$Label
)
{
    if ($Content.Contains($NewText))
    {
        Write-Host "[SKIP] $Label"
        return $Content
    }

    $Index = $Content.IndexOf($OldText)

    if ($Index -lt 0)
    {
        throw "$Label 적용 위치를 찾지 못했습니다."
    }

    $SecondIndex = $Content.IndexOf($OldText, $Index + $OldText.Length)

    if ($SecondIndex -ge 0)
    {
        throw "$Label 적용 위치가 2개 이상입니다."
    }

    Write-Host "[APPLY] $Label"
    return $Content.Substring(0, $Index) + $NewText + $Content.Substring($Index + $OldText.Length)
}

$External = Read-NormalizedFile $ExternalPath
$Inventory = Read-NormalizedFile $InventoryPath

$External = Replace-OnceOrSkip `
    $External `
    "public sealed class ProjectJNetworkExternalGameplay :" `
    "public sealed partial class ProjectJNetworkExternalGameplay :" `
    "ExternalGameplay partial 선언"

$External = Replace-OnceOrSkip `
    $External `
    "NetworkPushCooldown = TickTimer.CreateFromSeconds(`n                Runner,`n                PushCooldownSeconds`n            ); // 시도 즉시 쿨타임 시작" `
    "NetworkPushCooldown = TickTimer.CreateFromSeconds(`n                Runner,`n                CurrentPushCooldownSeconds`n            ); // 현재 Push 재사용 시간 적용" `
    "망치 Push 쿨타임 연결"

$External = Replace-OnceOrSkip `
    $External `
    "pushDirection.normalized * PushForce" `
    "pushDirection.normalized * CurrentPushForce" `
    "망치 Push 힘 연결"

$External = Replace-OnceOrSkip `
    $External `
    "distanceSquared > PushSearchRange * PushSearchRange" `
    "distanceSquared > CurrentPushSearchRange * CurrentPushSearchRange" `
    "망치 Push 사거리 연결"

$Inventory = Replace-OnceOrSkip `
    $Inventory `
    "InitializeJetpackAuthority(); // 제트팩 연료 상태 초기화`n            InitializeSnowballAuthority(); // 눈덩이 감속 상태 초기화" `
    "InitializeJetpackAuthority(); // 제트팩 연료 상태 초기화`n            InitializeHammerAuthority(); // 망치 강화 상태 초기화`n            InitializeSnowballAuthority(); // 눈덩이 감속 상태 초기화" `
    "망치 Spawn 초기화"

$Inventory = Replace-OnceOrSkip `
    $Inventory `
    "ClearJetpackAuthority(); // 제트팩 효과 제거`n            ClearSnowballSlowAuthority(); // 눈덩이 감속 효과 제거" `
    "ClearJetpackAuthority(); // 제트팩 효과 제거`n            ClearHammerAuthority(); // 망치 효과 제거`n            ClearSnowballSlowAuthority(); // 눈덩이 감속 효과 제거" `
    "망치 전체 초기화 연결"

$Inventory = Replace-OnceOrSkip `
    $Inventory `
    "ClearJetpackAuthority(); // 부활 시 제트팩 효과 즉시 제거`n            ClearSnowballSlowAuthority(); // 부활 시 눈덩이 감속 제거" `
    "ClearJetpackAuthority(); // 부활 시 제트팩 효과 즉시 제거`n            ClearHammerAuthority(); // 부활 시 망치 효과 즉시 제거`n            ClearSnowballSlowAuthority(); // 부활 시 눈덩이 감속 제거" `
    "망치 Respawn 해제 연결"

$OldSwitch = @"
                case ProjectJNetworkItemId.Jetpack: // 제트팩 선택 상태
                    success = UseJetpackAuthority(); // 서버 권한 5초 연료 활성화
                    break; // 제트팩 분기 종료

                case ProjectJNetworkItemId.Snowball:
"@

$NewSwitch = @"
                case ProjectJNetworkItemId.Jetpack: // 제트팩 선택 상태
                    success = UseJetpackAuthority(); // 서버 권한 5초 연료 활성화
                    break; // 제트팩 분기 종료

                case ProjectJNetworkItemId.Hammer: // 망치 선택 상태
                    success = UseHammerAuthority(); // 서버 권한 6초 밀치기 강화 활성화
                    break; // 망치 분기 종료

                case ProjectJNetworkItemId.Snowball:
"@

$Inventory = Replace-OnceOrSkip `
    $Inventory `
    $OldSwitch `
    $NewSwitch `
    "망치 아이템 사용 분기 연결"

Copy-Item $ExternalPath "$ExternalPath.day113.bak" -Force
Copy-Item $InventoryPath "$InventoryPath.day113.bak" -Force

Write-Utf8File $ExternalPath $External
Write-Utf8File $InventoryPath $Inventory

$RequiredMarkers = @(
    @($External, "public sealed partial class ProjectJNetworkExternalGameplay :", "External partial"),
    @($External, "CurrentPushCooldownSeconds", "Push cooldown"),
    @($External, "pushDirection.normalized * CurrentPushForce", "Push force"),
    @($External, "CurrentPushSearchRange * CurrentPushSearchRange", "Push range"),
    @($Inventory, "InitializeHammerAuthority(); // 망치 강화 상태 초기화", "Hammer initialize"),
    @($Inventory, "ClearHammerAuthority(); // 망치 효과 제거", "Hammer clear"),
    @($Inventory, "ClearHammerAuthority(); // 부활 시 망치 효과 즉시 제거", "Hammer respawn"),
    @($Inventory, "case ProjectJNetworkItemId.Hammer: // 망치 선택 상태", "Hammer use")
)

foreach ($Marker in $RequiredMarkers)
{
    if (-not $Marker[0].Contains($Marker[1]))
    {
        throw "최종 검증 실패: $($Marker[2])"
    }
}

Write-Host ""
Write-Host "DAY 113 FIX APPLIED"
