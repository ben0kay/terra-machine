# DG1 local validation

Status: awaiting compilation and native/runtime acceptance. The implementation is the single DG1 commit containing this document (SHA supplied in the delivery message). Do not treat a successful push as a passed test.

Prerequisites: project-matching Godot .NET editor (project SDK Godot.NET.Sdk/4.7.2), .NET 8 SDK, bundled Voxel Tools extension loading successfully on your platform. No editor/SDK upgrade or vendor changes were made.

From the repository root run:
```powershell
dotnet build terra-machine.csproj
```
Expected exit code 0 and no compiler errors. Then open the project in the matching Godot .NET editor and run WORLD/Scenes/test.tscn (F6, or F5). Report the command, exit code and relevant errors before changing code.

Checks:
1. Wait for Terrain ready. Go to an open patch near spawn; disable grass render layers via F1 if they obscure the ground.
2. Press 1. Bottom-left label shows shovel equipped. Look downward at terrain within 3.5 m and click once. Expect one small rounded depression and request feedback; allow native mesh/collision to catch up.
3. Hold LMB: only the initial edge digs. Click repeatedly with >=0.65 s pauses to extend a trench. Approach from the side, step into it and check the player's footing matches the updated geometry. Terrain remains one-metre base voxels, so this is not sub-centimetre shovel precision.
4. Aim at sky/distant ground, a boulder, trunk or branch: no edit behind/through it. Missing resident data must report a retry message and perform no partial border edit. Unsupported API messages must leave movement/streaming operational.
5. Escape then LMB: recapture only, no dig. Test F1 controls, benchmark blocking, focus loss, loading, unequip and F4 respawn. A queued click must not fire after losing control or teleporting.
6. Cross a nearby chunk boundary and dig on both sides. Watch for native errors, seams and collision stalls. Terrain debug modes still work. Renderer hiding does not disable physical terrain.
7. F2: compare idle shovel equipped/unequipped and individual clicks. New tool physics callback should run only for a queued click; no new steady timer/process. Profile native remesh/collider cost locally before lowering cooldown/increasing radius.
8. Scene reload resets the DG1 experiment. Persistence across restart/chunk eviction is not guaranteed. Cut walls may still have grass/leaf shading and collidable scenery can remain unsupported; DG2/vegetation invalidation are separate work.

Changed owners: PLAYER/Equipment/PlayerEquipment.cs; PLAYER/player.tscn and PlayerInput.cs; EQUIPMENT/Tools/Shovel/ (Shovel + typed config); WORLD/Terrain/Excavation/ (service + typed config); optional WORLD/Terrain/ITerrainExcavation.cs; native Integration/VoxelTerrainAdapter.cs; WORLD/Scenes/test.tscn and TestWorld.cs; project Input Map; owning documentation. Vendor addon, player movement controller, forest layout, geology and saves are untouched.

Actual checks here: upstream primary API/source review; project reference and focused C# input/loop/delimiter checks. Compilation and all visual/physics/performance checks above remain unrun.

## Shallow amount and foliage follow-up

New checks: F1 EXCAVATION defaults to 1.00; compare a fresh flat-ground click at 1 versus 4–6, and verify the higher setting excavates more while reach/cooldown remain. 1 means shallow default, not 1 m³. Test repeat clicks with pauses; one-metre voxel spacing limits very fine cuts. Reset slider to 1 manually or restart (session-only). Ensure a shallow click no longer always trips the stale-collider guard: that guard samples inward from the actual hit, not the now-outside brush centre.

Dig through Bermuda, forest meadow, spawn Meadow 2, ferns and moss. Roots within the cut should disappear promptly, across both/all active LOD batches and when F1 hides/re-enables layers. Walk away/back so grass tiles rebuild; no plants should respawn in recorded cuts while the edit remains. Dig during unfinished foliage placement and confirm final batches also prune. Walk along an untouched patch edge and verify it stays. Adjacent tree/rock/branch colliders remain unchanged. Native collision rebuild is still async.

Implementation source/paths and flat-cap volume math were checked here. Godot/.NET are unavailable; compilation, live MultiMesh compaction/culling/contact/streaming and measured frame cost still require local checks. No claim of observed runtime success for this follow-up.
