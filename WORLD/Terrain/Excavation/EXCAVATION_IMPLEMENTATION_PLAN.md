# Excavation Implementation Plan — Terra Machine

**Status:** DG1 implemented in source, awaiting local compilation/native-runtime validation. DG0 remains documentation-complete; DG2/DG3/DG4 are not implemented. The owner authorised DG1 in this session; later phases still require separate authorization.

**Current aim:** Prove that a player can press **1** to equip a fake shovel and left-click **nearby soil** to make a small **real voxel depression** with refreshed collision. Longer-term uses are shallow trenches, animal pits/zombie traps, post holes and minimal construction preparation — **not** free deep mining.

## Phases and allocation

| Phase | Goal | Suggested owner | Status |
| --- | --- | --- | --- |
| **DG0** | Documentation and reserved folder ownership (this commit) | Regular Chat | Implemented (docs only) |
| **DG1** | Key 1 equips placeholder shovel; one click requests a small excavation | Codex | Awaiting Validation |
| **DG2** | Gameplay material/depth resistance and visible soil/rock cut surfaces | Codex | Planned |
| **DG3** | Save/reload edited terrain while preserving base generator and streaming | Codex | Planned |
| **DG4** | Pit/trench interactions with animal/zombie navigation or traps | Codex, later | Deferred |

Each phase is independently reviewable. Do not automatically implement the next phase.

## Current confirmed architecture (as inspected)

- Existing test scene: `WORLD/Scenes/test.tscn`; `WORLD/Scenes/TestWorld.cs` handles composition and waits for collision at spawn.
- Reusable player: `PLAYER/player.tscn`; `PlayerInput.cs` captures/discards input and manages mouse, `PlayerController.cs` owns movement. F1/F2/F4, arrow keys, Shift, RMB, Esc and LMB are already used in the test.
- Game-owned terrain contract: `WORLD/Terrain/ITerrainService.cs` with `BindViewer` / `TryFindGround`; no edit operation yet.
- Only `WORLD/Terrain/Integration/VoxelTerrainAdapter.cs` touches native `VoxelLodTerrain` / Voxel Tools dynamic APIs. It generates SDF terrain from an image-backed landscape and owns native viewer binding. Preserve terrain streaming, LOD, existing grass/forest integration and shader debug.
- Existing ground visuals use slope/forest textures, **not authoritative geological material IDs**. Editing may expose incorrect green or grassy tunnel walls until material-aware rendering is designed.
- Voxel edit persistence, dig permissions and depth rules are **not implemented** at the time of writing. Recheck actual source before coding because it can evolve.

## Boundaries and proposed ownership

```text
PLAYER/
  Equipment/README.md               # slot/equip coordination; no native terrain calls
EQUIPMENT/
  README.md
  Tools/
    Shovel/
      README.md                      # tool input, reach and single click activation
      Configuration/README.md        # future ShovelMaster.tres
WORLD/Terrain/
  ITerrainService.cs                 # existing shared terrain boundary
  ITerrainExcavation.cs              # proposed small game-owned edit contract (DG1)
  Excavation/
    README.md
    EXCAVATION_IMPLEMENTATION_PLAN.md
    Configuration/README.md         # future ExcavationMaster.tres
  Integration/VoxelTerrainAdapter.cs # existing sole Voxel Tools API gateway
```

Request flow: **hotkey/tool → targeted terrain hit → excavation service validates → ITerrainExcavation → native adapter makes a bounded edit → voxel meshing and collision refresh**. Keep player/tools unaware of Voxel Tools class strings. The new interface should be **optional**, not a breaking rewrite of `ITerrainService`, unless a concrete implementation reason is established.

### DG0 — Documentation and folders (implemented)

Create only README/plan files to reserve owning paths. No empty C# files, placeholder scripts or fabricated `.tres` assets. Retain repo's existing module documentation patterns.

**Pass:** Paths/plan present on the active branch; no runtime behavior changed. **Validation:** Git tree inspection only.

### DG1 — Simple fake shovel with voxel removal (Codex)

**Dependencies:** Confirm native addon exposes terrain-edit API compatible with the installed version and C# dynamic invocation. Do not assume API signatures; inspect bundled addon docs/source and test in a disposable area. Check existing Input Map actions.

**Scope:**
1. Press `1` to select/equip shovel. Prefer a tiny temporary visual indicator over an elaborate inventory/hotbar; an optional crude first-person mesh can be added if simple.
2. Mouse LMB while captured and enabled performs **one** targeted request per click, using a short, configurable reach, small spherical/rounded dig brush and cooldown. Ignore hits on non-terrain. Never dig when cursor is released, controls are blocked, or terrain isn't loaded.
3. Physically subtract the affected voxel volume; update native mesh and collision. Allow consecutive clicks to form a **shallow trench**. Surface/edit alignment must be correct in world/local voxel coordinates. Limit per-click affected area; handle pending meshing/collision safely.
4. Preserve existing world/player controls, viewer-driven streaming, render debug, and terrain appearance where possible. Do not alter vendor addon code.
5. Leave persistence, tools beyond the fake shovel, resource drops, real material hardness, zombie noise, stamina, animations and trap AI out of DG1.

**Likely ownership:** `PLAYER/Equipment/`, `EQUIPMENT/Tools/Shovel/`, `WORLD/Terrain/ITerrainExcavation.cs`, `WORLD/Terrain/Excavation/`, and **minimal** modifications to `Integration/VoxelTerrainAdapter.cs`, `player.tscn`, `test.tscn`, Input Map and associated READMEs. Only create config files actually used.

