# Environment

`environment.tscn` is a reusable Sky3D setup with explicit SunLight, MoonLight, SkyDome and TimeOfDay children. No demo city or demo camera is imported. Both editor and game time progression are disabled at 13:00, screen-space fog is off for clear terrain inspection. Sky3D still performs its own native/GDScript shader/cloud work; the atmosphere is not claimed to have zero rendering cost.

Adjust Sky3D/SkyDome/TimeOfDay and the Environment resource in this scene. No new C# sky manager, weather or day/night gameplay is introduced. Vendor files stay unchanged. Validate daylight, terrain shading, sun shadows and cloud appearance locally.
