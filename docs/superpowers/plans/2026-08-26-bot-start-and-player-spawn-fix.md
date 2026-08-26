# Bot Start and Player Spawn Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore autonomous bot movement after spawn and place human players on numbered race spawn points.

**Architecture:** Keep terrain choice in the Runtime navigation sensor and policy, correcting the safety baseline at the deterministic policy boundary. Reuse the existing scene spawn-point component from the Fusion player spawner, with the existing generated coordinate retained only as a fallback.

**Tech Stack:** Unity C#, Photon Fusion, Unity Physics, NUnit EditMode tests

**Spec:** `docs/superpowers/specs/2026-08-26-bot-start-and-player-spawn-fix-design.md`

## Global Constraints

- Preserve `Assets/ProjectJ/Scenes/Game.unity` exactly as found.
- Do not commit changes; the user will test the working tree first.
- Use Allman braces and concise Korean comments on functional C# lines; do not comment closing braces.
- Preserve the existing `0.6m` maximum safe drop.

---

### Task 1: Grounded bot traversal

**Files:**
- Modify: `Assets/ProjectJ/Tests/EditMode/ProjectJBotAutonomousNavigationPolicyTests.cs`
- Create: `Assets/ProjectJ/Tests/EditMode/ProjectJBotTraversalSensorTests.cs`
- Create: `Assets/ProjectJ/Tests/EditMode/ProjectJBotTraversalSensorTests.cs.meta`
- Modify: `Assets/ProjectJ/Runtime/AI/ProjectJBotNavigationPolicy.cs`
- Modify: `Assets/ProjectJ/Runtime/AI/ProjectJBotTraversalSensor.cs`

**Interfaces:**
- Consumes: `ProjectJBotNavigationPolicy.SelectBestCandidate(...)` and `ProjectJBotTraversalSensor.TrySelectTraversal(...)`.
- Produces: valid flat-floor and jump decisions when the respawn marker is above the grounded feet, without treating the sensor owner as an obstacle.

- [ ] **Step 1: Write failing tests**

```csharp
[Test] // 소환 높이 보정 검증
public void SelectBestCandidate_AllowsCurrentFloorBelowSpawnMarker() // 현재 바닥 이동 허용 검증
{
    ProjectJBotTraversalCandidate candidate = new ProjectJBotTraversalCandidate( // 평지 후보 생성
        Vector3.forward, // 전방 방향
        Vector3.forward, // 바닥 착지 위치
        0f, // 평지 높이 차이
        true, // 바닥 존재
        true, // 경로 확보
        true, // 머리 공간 확보
        false // 틈 없음
    );
    ProjectJBotTraversalDecision result = ProjectJBotNavigationPolicy.SelectBestCandidate( // 후보 선택 실행
        new[] { candidate }, // 단일 후보 전달
        Vector3.forward, // 목표 방향 전달
        2f, // 높은 소환 지점 전달
        0.35f, // 계단 한계 전달
        1.5f, // 점프 한계 전달
        0.6f, // 낙하 한계 전달
        Vector3.zero // 실패 방향 없음
    );
    Assert.That(result.IsValid, Is.True); // 현재 평지 허용 검증
}
```

- [ ] **Step 2: Verify RED**

Run: Unity Test Runner for `ProjectJBotAutonomousNavigationPolicyTests.SelectBestCandidate_AllowsCurrentFloorBelowSpawnMarker`.

Expected: FAIL because the candidate landing at `Y=0` is rejected against `minimumSafeY=2`.

- [ ] **Step 3: Implement the minimum fix**

Derive the candidate's current foot height as `LandingPosition.y - HeightDelta`, clamp the supplied safety baseline to that value, and preserve `maximumSafeDrop`. Replace the jump `CheckSphere` with a non-allocating overlap result loop that skips the sensor owner's hierarchy and the Player layer; apply the same filter to the other sensor query result loops.

- [ ] **Step 4: Verify GREEN**

Run: Unity Test Runner for `ProjectJBotAutonomousNavigationPolicyTests` and `ProjectJBotTraversalSensorTests`.

Expected: PASS for flat ground below a spawn marker and a jump arc beginning inside the owner's collider.

---

### Task 2: Shared human and bot spawn slots

**Files:**
- Modify: `Assets/ProjectJ/Tests/EditMode/ProjectJDay139BotSpawnSoloStartTests.cs`
- Modify: `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayerSpawner.cs`
- Modify: `Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkPlayer.prefab`
- Modify: `Assets/ProjectJ/Network/Fusion/Player/Resources/ProjectJNetworkBot.prefab`

**Interfaces:**
- Consumes: `ProjectJNetworkSpawnPoint.TryGetPose(int, out Vector3, out Quaternion)`.
- Produces: `TryGetSpawnPoseForSlot(int, out Vector3, out Quaternion)` and Player-layer character prefab roots.

- [ ] **Step 1: Write failing tests**

Add an EditMode reflection test that creates a real `ProjectJNetworkSpawnPoint`, invokes `TryGetSpawnPoseForSlot`, and verifies the exact position and rotation. Add prefab tests that load both network character prefabs and assert their root layer equals `LayerMask.NameToLayer("Player")`.

- [ ] **Step 2: Verify RED**

Run: Unity Test Runner for `ProjectJDay139BotSpawnSoloStartTests`.

Expected: FAIL because `TryGetSpawnPoseForSlot` does not exist and both prefab roots use `Default`.

- [ ] **Step 3: Implement the minimum fix**

Change the player spawner to calculate the existing active-player-order slot, resolve a scene pose by slot, and pass both position and rotation to `Runner.Spawn`. Keep `(slot * 3f, 2f, 4f)` plus identity rotation only as the missing-marker fallback. Set both prefab hierarchies to the Player layer.

- [ ] **Step 4: Verify GREEN**

Run: Unity Test Runner for `ProjectJDay139BotSpawnSoloStartTests`.

Expected: PASS for exact marker pose resolution and Player-layer prefab roots.

---

### Task 3: Verification

**Files:**
- Verify: all changed C# files, prefab YAML, and tests.

**Interfaces:**
- Produces: compile, formatting, and working-tree evidence without a commit.

- [ ] Run `dotnet build ProjectJ.Tests.EditMode.csproj --no-restore`.
- [ ] Run `dotnet build Assembly-CSharp.csproj --no-restore`.
- [ ] Run Unity EditMode tests when the project lock is released.
- [ ] Run `git diff --check`, inspect `git status --short`, and confirm no new `Game.unity` changes were introduced by this fix.
