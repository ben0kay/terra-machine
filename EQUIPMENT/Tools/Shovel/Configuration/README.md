ShovelMaster.tres is used by the player prefab. ShovelSettings exposes Reach (3.5 m), Radius (1 m), SurfaceInset (-0.75 m; outward centre, 0.25 m penetration at default amount 1) and CooldownSeconds (0.65 s). ExcavationService enforces its own smaller work limits before calling the adapter. Restart after editing this resource; no live polling. DG2 material/tool effectiveness remains unimplemented.

Negative inset is intentional: only a shallow cap intersects the ground. F1's amount slider scales cap depth in ExcavationService without modifying the saved master.
