# Player equipment

**Reserved; not implemented.** The future equipment/selection controller will own selected slot, switching on hotkey `1` and forwarding discrete tool activation. It will not own terrain editing, material hardness, save/load or native addon APIs.

Keep existing player movement, input capture and debug-overlay blocking intact. Before implementing, inspect `PlayerInput.cs`, `PlayerController.cs` and the project Input Map; avoid conflicts with existing controls. Do not build an entire inventory/hotbar architecture for one temporary shovel.
