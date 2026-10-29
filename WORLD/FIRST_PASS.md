# Terrain visual test — first pass

| Phase | Goal | Suggested assistant | Status |
| --- | --- | --- | --- |
| T0 | Modular player, smooth hills, Sky3D and safe test scene | Codex / Work | Implemented; awaiting local validation |
| T1 | Isolated excavation requests and updated collision | Codex / Work | Planned; not authorized in this job |
| T2 | Persistent edits and deterministic reload | Codex / Work | Planned; not authorized in this job |

## T0 scope and dependencies

User-authorized first visual pass only. Godot .NET matching SDK 4.7.2, .NET 8 and the already committed Voxel Tools/Sky3D addons. Mirrors War Machine's CAPS feature owners, reusable scene and Configuration/Master resource pattern without importing its gameplay systems. Terra Machine's own main branch is the implementation destination; War Machine was read-only reference.

Source includes PLAYER input/settings/controller/prefab; WORLD test composition/config/HUD; isolated terrain/native resources/shader; reusable Sky3D environment; .NET csproj and F5 entry point. No digging, saving, resource collection, entities, vegetation or vehicles.

## Performance handoff

Startup/recovery wakes a 10Hz collision ray until success/timeout. Player physics is disabled while waiting. Native viewer movement drives streamed chunks without C# copying transforms. Generated terrain is bounded to 1024 × 384 × 1024 voxels with 384m view distance and four LODs. Native terrain settings govern actual meshing/collision workload; performance is not measured here. During play: player capsule physics, one cheap composition bounds check, 4Hz HUD, native streaming and Sky3D/cloud rendering. Fixed time disables the clock; clouds can still animate. Scene unload removes the externally parented viewer and scene-owned timers/nodes.

## Validation performed

Reviewed code, scene/resource paths, load-step counts, input constants, boundaries and new loop syntax. Verified native extension manifest and inspected bundled Sky3D initialization. Read upstream Voxel Tools API and Transvoxel shader source. No Godot/.NET executable is installed here: **compilation, scene parsing, native extension compatibility, shader compilation, rendering and performance are not verified**.

## Local acceptance — keep brief

1. Pull, open project with the matching Godot .NET editor and let initial asset/C# import finish. Build with the editor's Build button or `dotnet build terra-machine.csproj`.
2. F6 `WORLD/Scenes/test.tscn` (F5 selects it too). Expect sky, green/rock smooth hills, loading status then Terrain ready, and a player settling onto the surface. Missing extension must report an error and keep the player suspended; timeout should allow F4 retry.
3. Walk/turn/sprint/RMB jump; check slopes and landing, mouse inversion if desired, Escape/click recapture and alt-tab release. Let a few chunks load while walking; ensure collision follows and inspect LOD seams.
4. F4 should return to the ground at spawn; walking outside the safety bound recovers there. Stop/restart and check no viewer/extension cleanup errors. Check sun/clouds and FPS/frame time on the user's machine.

T0 is accepted only after these local build/visual checks. Next work needs a separate user request. No runtime success or measured FPS claim is implied by pushing the source.
