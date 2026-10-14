# WhoDunBit — Initial Project Configuration

**Purpose:** Record the initial Unity configuration, every adjustment from the created project and the rationale for retained defaults.  
**Version:** 1.0  
**Created:** 14 October 2026  
**Updated:** 14 October 2026  
**Status:** Active prototype baseline  
**Related documents:** [Game Design Document](Game%20Design%20Document.md), [Technical Design Document](Technical%20Design%20Document.md), [Documentation index](README.md)

## Project baseline

The Unity project is in `WhoDunBit Project/`, using Unity 6.3 LTS / 6000.3.14f1 and Universal Render Pipeline 17.3.0. Windows x86-64 is the active build target. The PC quality level and default graphics settings both use `Assets/Settings/PC_RPAsset.asset`, with the template's Forward+ renderer.

These settings establish a starting point for the GDD's grounded, moderately detailed, readable presentation and responsive multiplayer FPS. Rendering values are provisional prototype choices, not measured performance guarantees or final user-facing quality presets.

## Changes from the created project

All Unity settings were changed through the running Editor and saved using Unity APIs.

| Location / setting | Before | After | Rationale and practical effect |
|---|---|---|---|
| Player > Product Name | `WhoDunBit Project` | `WhoDunBit` | Uses the game's name in application identity and the player window. This also changes the default product-derived persistent-data path; no game save system exists yet. |
| Player > Run In Background | Disabled | Enabled | Allows the application to continue updating after losing focus. This is necessary for a listen-server host that switches windows, and useful when testing multiple clients. Multiplayer still needs to be implemented and validated. |
| Player > Resizable Window | Disabled | Enabled | Allows resizing in windowed mode for different desktop layouts and local client testing. The existing borderless fullscreen startup mode remains selected. |
| Player > Default Screen Width | 1024 | 1280 | Establishes a practical 16:9 default when windowed settings are used, paired with the height below. Borderless fullscreen continues to use desktop dimensions; this does not force fullscreen rendering to 720p. |
| Player > Default Screen Height | 768 | 720 | Completes the 1280×720 windowed default. Saved user resolution preferences can override defaults. |
| Editor > Project Generation > Root Namespace | Empty | `WhoDunBit` | Gives generated C# projects a consistent root namespace. It does not rewrite existing scripts or automatically impose namespace declarations on every new script. |
| PC URP asset > Anti Aliasing (MSAA) | Disabled / 1 sample | 2× | Provides modest geometric edge smoothing without a temporal history. This adds GPU/memory cost compared with disabled MSAA; it is a provisional clarity/quality trade-off rather than a speed improvement. The existing main camera allows MSAA. |
| PC URP asset > Opaque Texture | Enabled | Disabled | Stops requesting a scene-colour copy globally before transparent rendering. No authored effect currently needs it. Future refraction or distortion effects may require enabling it globally or for a particular camera. |
| PC URP asset > Shadow Cascade Count | 4 | 2 | Reduces directional-shadow rendering work for the initial moderate-detail baseline. It trades some shadow detail/distribution for lower cost. The existing two-cascade split remains 0.25 and maximum shadow distance remains 50 metres. |
| PC URP asset > Soft Shadow Quality | High | Medium | Reduces shadow-filtering work while retaining soft shadows. Shadow edges receive less detailed filtering; final tuning requires representative maps and target hardware. |

The saved Unity settings changes are limited to `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorSettings.asset` and `Assets/Settings/PC_RPAsset.asset`.

## Existing settings retained

| Setting | Retained value / decision | Reason |
|---|---|---|
| Editor version | 6000.3.14f1 | Matches the documented engine baseline; recorded in `ProjectSettings/ProjectVersion.txt`. |
| Build target | Windows x86-64 | Matches the initial Windows PC platform. |
| Rendering | URP 17.3.0, Forward+, Render Graph | Retains the created project's modern rendering foundation. No rendering-path migration is needed. |
| Colour / HDR | Linear colour; HDR enabled | Supports believable lighting and room for deliberate lighting effects. HDR rendering is separate from HDR monitor output. |
| Depth texture / SSAO | Depth texture and template SSAO retained | Preserves the template's depth-based rendering/ambient occlusion; these need representative visual and performance evaluation. |
| Render scale / batching | 1.0; SRP Batcher enabled | Keeps native-resolution clarity and the existing batching optimisation. |
| Input | Input System package 1.19.0; new input backend only | Already suitable for future configurable controls. No gameplay bindings were finalised. |
| Asset serialization | Force Text | Keeps supported assets and scenes readable in Git diffs. |
| Version control mode | Visible Meta Files | Asset GUIDs and references must travel with the assets. |
| Play Mode reloads | Domain and scene reload enabled | Avoids relying on static-state cleanup before game systems exist. |
| Physics | 0.02-second fixed timestep; default gravity, solver and collision matrix | Avoids committing to simulation/network rates or collision policies before the controller, props and networking exist. Fixed physics timing is not a selected networking tick rate. |
| Frame limiting | VSync remains off; no custom frame-cap code added | Final frame pacing and settings need a runtime options system and hardware targets. |
| Build scripting | Existing Mono backend and .NET Standard compatibility | Avoids selecting production/mod scripting constraints prematurely. Backend and mod trust decisions remain open. |
| Product metadata | Version 0.1.0; existing company/application identifier | No publisher identity has been specified. These placeholders need review before distribution. |
| Pipeline tooling | Editor integration retained; Pipeline server disabled in player builds | Preserves local development access with the package's existing build behaviour. |
| Content and packages | Existing template assets, scene and package versions retained | No gameplay was implemented, packages removed or sample content deleted as part of configuration. |

## Git exclusions

`WhoDunBit Project/.gitignore` is at the Unity project root, so anchored rules apply correctly despite the project being nested inside the Git repository.

- Exclude `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, Unity temporary/export folders, logs and crash reports. These contain generated or machine-local state.
- Exclude `Build`, `Builds`, Android application outputs, macOS application bundles, profiler memory captures and recordings. Build output should go in the ignored build directories; source plugins and DLLs remain eligible for Git.
- Exclude generated root C# projects/solutions, including Unity 6 `.slnx` files, plus IDE caches and personal settings. The existing shared `.vsconfig` remains eligible for Git.
- Exclude temporary files, Blender backup files and their metadata, and common operating-system metadata.
- Exclude regenerable Rider integration, UI Toolkit schemas, test scenes, scene dependency caches and Visual Scripting caches.
- Exclude generated Addressables output and temporary folders. Authored Addressables definitions and content-update state remain eligible for version control.
- Keep authored assets, ordinary `.meta` files, `Packages/manifest.json`, `Packages/packages-lock.json` and `ProjectSettings`. The ignore file does not blanket-ignore DLLs, plugins, ZIPs or Unity packages that might be authored inputs.

No existing tracked files required removal from Git's index. This setup does not install Git LFS or alter repository remotes.

## Validation

The initial settings were snapshotted before modification and compared with saved files afterwards. All ten setting changes above were verified through the live Editor and on disk. Git ignore checks confirmed generated files are excluded and representative source assets, metadata, package files and project settings remain eligible.

The existing template scene entered Play Mode and advanced for more than 1,000 frames using the PC quality level and PC URP asset, with its main camera present. Unity reported no compilation failures, console errors or warnings. The Editor was then returned to Edit Mode. This smoke check does not validate multiplayer, shipping performance, gameplay feel or standalone window behaviour before those systems/builds exist.

## Revision history

| Date | Revision |
|---|---|
| 14 October 2026 | 1.0 — Applied and documented the initial desktop configuration and Unity Git exclusions. |
