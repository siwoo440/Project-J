# Autonomous Bot Navigation and Player Overlap Design

## Goal

Replace route-node following with physics-sensed local traversal while retaining checkpoints and the finish trigger as global progress targets. Players and bots must pass through each other without affecting terrain movement queries.

## Navigation architecture

- `ProjectJNetworkBotController` remains the State Authority input producer and preserves start-delay, push, item, roster, and legacy refresh entry points.
- `ProjectJBotTraversalSensor` samples twelve local directions with non-allocating physics queries. It detects ground, step height, gaps, landing clearance, obstacles, and unsafe drops while excluding player-layer colliders.
- `ProjectJBotNavigationPolicy` remains a pure Runtime policy. It rejects unsafe candidates, classifies walk or jump actions, scores goal alignment and upward progress, penalizes failed directions, and selects the best candidate.
- Checkpoints are sorted by `CheckpointId`. The checkpoint following `CurrentCheckpointId` is the global target; after CP4, `FinishTrigger` is the target.
- Route nodes do not participate in runtime navigation. Existing route objects remain untouched in `Game.unity` because that scene contains user changes.

## Traversal rules

- Ground-continuous candidates up to the player's 0.35 m step capability use walking.
- Higher reachable landings and safe small gaps use the existing player jump input.
- Jump reach uses the player's exposed walk speed, jump speed, gravity, collider height, and collider radius. Bots receive no hidden movement bonus.
- Candidates without ground, headroom, or a safe landing are rejected.
- Candidates below the current checkpoint respawn-height safety baseline are rejected.
- Direction commitment prevents frame-to-frame oscillation. Replanning occurs after the sensor interval, lost safety, landing, checkpoint changes, or stalled movement.

## Recovery

- Meaningful horizontal progress resets the stalled timer and stores a recent safe position.
- A stalled direction is temporarily penalized, causing side and rear alternatives to be evaluated.
- When no forward option exists, the bot may move toward a recent safe position without going below the checkpoint baseline.
- A State Authority-only recovery respawn is allowed after prolonged failure and is protected by a cooldown.

## Player overlap

- Unity's existing Player-to-Player layer collision ignore rule remains active.
- Every direct movement capsule cast, ground query, position overlap, ceiling query, and standing-clearance query ignores other `ProjectJNetworkPlayer` colliders.
- Bot traversal sensing excludes the Player layer, so players and bots are never treated as walls, steps, floors, or landing surfaces.
- Push and item target queries remain unchanged.

## Cleanup

- Re-check references before deletion.
- Delete only unused, one-shot Editor installer/setup scripts created before Day136, together with their `.meta` files.
- Preserve reusable operational tools such as a still-required dedicated server builder.
- Do not delete Day136-or-later scripts, runtime route components, scene route objects, or modify `Game.unity` in this change.

## Verification

- Policy tests cover unsafe drops, walk/jump classification, blocked landings, goal scoring, failed-direction penalties, and ballistic reach.
- Physics tests cover stairs, gaps, headroom, cliffs, and ignoring player-layer colliders where practical.
- Collision tests prove that movement query filtering excludes players while retaining world layers.
- Compile the Unity-generated C# projects and run relevant EditMode tests when the local Unity installation permits batch execution.
- Manual acceptance remains required for Start-to-Finish behavior in the current Game scene and multiplayer overlap behavior.

