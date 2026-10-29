# TERRA MACHINE

> **Working title:** Terra Machine  
> **Direction:** First-person, semi-realistic, voxel-based wilderness survival and anomalous exploration.  
> **Status (2026-10-29):** Early Godot project / technical foundation. The game vision below is **proposed direction**, not a list of implemented systems or blanket authorization to build them.

## Read this first — project identity and scope

Terra Machine is a **fresh project**, separate from [War Machine](https://github.com/ben0kay/war-machine). Its defining technical choice is **smooth, volumetric, editable voxel terrain from the beginning**. The player should eventually be able to dig into hills, tunnel beneath the surface, excavate resources, make shelters and find underground cavities. War Machine's heightmap-based world is a reference for design lessons and coding conventions, **not** an architecture to copy into this project.

The intended experience is an atmospheric, player-driven survival sandbox: wake in an unfamiliar, increasingly strange world; scavenge, gather, hunt, repair, explore and gradually become capable of travelling into more dangerous regions. Zombies may inhabit settlements, but the game should **not be defined primarily by zombie combat or gun progression**. Wildlife, weather, underground geology, machines, eerie sites and alien phenomena should give the wilderness purpose.

The player needs freedom **and** meaningful long-term discoveries. No exact lore, final game loop, release scope or endgame is settled yet.

### Why voxel terrain matters

The ground is part of the gameplay, not just a visual surface. A player might dig a trench, excavate a hillside shelter, follow an ore vein, open a tunnel into a natural cave, or uncover something artificial far beneath the forest. Terrain changes must eventually be persistent and compatible with nearby structures and world streaming. Smooth/organic surfaces are preferred over visible Minecraft-style cube terrain.

## Current test — first pass

**Implemented in source, awaiting local compilation and runtime validation.** Open `WORLD/Scenes/test.tscn` and press F6, or F5 (now the main scene). Use the **Godot .NET editor** and .NET 8 SDK. The project SDK matches War Machine's `Godot.NET.Sdk/4.7.2`; verify your installed editor matches. The bundled Voxel Tools extension declares Godot 4.4.1 minimum; that is not proof of compatibility with every newer engine build.

This pass provides smooth seeded hills with slope-based green/rock shading, streamed terrain/collision, a reusable first-person capsule player, fixed 13:00 Sky3D lighting, a crosshair and FPS/loading text. Terrain is instantiated at runtime through the adapter; opening the test scene in the editor does not yet preview the generated landscape. No digging, saving, trees or gameplay resource materials in this pass.

| Scene | Owner |
| --- | --- |
| `WORLD/Scenes/test.tscn` | Composition, loading gate, test HUD and boundary recovery |
| `PLAYER/player.tscn` | Input, native capsule movement and first-person camera |
| `WORLD/Terrain/terrain.tscn` | Game-facing terrain service |
| `WORLD/Terrain/Integration/voxel_terrain.tscn` | Native Voxel Tools nodes, mesher, bounds and LOD |
| `WORLD/Environment/environment.tscn` | Sky3D and sun/moon lights |

Controls: **arrows or WASD** move, **Shift** sprint, **RMB** jump, mouse look, **Esc** releases cursor, **LMB** recaptures, **F4** respawns/retries loading. Spawn waits for an actual terrain collider; startup timeout remains safely suspended. The 1024m square test terrain has a 440m playable half-extent with automatic spawn recovery before its edge. F4 does not delete/regenerate the world.

## Configuration and architecture

- `PLAYER/Configuration/PlayerMaster.tres`: movement, jump, mouse sensitivity/inversion and FOV. Camera/collision geometry remains visible in the player scene.
- `WORLD/Configuration/WorldMaster.tres`: spawn probe, timeout and test safety bounds.
- `WORLD/Terrain/Integration/TerrainMaster.tres`: generator seed, noise and height. Streaming/bounds/LOD live on the native scene, which the adapter reads for viewer distance.
- Environment/time are configured in the environment scene. Stop/restart after configuration edits; live reconfiguration is not implemented.

Only `WORLD/Terrain/Integration/` may contain Voxel Tools types, dynamic names or API calls. Player and world composition use ordinary Godot transforms and `ITerrainService`. The adapter parents a native viewer under the supplied player transform, freeing it on unload; no scripted per-frame viewer synchronization. Generation and meshing run in the native addon, not managed per-voxel loops. Sky3D remains separately owned in Environment; vendor addon code is untouched.

See [WORLD/README.md](WORLD/README.md), [PLAYER/README.md](PLAYER/README.md) and [first-pass validation](WORLD/FIRST_PASS.md). Follow `CodePreferences.md` for code style. Its opening branch policy names **War Machine** specifically; War Machine reference reads use `implement-td0-td1`. Terra Machine currently develops on `main`; do not apply War Machine's branch name to this repository.

## Design pillars

1. **Editable world first.** Real digging, terrain collision, caves and saved excavations should be proven before building a large survival game around them.
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

## Proposed development sequence — not yet authorized for implementation

| Stage | Target outcome | Status |
| --- | --- | --- |
| **V0 — Compatibility** | Confirm Godot .NET/C# runtime, installed Voxel Tools edition, native extension loading, a minimal smooth terrain scene and collision | Proposed / unverified |
| **V1 — Excavation** | First-person movement and one tool that makes a true hole/tunnel in volumetric terrain with updated collision | Proposed |
| **V2 — Persistence** | Save the edited terrain; restart; find exactly the same hole; verify stable seed and chunk reload | Proposed |
| **V3 — Geology** | Soil/stone/ore identities, excavation resistance and limited material gathering | Proposed |
| **V4 — Wilderness** | One coherent outdoor scene with low-cost vegetation and terrain-aware placement | Proposed |
| **V5 — Survival loop** | One small scavenge/gather/store/use loop; prove different resources serve a purpose | Possible later slice |
| **V6 — World expansion** | More biomes, authored prefab sites, wildlife, persistence for placed objects, and exploration hooks | Long-term direction |

**First acceptance milestone:** A player walks up to an ordinary-looking hillside, digs a traversable tunnel, exits/reopens the game, and finds the tunnel unchanged. Smooth terrain, correct collision and tested C# interop come before extensive content.

## AI development workflow

- **Codex / ChatGPT Work:** Build or integrate complex cross-system foundations—native voxel interoperability, world generation, streaming, saving, physics/terrain interaction—and keep them isolated and documented.
- **Regular ChatGPT:** Propose architecture and work on bounded additions after contracts are proven—an extra ore, plant, animal behaviour, tool, material or prefab—without sweeping rewrites.
- **Local testing:** Run Godot/.NET builds, shader/native-load checks and in-engine visual/performance acceptance; never equate committed source with a successful runtime test.

Prefer C#, modular directories and local READMEs, inspector-friendly settings, clear ownership, reusable helpers, stable scene contracts, performance-aware scheduling, and **one commit per explicitly authorized task**. Inactive systems should sleep. Do not introduce per-entity per-frame scans when event-driven or staggered logic would suffice. Avoid code/scene duplication when a small reusable module is sufficient.

### Immediate next decision

The separately authorized first visual pass is implemented in source. Build and run `WORLD/Scenes/test.tscn` locally to validate C# setup, addon compatibility, terrain collision and lighting. Digging and persistence remain subsequent stages requiring separate approval.

---

**North star:** *Build a world worth surviving in, reshaping and exploring: a believable wilderness with meaningful resources, free-form player choices and discoveries that grow stranger the farther—and deeper—the player ventures.*