**Performance:** No new per-frame idle loops; use input edges, a raycast on action and one bounded native edit per click. Do not recalculate entire world meshes. Queue/coalesce edits only if evidence requires it. Keep editing away from nonresident chunks.

**Pass:** In the test scene, `1` selects shovel; one LMB click near soil makes a real depression; repeated clicks create a shallow trench; the player can step into it and collision matches the mesh; no accidental changes to non-terrain assets. Compilation, gameplay and native API checks required locally; no claim of validation before testing.

### DG2 — Material and depth resistance (Codex)

**Dependencies:** DG1 passes; inspect current generator's original surface and shader/material channels before adding gameplay geology.

**Scope:** Create material identities for **topsoil → compacted soil/gravel → weathered rock → solid bedrock**, with a **configurable** difficulty curve that can rise steeply (possibly exponentially) below an initial depth. Derive depth from **original local generated surface**, not arbitrary world Y. Exposed bedrock on cliffs and variable soil thickness must be possible. For an early shovel test, bedrock may simply refuse excavation and report unobtrusive feedback.

Do not force an expensive new geological generator or invent precise hardness numbers upfront. Keep visuals of newly exposed soil/rock credible; test removal alongside steep slopes and cliff faces. Separate material hardness from surface texture blending.

**Performance:** Only sample rules at the action location and affected edit bounds; avoid continuous geological queries or reprocessing global height images.

**Pass:** Shovel digs shallow soil, slows/refuses harder ground appropriately, cliff-side behavior is sensible, and exposed walls do not conspicuously show grass.

### DG3 — Persistent voxel edits (Codex)

**Dependencies:** DG1/DG2 validated; verify streaming mode and a compatible Voxel Tools storage/stream setup.

**Scope:** Use Voxel Tools' suitable persistent stream/storage, preserving deterministic/generated base terrain plus modified blocks. Define save slot, load/unload ordering, flush-on-exit and basic corruption/backup handling. Avoid writing a competing custom voxel chunk saver. Consider generator revisions and how modified chunks interact with future landscape tuning.

**Pass:** Dig trench, leave the region or quit safely, re-enter and find identical edited collision/geometry; unedited chunks still generate normally. Test with grass/trees nearby and across chunk boundaries.

### DG4 — Practical traps (deferred)

**Goal:** Later use shallow trenches/pits for hunting or diverting zombies, and post holes for shelter building. Validate actual creature physics and navigation before creating gameplay expectations. A hole alone should not automatically create a guaranteed animal trap. Avoid global navmesh rebuild per edit; prefer localized/event-driven updates.

**Out of scope:** Full structural collapse, deep industry/mining, crafting trees, vehicle digging, many tools, advanced AI, complex inventory and sound detection. Future noise-from-digging can integrate with a shared sound event system, rather than be embedded in the tool.

## Risk / simplification notes for Codex

- **Biggest compatibility risk:** Native Voxel Tools editor version and direct C# interop for editing; verify exact method signatures rather than guessing.
- **Visual risk:** Existing green/leaf/slope ground shader may colour exposed underground surfaces incorrectly.
- **Gameplay risk:** Depth below original topography differs from depth below a player's freshly excavated surface. Keep source terrain/geology separate from modifications.
- **Scale risk:** Terrain editing must leave streaming intact. Large/continuous brushes, per-frame whole-region scans and frequent expensive collider rebuilds are not part of the first pass.
- **Simplify aggressively:** No realistic shovel art, durability, tools inventory, or mining resource economy is required to prove one hole.
- **Testing handoff:** At each implementation, report what was actually compiled/tested, what is unverified locally, and any native extension/runtime compatibility issue. Follow `CodePreferences.md` where applicable, but note its War Machine-specific branch references do not apply to Terra Machine's `main`.

**Next action:** Validate DG1 locally using DG1_VALIDATION.md; do not automatically start DG2. The DG0 implementation-time architecture snapshot above remains historical context.

## DG1 implementation handoff

Implemented PlayerEquipment + event-driven HUD, Shovel/Settings/Master, ExcavationService/Settings/Master and optional ITerrainExcavation. TestWorld binds them; the player remains usable without an excavation backend. Key 1 toggles, captured LMB queues one action, 0.65 s cooldown, 3.5 m reach and 1 m-radius rounded subtraction (0.15 m inset). One physics callback handles it then disables itself. UI/loading/focus loss/unequip/respawn cancel or reject pending requests. Mouse recapture is consumed before tool dispatch.

VoxelTerrainAdapter alone caches the native tool, selects named CHANNEL_SDF/MODE_REMOVE constants, validates methods, stops on props, transforms the world hit into terrain-local coordinates, rejects scaled terrain and padded unloaded/out-of-bounds areas, and invokes do_sphere. One optional current-SDF check rejects stale already-empty collider hits. Native post_edit_area schedules LOD mesh/collision refresh; a successful request is not a claim that collision has already caught up. No persistence, hardness, cut-face material changes or vegetation cleanup was added. Use an open patch or F1 render toggles to inspect the actual excavation. Edits can disappear on chunk eviction/restart; DG3 owns that.

Upstream API/source was reviewed (VoxelTool do_sphere/is_area_editable, SDF channel/mode, and VoxelToolLodTerrain::_post_edit). The bundled repo contains binaries, not corresponding API source/version metadata; installed-binary behaviour must still be verified locally. No Godot or dotnet executable is installed in this environment, so compilation/native runtime were not run. Source wiring, resource paths, input edges, callback sleep/cancellation and loops were reviewed. Full handoff: DG1_VALIDATION.md. Status remains Awaiting Validation until local results are available. This implementation is the single DG1 commit containing this section; its immutable SHA is supplied in the handoff response.
