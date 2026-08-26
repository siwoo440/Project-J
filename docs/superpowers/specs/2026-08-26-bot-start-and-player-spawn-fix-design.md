# Bot Start and Player Spawn Fix Design

## Goal

Make spawned bots select a valid movement immediately after landing, make traversal queries ignore player bodies, and place human players on the same numbered scene spawn points used by the match flow.

## Confirmed Causes

- Scene spawn markers are at world `Y=2`, while grounded character feet settle on the floor below.
- The bot policy rejects every landing below the unsnapped respawn marker height.
- Network player and bot prefab roots are on `Default`, so the existing Player-layer query exclusion does not apply to their colliders.
- `ProjectJNetworkPlayerSpawner` uses `(slot * 3, 2, 4)` and `Quaternion.identity` instead of `ProjectJNetworkSpawnPoint` poses.

## Design

- Clamp the traversal safety baseline to the currently observed foot height reconstructed from each candidate. Preserve the existing per-step maximum drop limit.
- Filter the sensor's own collider and Player-layer colliders from ground, walk, landing-clearance, and jump-arc queries.
- Assign both network player prefabs and their children to the configured `Player` layer.
- Resolve a joining player's active-player-order slot through `ProjectJNetworkSpawnPoint.TryGetPose`; use the old generated position only when the scene has no matching marker.
- Spawn with the resolved rotation as well as position.

## Constraints

- Do not modify `Assets/ProjectJ/Scenes/Game.unity`.
- Do not commit.
- Preserve Allman braces and concise Korean comments on functional C# lines.
