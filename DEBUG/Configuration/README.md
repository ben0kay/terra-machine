# Debug defaults

DebugMaster.tres uses DebugSettings. Enabled=false disables opening the optional overlay. WarmupSeconds=2 and SampleSeconds=3 give approximately 25 seconds for the five cases. ReadoutInterval=0.25 only runs while the panel/comparison is active. Runtime toggles do not modify this resource or save to disk.

FpsCap defaults to 60 and is applied on overlay startup; F1 can remove it temporarily. ShowLagometer controls initial visibility of the optional frame-time graph. F2 toggles graph visibility during play. VSync settings are unchanged.
