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

## Current repository — verified source snapshot

| Area | Current evidence | Status / important distinction |
| --- | --- | --- |
| Engine | `project.godot` advertises Godot **4.7**, **Forward Plus**, Windows **D3D12** and **Jolt Physics** | Verify the locally installed Godot .NET version; the project config alone does not prove runtime support |
| Language | `project.godot` names the `terra-machine` .NET assembly, but the current feature list does **not** declare `C#` | **C# is the intended primary gameplay language**; first establish and test a working Godot .NET/C# setup |
| Voxel addon | `addons/zylann.voxel/` exists | Files being present does **not** prove the native extension loads or that digging, saving and LOD work with Godot 4.7 |
| Atmosphere addon | `addons/sky_3d/` exists, alongside `demo/Sky3DDemo.tscn` | Bundled reference content, not proof of an integrated gameplay environment |
| Gameplay | No established player/voxel gameplay scene was confirmed in the root inspection | Do not claim movement, mining, world generation, persistence, physics or streaming are implemented and tested |

**Branch/notes caution:** This repository currently uses `main`. The copied root `CodePreferences.md` still begins with **War Machine-specific** instructions for `ben0kay/war-machine` and branch `implement-td0-td1`, plus paths and build names from that project. Those references are **not** Terra Machine's active branch or directory structure. Follow its general code quality/performance conventions where applicable, but audit and correct the cross-project references in a separately authorized documentation task. Do not edit War Machine when instructed to work on Terra Machine.

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

Before substantial gameplay work, verify the exact installed Voxel Tools build and Godot compatibility, then develop **V0 only** as an isolated technical test. Subsequent stages require separate review and approval.

---

**North star:** *Build a world worth surviving in, reshaping and exploring: a believable wilderness with meaningful resources, free-form player choices and discoveries that grow stranger the farther—and deeper—the player ventures.*
