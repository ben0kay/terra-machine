# Vegetation

`TreePreview/` owns the bounded twelve-fir/four-pine visual group. `GrassScatter/` owns the player-centred, distance-faded grass trial, batched into nearby tiles. Mesh/texture assets remain under ASSETS/Vegetation; world placement/settings belong here. Both use game-owned terrain interfaces with no Voxel Tools dependencies. Trees sleep after placement; grass runs bounded placement on travel and retains a small crossing timer. See each feature README for lifecycle, tuning and acceptance checks.
