# Autonomous Bot Navigation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make bots traverse the level by local physics-based decisions without route nodes, and make all players pass through one another.

**Architecture:** A pure Runtime navigation policy scores sensor observations, a focused Runtime sensor gathers non-allocating physics observations, and the existing Fusion bot controller resolves checkpoint/finish targets and emits normal player inputs. Direct player movement queries explicitly filter player colliders in addition to the existing layer collision rule.

**Tech Stack:** Unity C#, Photon Fusion, Unity Physics, NUnit EditMode tests

**Spec:** `docs/superpowers/specs/2026-08-26-autonomous-bot-navigation-design.md`

## Global Constraints

- Preserve `Assets/ProjectJ/Scenes/Game.unity` exactly as found.
- Do not commit changes; the user will test the working tree first.
- Use Allman braces and concise Korean comments on functional C# lines; do not comment closing braces.
- Keep the existing player movement statistics and State Authority ownership model.
- Keep push and item targeting behavior.

---

### Task 1: Player query filtering

**Files:**
- Modify: `Assets/ProjectJ/Runtime/Player/PlayerCollisionRules.cs`
- Modify: `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs`
- Test: `Assets/ProjectJ/Tests/EditMode/PlayerCollisionRulesTests.cs`

**Interfaces:**
- Produces: `PlayerCollisionRules.ExcludePlayerLayer(int sourceMask)` and network-player collider filtering in all locomotion queries.

- [ ] Add failing tests proving the Player layer is removed while Default terrain remains.
- [ ] Run the EditMode test assembly and confirm the new assertion fails.
- [ ] Add `ExcludePlayerLayer` and skip colliders belonging to another `ProjectJNetworkPlayer` in movement, ground, ceiling, and clearance result loops.
- [ ] Re-run the collision tests and confirm they pass.

### Task 2: Pure autonomous-navigation policy

**Files:**
- Modify: `Assets/ProjectJ/Runtime/AI/ProjectJBotNavigationPolicy.cs`
- Test: `Assets/ProjectJ/Tests/EditMode/ProjectJBotNavigationPolicyTests.cs`

**Interfaces:**
- Produces: `ProjectJBotTraversalAction`, `ProjectJBotTraversalCandidate`, `ProjectJBotTraversalDecision`, `SelectBestCandidate(...)`, and `CanReachLanding(...)`.

- [ ] Add failing tests for walk, jump, gap, cliff, blocked headroom, goal alignment, failed direction, and ballistic reach.
- [ ] Run the focused policy tests and confirm the new tests fail.
- [ ] Implement only the data types and deterministic scoring needed by those tests.
- [ ] Re-run the focused tests and confirm they pass.

### Task 3: Physics traversal sensor

**Files:**
- Create: `Assets/ProjectJ/Runtime/AI/ProjectJBotTraversalSensor.cs`
- Create: `Assets/ProjectJ/Runtime/AI/ProjectJBotTraversalSensor.cs.meta`
- Test: `Assets/ProjectJ/Tests/EditMode/ProjectJBotTraversalSensorTests.cs`
- Create: `Assets/ProjectJ/Tests/EditMode/ProjectJBotTraversalSensorTests.cs.meta`

**Interfaces:**
- Consumes: policy candidate types from Task 2 and `PlayerCollisionRules.ExcludePlayerLayer` from Task 1.
- Produces: `TrySelectTraversal(Vector3 position, Vector3 goalDirection, float safetyFloorY, float moveSpeed, float jumpSpeed, float gravity, float colliderRadius, float colliderHeight, Vector3 failedDirection, out ProjectJBotTraversalDecision decision)`.

- [ ] Add physics-scene tests for a safe floor, an unsafe cliff, a low step, and player-layer exclusion.
- [ ] Run the focused tests and confirm failure because the sensor is absent.
- [ ] Implement twelve-direction non-allocating ground, body, and landing-clearance sampling.
- [ ] Re-run the sensor and policy tests.

### Task 4: Fusion bot controller integration

**Files:**
- Modify: `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkBotController.cs`
- Modify: `Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkExternalGameplay.cs`
- Test: `Assets/ProjectJ/Tests/EditMode/ProjectJDay139BotSpawnSoloStartTests.cs`

**Interfaces:**
- Consumes: traversal sensor decision and existing player movement properties.
- Produces: checkpoint/finish target resolution, route-independent `TryBuildInput`, safe-direction memory, and `RequestBotRecoveryRespawn()`.
- Preserves: `RefreshRoute`, `RouteCount`, `HasRoute`, `CurrentRouteIndex`, and `ConfigureStartDelay` for existing callers.

- [ ] Add reflection tests proving the controller references the sensor contract rather than requiring route nodes and exposes legacy-compatible entry points.
- [ ] Run the focused tests and confirm the new expectations fail.
- [ ] Replace route-node lists with sorted checkpoints plus finish, drive normal move/jump input from sensor decisions, and reset state on checkpoint/respawn changes.
- [ ] Add State Authority-only recovery respawn with prolonged-stall and cooldown guards.
- [ ] Re-run bot and policy tests.

### Task 5: Safe legacy Editor cleanup

**Files:**
- Delete: only reference-free pre-Day136 one-shot scripts under `Assets/ProjectJ/Editor`, with matching `.meta` files.

**Interfaces:**
- Produces: no runtime interface changes.

- [ ] Search every deletion candidate by class name and GUID.
- [ ] Preserve any candidate used by runtime code, serialized assets, build automation, or current operational workflows visible in the repository.
- [ ] Delete the safe subset using an explicit patch.
- [ ] Search again to confirm no dangling source or GUID references.

### Task 6: Verification

**Files:**
- Verify: all changed C# files and tests.
- Preserve: `Assets/ProjectJ/Scenes/Game.unity`.

**Interfaces:**
- Produces: evidence for compile and automated-test status, plus a manual test checklist.

- [ ] Compile available Unity-generated solutions or projects.
- [ ] Run relevant EditMode tests through Unity batch mode when Unity is installed and available.
- [ ] Inspect `git diff --check`, `git status`, and the scene diff to prove `Game.unity` was not changed by this work.
- [ ] Report exact passed, failed, and unverified items without committing.

