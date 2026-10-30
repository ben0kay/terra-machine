# TERRA MACHINE

> **Working title:** Terra Machine  
> **Direction:** Hardcore first-person wilderness survival simulator in the long term; **currently focused on Sons of the Forest-like landscape immersion**, semi-believable visuals and optimized forests over new gameplay systems. Smooth editable voxel terrain remains foundational.  
> **Status (2026-10-29):** Early Godot project / technical foundation. The game vision below is **proposed direction**, not a list of implemented systems or blanket authorization to build them.

## Read this first — project identity and scope

Terra Machine is a **fresh project**, separate from [War Machine](https://github.com/ben0kay/war-machine). Its defining technical choice is **smooth, volumetric, editable voxel terrain from the beginning**. The immediate objective is **a beautiful, convincing wilderness test world**, with terrain, lighting, materials and vegetation as the development priority. Some genuine surface excavation, shallow tunnels and caves are important, but this is **not presently intended as a game about digging endlessly downward**. War Machine's heightmap-based world is a reference for design lessons and coding conventions, **not** an architecture to copy into this project.

The intended experience is an atmospheric, player-driven survival sandbox: wake in an unfamiliar, increasingly strange world; scavenge, gather, hunt, repair, explore and gradually become capable of travelling into more dangerous regions. Zombies may inhabit settlements, but the game should **not be defined primarily by zombie combat or gun progression**. Wildlife, weather, underground geology, machines, eerie sites and alien phenomena should give the wilderness purpose.

The player needs freedom **and** meaningful long-term discoveries. No exact lore, final game loop, release scope or endgame is settled yet.

### Why voxel terrain matters

The ground is part of the gameplay, not just a visual surface. A player might dig a trench, excavate a hillside shelter, follow an ore vein, open a tunnel into a natural cave, or uncover something artificial far beneath the forest. Terrain changes must eventually be persistent and compatible with nearby structures and world streaming. Smooth/organic surfaces are preferred over visible Minecraft-style cube terrain.

## Creative direction — hardcore survival simulation, immersion first (2026-10-30)

**North-star genre:** A grounded, hardcore **first-person wilderness survival simulator** in a persistent, physically interactive world. Aim for semi-believable, visually cohesive scenery and convincing systemic consequences, **not photorealism or full simulation of everything**. This is an evolving creative direction, not a mandate to implement all systems now.

### Three principal game references — distinct roles

- **Sons of the Forest — CURRENT PRIMARY REFERENCE for landscape, atmosphere and immersion.** Prioritize lush, natural-feeling, dense forest, mature canopy, filtered sunlight and shade, convincing ground cover, varied geology, undergrowth, organic clearings, believable scale, environmental audio/visual mood, and intimate first-person presence in the wilderness. Physical, tactile interactions such as later carrying wood, building and digging are useful long-term reference points. **Do not chase its graphical fidelity or copy its exact mechanics**: strive for a cohesive, semi-realistic look at a sustainable frame rate on modest hardware.
- **DayZ — FUTURE REFERENCE for unforgiving survival and exploration.** Scavenging, scarce supplies, equipment choices, injury/exposure, travel between locations, meaningful risk and consequential preparation. Possible vehicles and larger regions matter eventually; they are not today's implementation priorities.
- **Project Zomboid — FUTURE REFERENCE for interconnected simulation depth.** Systemic interactions among health, equipment, environment, construction, noise, enemies and persistence. Simulate nearby and meaningful events in detail; abstract, suspend or stream away distant systems. Do not introduce complexity simply for realism's sake.

**Immediate priority:** Make the **existing test landscape and forest feel right at ground level**, using Sons of the Forest as an *immersion reference*, before expanding survival systems. It must be compelling just to walk through the woods. Prioritize forest silhouette and density, natural vegetation distribution, varied terrain and geology, forest-floor material blending, atmospheric daylight/shadows, close-up plausibility, and consistent visual style. Use measured performance, instancing, simplified tree LODs and selective shadows to protect FPS. A beautiful 256 × 256 m benchmark within the current test world takes precedence over increasing map size.

**Gameplay identity over time:** DayZ-like harsh survival + Project Zomboid-like depth + Sons of the Forest-like first-person presence and physical world interaction, with Terra Machine's **smooth editable voxel ground** as a differentiator. Shallow trenches, animal/zombie pits and small shelters are desirable eventual uses of digging; material resistance increases with geology/depth. Preserve the existing excavation plan and avoid turning the game into an unrestricted deep mining simulator.

**Scope guardrails for Codex and future contributors:** This passage defines direction, not automatic authorization for new survival, AI, building, crafting, vehicles, networking or world-streaming gameplay systems. Keep the current working test scene, terrain adapter, environment assets and render/performance controls intact. Prioritize focused visual/performance passes, validate them in first person and only then expand gameplay. Large streamed landscapes and drivable vehicles remain **long-term aspirations**, not commitments to produce an enormous world before a polished playable slice.

## DG1 excavation proof — awaiting local validation

The test scene now includes a temporary shovel: press **1** to equip/unequip, then captured **LMB** requests one small rounded voxel subtraction within 3.5 m (1 m radius, 0.65 s cooldown). The HUD shows feedback; no held model or inventory is needed. F1 → EXCAVATION now scales dig amount, with 1.0 giving a shallow default scoop and higher values available for testing. Requests reject props, unsupported native APIs and unloaded edit bounds. Native terrain rebuilds mesh/collision asynchronously. This is an experiment, not accepted gameplay: compilation/contact/FPS still need local checks in [DG1_VALIDATION.md](WORLD/Terrain/Excavation/DG1_VALIDATION.md).

Depth hardness, exposed soil/rock materials and persistence are not implemented in this pass. Ground foliage now clears from accepted excavations; collidable scenery remains unchanged. Holes may disappear after chunk eviction/restart and surface shading may remain over cuts. Use an open area/F1 grass toggles to inspect. The existing [excavation plan](WORLD/Terrain/Excavation/EXCAVATION_IMPLEMENTATION_PLAN.md) marks DG1 Awaiting Validation; later phases remain separately authorised.

## Latest test-landscape pass

`WORLD/Scenes/test.tscn` now uses a deliberately shaped meadow → broad mountain shoulder → plateau/cliff composition with a mountain backdrop. Initial surface and shared slope map are prepared once; native SDF terrain streaming remains isolated. Tune `WORLD/Terrain/Landscape/Configuration/LandscapeMaster.tres`. See `WORLD/Terrain/Landscape/README.md` for checks and local acceptance still required. This replaces the small noise-hill surface for the test scene; it is not a general world generator.

The test scene also includes a player-centred grass trial with batched tiles and 20/45/60 m density/fade bands. Tune `WORLD/Vegetation/GrassScatter/Configuration/GrassMaster.tres`; F1 includes a grass visibility toggle. Local visual/FPS acceptance is pending.

## Current direction — VISUAL-FIRST WILDERNESS (priority for Codex)

**This section takes precedence over older speculative survival, mining and feature-phase ideas below.** The next objective is to make the existing test environment **look excellent at player eye level** before designing complex gameplay mechanics around it. Prioritize an Earth-like environment (forests, clearings, rock, hills, caves, natural vegetation and atmospheric lighting), not a hostile alien planet or zombie apocalypse. Mystery/sci-fi elements remain optional, later design choices.

### Immediate test-world target

- Focus on one deliberately polished **approximately 256 × 256 m visual benchmark area** (a target composition, not necessarily a replacement for the current 1024 m terrain bounds). It should feel hand-composed **even if generation is seeded and procedural**.
- Include varied terrain: a natural hillside, rocky outcrops/cliffs, a small valley or clearing, a densely wooded section and an identifiable cave mouth/underground pocket. Streams or water are optional; do not force them into the first pass.
- Achieve compelling **near-ground** materials: soil, exposed stone, grassy/mossy ground, sensible slope transitions and convincing scale. Avoid noisy repetitive hills and obvious texture tiling.
- Build a forest in layers: mature trees with irregular spacing and varying silhouette, saplings/understory, grass/ferns/shrubs, scattered stones, deadwood and forest-floor detail. Prefer mesh assets/instancing for plants and props rather than voxelizing individual trees.
- Tune Sky3D/sun/shadows/fog to produce attractive atmosphere **without using fog to hide missing geometry**. Forest canopy should create meaningful shade.
- Judge results by walking the terrain, looking across a valley, entering a cave and viewing terrain close-up—not screenshots from high overhead alone.
- Establish this quality bar **before** scaling toward a mega-world. Once an attractive patch is validated, turn its design into reproducible biome/placement rules across seamlessly streamed chunks, with LOD and performance checks.

### Excavation scope: real, but modest

Voxel terrain is still a **foundational requirement**, not an optional fake mining interaction. Plan for actual shallow ground removal, diggable hillsides, trenches, short tunnels and small underground shelters, with collision updating and terrain edits eventually persisting across saves. A player should be able to enter excavated space.

**Do not make deep unrestricted mining the initial gameplay objective.** The tentative balancing intention is to make digging **progressively, potentially exponentially, harder with depth**: shallow soil is relatively accessible; denser rock and deeper geology demand dramatically more effort, better tools or special circumstances. Treat this as a **tunable gameplay resistance curve**, not a baked-in voxel-engine limitation or a finalized numeric formula. Surface height versus absolute world elevation needs careful definition; prefer depth below the **original generated terrain surface** where appropriate, and consider rock/material type. Never silently assume a fixed world-Y threshold is the actual depth below ground.

Natural caves, pre-existing mines or other openings can provide deeper exploration **without requiring the player to excavate kilometres downward**. Keep digging costs/materials configurable and isolated from the native Voxel Tools integration. Do not add pickaxe skill trees, metallurgy, large-scale excavation machines, terrain-collapse physics or extensive survival UI now.

### Future digging hardness, noise and geology — design note

**Status: proposed mechanic, not implemented or authorized for implementation.** Terra Machine should allow useful **shallow terrain excavation** without making effortless deep tunnelling the dominant strategy. The player might dig a trench, post holes, a small cellar, an animal-trap pit or a short hillside shelter. Depth eventually exposes hard rock and makes further excavation increasingly impractical with hand tools.

| Ground/material | Intended behaviour | Typical tools / trade-offs |
| --- | --- | --- |
| Surface turf / topsoil | Relatively easy to remove; suitable for shallow trenches and foundations | Shovel; comparatively quiet |
| Dense soil, clay, gravel | Slower excavation and greater effort | Shovel or pick; more sustained activity |
| Weathered / fractured rock | Significantly tougher, especially as excavation continues | Pickaxe or stronger tools; repeated impacts create noise |
| Solid bedrock | Very high resistance, often practically undiggable by hand | Specialized equipment or alternate cave route; considerable noise/resource expense |

**Hardness model:** Use a **configurable, steeply increasing resistance curve** as a possible depth modifier (potentially exponential after a threshold), combined with the **actual geological material** and tool effectiveness. It must not be a uniform hard cutoff at one world-space Y level. Calculate depth relative to the **original local terrain surface** where appropriate; rock may be exposed at a cliff and deep soils may exist in valleys. Do not settle exact numbers, hardness factors, stamina costs or maximum depths until gameplay testing. Keep this tuning outside the voxel meshing/streaming engine.

**Noise and risk:** Excavation emits gameplay sound events. Quiet soil digging should be less conspicuous than striking rock; repeated pickaxe blows, drilling or machinery can create escalating sound/activity that **nearby zombies may investigate**. The goal is a strategic decision: quiet/slow, noisy/fast, upgrade tools, find a natural cave or give up. This is a future sound/AI integration, **not a requirement to build zombie perception during the visual-first stage**.

**Wildlife and construction uses:** Shallow dug pits/trenches may support later animal traps and shelter foundations; effective traps should depend on animal behaviour and placed components, not merely any hole. Ground near future building anchors may require protected support or event-driven checks when excavated. Full structural simulation, unrestricted deep mining and dynamic navigation rebuilding are **out of initial scope**.

**Core technical requirements when authorized:** Real voxel removal, proper refreshed collision, stable save/reload of modified terrain, material-aware edits, configurable excavation resistance, and minimal localized update cost. Natural caves and authored mine entrances can still provide **deep exploration** without deep player excavation. Prioritize a beautiful test landscape first.

### Excavation implementation handoff

The **documentation-only DG0 folder foundation** and a future **DG1–DG4 shovel/excavation plan** live at [WORLD/Terrain/Excavation/EXCAVATION_IMPLEMENTATION_PLAN.md](WORLD/Terrain/Excavation/EXCAVATION_IMPLEMENTATION_PLAN.md). Code is **not implemented** by the scaffolding commit. Future Codex work should preserve the existing native adapter boundary and begin with one shallow fake-shovel test (`1` to equip; LMB to dig) only when separately authorized.

### Practical sequencing and guardrails

1. **ENV0 — Visual terrain baseline:** refine hill/valley shape, terrain materials and scale using current working test scene; verify voxel/collision stability.
2. **ENV1 — Forest composition:** mature canopy trees, understory, ground-cover and rock detail, using instancing/streaming-aware placement.
3. **ENV2 — Atmosphere:** refine daylight, shadows, haze and visual depth; make the scene attractive from a first-person perspective.
4. **ENV3 — Caves and excavation proof:** one natural cavity and one bounded digging interaction; verify mesh/collision after edit and basic save/load in a separately authorized phase. Depth resistance can initially be a configuration-only or simple proof.
5. **ENV4 — Procedural repeatability and performance:** deterministic biome/placement rules, test adjacent chunks, remove seams/pop-in, and profile performance including terrain editing and vegetation.

These are **planning phases**, not permission to implement them all at once. Keep using `WORLD/Scenes/test.tscn`, `PLAYER/player.tscn` and the existing terrain-service/adapter boundary where possible; don't regenerate or replace existing working infrastructure without a specific reason. The existing V0/V1 roadmap farther below describes **technical capabilities**, while ENV stages describe the **current priority order and quality goals**. No enemies, crafting, vehicles, settlements or large-world systems should be built until separately approved.

## Current test — first pass

**Implemented in source, awaiting local compilation and runtime validation.** Open `WORLD/Scenes/test.tscn` and press F6, or F5 (now the main scene). Use the **Godot .NET editor** and .NET 8 SDK. The project SDK matches War Machine's `Godot.NET.Sdk/4.7.2`; verify your installed editor matches. The bundled Voxel Tools extension declares Godot 4.4.1 minimum; that is not proof of compatibility with every newer engine build.

The initial foundation provides smooth seeded hills, streamed terrain/collision, a reusable first-person capsule player, fixed 13:00 Sky3D lighting, a crosshair and FPS/loading text. The subsequently authorized [ground-and-fir visual test](WORLD/VISUAL_TEST_PASS.md) adds textured forest floor/grass/rock with normal/roughness maps and a twelve-fir preview ahead of spawn; first-person visual acceptance is pending. Terrain is instantiated at runtime through the adapter; opening the test scene in the editor does not yet preview the generated landscape. No digging, saving, dense grass, tree collision or gameplay resource materials in this pass.

| Scene | Owner |
| --- | --- |
| `WORLD/Scenes/test.tscn` | Composition, loading gate, test HUD and boundary recovery |
| `PLAYER/player.tscn` | Input, native capsule movement and first-person camera |
| `WORLD/Terrain/terrain.tscn` | Game-facing terrain service |
| `WORLD/Terrain/Integration/voxel_terrain.tscn` | Native Voxel Tools nodes, mesher, bounds and LOD |
| `WORLD/Environment/environment.tscn` | Sky3D and sun/moon lights |

Controls: **arrow keys** move, **Shift** sprint, **RMB** jump, mouse look, **Esc** releases cursor, **LMB** recaptures, **F4** respawns/retries loading; **F1** opens render debug. Spawn waits for an actual terrain collider; startup timeout remains safely suspended. The 1024m square test terrain has a 440m playable half-extent with automatic spawn recovery before its edge. F4 does not delete/regenerate the world.

## Render debugging

**F1** opens the optional render overlay in test.tscn after spawn. Toggle terrain textures/normal maps, terrain/tree visibility, tree shadows and unshaded firs. Run the five-case same-camera comparison and Copy results to separate ground, tree and shadow costs. The view stays locked during comparison. User reported the initial ground/tree visual test working, with roughly 300 → 125 FPS; the cause is **not measured yet**. See [debug controls and validation](DEBUG/Overlay/README.md).

## Configuration and architecture

- `PLAYER/Configuration/PlayerMaster.tres`: movement, jump, mouse sensitivity/inversion and FOV. Camera/collision geometry remains visible in the player scene.
- `WORLD/Configuration/WorldMaster.tres`: spawn probe, timeout and test safety bounds.
- `WORLD/Terrain/Integration/TerrainMaster.tres`: generator seed, noise and height. Streaming/bounds/LOD live on the native scene, which the adapter reads for viewer distance.
- Environment/time are configured in the environment scene. Stop/restart after configuration edits; live reconfiguration is not implemented.

Only `WORLD/Terrain/Integration/` may contain Voxel Tools types, dynamic names or API calls. Player and world composition use ordinary Godot transforms and `ITerrainService`. The adapter parents a native viewer under the supplied player transform, freeing it on unload; no scripted per-frame viewer synchronization. Generation and meshing run in the native addon, not managed per-voxel loops. Sky3D remains separately owned in Environment; vendor addon code is untouched.

See [WORLD/README.md](WORLD/README.md), [PLAYER/README.md](PLAYER/README.md) and [first-pass validation](WORLD/FIRST_PASS.md). Follow `CodePreferences.md` for code style. Its opening branch policy names **War Machine** specifically; War Machine reference reads use `implement-td0-td1`. Terra Machine currently develops on `main`; do not apply War Machine's branch name to this repository.

## Design pillars

1. **Beautiful world first, editable world underneath.** Establish a compelling forested test region before complex mechanics; still verify real shallow digging, caves, collision and eventual persistence before expanding far beyond the benchmark.
2. **Survival through practical choices.** Shelter, carrying capacity, resources, exposure and travel should create self-directed goals rather than constant quest markers.
3. **Many viable ways to live.** Scavenge a town, establish a woodland camp, hunt, salvage equipment, gather timber or prospect for minerals. Different paths should complement each other.
4. **World that feels alive.** Deer flee, bears can threaten, wolves may stalk, robots investigate, and distant strange activity makes the player cautious. Not every encounter must become a firefight.
5. **Mystery through exploration.** Abandoned mines, research facilities, machine patrols, anomalous geological deposits and alien-like discoveries gradually reveal a wider world.
6. **Modular content expansion.** Once a foundation is stable, AI-assisted additions should be mostly isolated: new materials, ore distributions, plants, creatures, tools, structures, events and locations.
7. **Atmosphere over photorealism.** Coherent semi-realistic/stylized assets, convincing forests, terrain, lighting and weather should look appealing while remaining feasible on modest hardware.

## The possible player experience

### Starting out: uncertain survival

A plausible opening is a quiet house or settlement with little explanation, limited supplies and no mandatory tutorial. The player may hide inside for days, ration food, cautiously search nearby houses or choose the woods immediately. Everyday objects should be useful, and noise, visibility, weather and wildlife should matter. Opening specifics remain undecided.

### Establishing independence: supplies and transport

The player gradually accumulates equipment. A backpack cannot realistically transport everything in one trip: full bags, water jugs, bulky tools and containers may need to be moved to a safehouse or vehicle in batches. Eventually finding a car, checking its condition, locating fuel, loading supplies and taking a risky trip could be an important emergent milestone. A failed vehicle or forgotten equipment cache should generate stories. Avoid inventory busywork by supporting containers and sensible bulk transfer rather than moving every tiny item separately.

### Expanding outward: resource and place diversity

| Area | Possible rewards and pressures |
| --- | --- |
| Towns and houses | Food, medicine, batteries, finished tools, scrap, usable shelter; zombies and confined-space risk |
| Forests, lakes and wilderness | Water, wood, plants, wildlife, refuge, renewable supplies; weather, isolation and predators |
| Caves, quarries and mines | Stone, ores, mineral veins, geological formations and buried passages; darkness and excavation risk |
| Industrial sites | Generators, motors, fuel, wiring, machinery and repair components; hazards and guarded facilities |
| Anomalous zones | Unknown materials, unusual organisms, alien structures, unexplained devices and new discoveries |

**Mining and scavenging must form one coherent economy.** A player might scavenge copper wiring from houses or mine and process copper at greater scale. Don't require smelting ore for a trivial part that could obviously be salvaged from an abandoned modern town. Geology should enable self-sufficiency, new materials, excavation and discovery—not a disconnected crafting minigame.

### Longer term: access, not just levels

An attractive progression idea is **Survive → Establish → Become self-sufficient/mobile → Explore → Discover → Influence the world**. Capability comes from knowledge, tools, transport, repair, appropriate clothing, protective equipment and access to difficult locations, not merely higher character statistics. Optional late-game objectives might involve restoring communications, reaching sealed facilities, investigating deep anomalous structures or deciding how to respond to a mysterious catastrophe. Keep the sandbox playable even if such objectives are completed. **No definitive plot or ending is approved.**

## World technology and architecture direction

**Candidate foundation:** [Voxel Tools by Zylann](https://github.com/Zylann/godot_voxel), already represented under `addons/zylann.voxel/`. Investigate which edition/version is actually included and verify compatibility before relying on it. Smooth voxel editing, LOD, collision, generation, chunk management and saving are important evaluation points.

**C# remains the primary language.** Since native GDExtension custom types may not provide directly generated C# bindings, prefer a **small, documented terrain interoperability adapter** around the addon. The rest of the game should call stable terrain operations without knowing extension-specific dynamic API details. Avoid per-voxel cross-language calls in hot loops. A limited bridge in another language is acceptable only if evidence shows it is needed.

Keep these responsibilities separate:

- **Generation:** Determine terrain density, geology, biomes, caves, resources and eventually prefab sites from a seed.
- **Rendering and collision:** Mesh visible voxel chunks, support LOD and maintain nearby physical terrain.
- **Streaming/relevance:** Manage what exists near active players; no assumption that rendering culling alone suspends simulation.
- **Persistence:** Record modified terrain, depleted deposits, placed objects, containers and later entities across sessions.
- **World content:** Add authored building/structure prefabs, vegetation, wildlife and discoverable locations using terrain-aware placement.
- **Gameplay:** Interaction, inventory, mining, tools and survival should depend on stable interfaces, not direct knowledge of internal chunk structures.

A **finite, seeded initial world** is sufficient. Do not require infinite worlds, total building destruction, physical structural collapse or full geology simulation for the first prototype. These may become separate experiments later.

## Potential creatures and threats

- **Deer:** simple grazing/wandering, alertness and flight; a useful first wildlife behaviour test.
- **Bears:** watch, avoid, investigate, defend territory, bluff or attack depending on circumstances; a meaningful wilderness threat.
- **Other wildlife:** wolves, boars and smaller animals could share selected needs and perception capabilities without identical AI.
- **Zombies:** basic wandering, detection, pursuit and melee; one local hazard family rather than the entire game.
- **Autonomous machines:** ground robots or simpler **flying surveillance drones**, with patrol, scanning, investigation, pursuit or alarms; attacking is optional.
- **Anomalous life:** specialized sensory and movement behaviours if exploration later calls for it.

Build shared capabilities **in response to concrete needs**, not a sprawling speculative all-purpose AI framework. Distant entities must be inexpensive or dormant. The world can imply ecology without fully simulating every animal offscreen.

## Visual target

**Grounded semi-realism**, not movie-quality photorealism. Natural-looking smooth terrain; believable ground textures; distinct tree silhouettes and layered forest-floor vegetation; intentional light and shadow; fog and weather that give depth without hiding technical problems. A compact, beautiful test patch is more valuable than a giant empty map. Use reusable procedural art where appropriate and imported GLBs/textures where they clearly improve quality. Maintain practical draw calls, LOD, streaming and sensible GPU budgets.

## Longer-term technical capability roadmap — not yet authorized for implementation

| Stage | Target outcome | Status |
| --- | --- | --- |
| **V0 — Compatibility** | Confirm Godot .NET/C# runtime, installed Voxel Tools edition, native extension loading, a minimal smooth terrain scene and collision | Proposed / unverified |
| **V1 — Excavation** | First-person movement and one tool that makes a true hole/tunnel in volumetric terrain with updated collision | Proposed |
| **V2 — Persistence** | Save the edited terrain; restart; find exactly the same hole; verify stable seed and chunk reload | Proposed |
| **V3 — Geology** | Soil/stone/ore identities, excavation resistance and limited material gathering | Proposed |
| **V4 — Wilderness** | One coherent outdoor scene with low-cost vegetation and terrain-aware placement | Proposed |
| **V5 — Survival loop** | One small scavenge/gather/store/use loop; prove different resources serve a purpose | Possible later slice |
| **V6 — World expansion** | More biomes, authored prefab sites, wildlife, persistence for placed objects, and exploration hooks | Long-term direction |

**Current visual acceptance milestone:** The player can walk through an attractive, varied, natural-looking test wilderness with convincing terrain materials, mature forest layers, realistic scale and lighting; performance and chunk behaviour remain acceptable. **Separate excavation acceptance milestone:** Shallow digging creates a traversable hole or short tunnel, collision updates and the change survives save/reload. Do not gate early landscape/art work on a complete mining system.

## AI development workflow

- **Codex / ChatGPT Work:** Build or integrate complex cross-system foundations—native voxel interoperability, world generation, streaming, saving, physics/terrain interaction—and keep them isolated and documented.
- **Regular ChatGPT:** Propose architecture and work on bounded additions after contracts are proven—an extra ore, plant, animal behaviour, tool, material or prefab—without sweeping rewrites.
- **Local testing:** Run Godot/.NET builds, shader/native-load checks and in-engine visual/performance acceptance; never equate committed source with a successful runtime test.

Prefer C#, modular directories and local READMEs, inspector-friendly settings, clear ownership, reusable helpers, stable scene contracts, performance-aware scheduling, and **one commit per explicitly authorized task**. Inactive systems should sleep. Do not introduce per-entity per-frame scans when event-driven or staggered logic would suffice. Avoid code/scene duplication when a small reusable module is sufficient.

### Immediate next decision

First validate the existing source implementation locally using `WORLD/Scenes/test.tscn`. Then propose the **ENV0 visual-quality pass** with concrete scene/material/terrain changes and a small screenshot-based review target. Preserve the working Voxel Tools adapter and Sky3D integration. Digging and persistence remain subsequent, separately approved experiments—not reasons to delay improving the test world's appearance.

---

**North star:** *Build a world worth surviving in, reshaping and exploring: a believable wilderness with meaningful resources, free-form player choices and discoveries that grow stranger the farther—and deeper—the player ventures.*
