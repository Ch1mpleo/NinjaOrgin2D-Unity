# 2D RPG Course (Unity)

A Unity project for a 2D RPG course. This repository tracks source assets and project settings while ignoring local build/cache artifacts.

## Requirements
- Unity (open with the version this project was created in)
- Git
- Git LFS (recommended for large binary assets)

## Getting Started
1. Clone the repository
2. Install Git LFS and enable tracking (recommended):
```bash
# one-time on your machine
git lfs install
# common Unity binary types; adjust as needed for your project
git lfs track "*.png" "*.jpg" "*.jpeg" "*.psd" "*.tga" "*.wav" "*.mp3" "*.ogg" "*.ttf" "*.otf" "*.fbx" "*.anim" "*.controller" "*.prefab" "*.unity" "*.mat" "*.asset" "*.shadergraph" "*.sd" "*.mp4"
# commit any .gitattributes changes
git add .gitattributes
git commit -m "chore: enable Git LFS for common Unity assets"
```
3. Open the project folder in Unity Hub (project root is the folder containing `Assets`, `ProjectSettings`, and `Packages`). Unity will import assets on first open.

## Project Structure
- `Assets/`: Game code, scenes, prefabs, animations, sprites, scripts
- `ProjectSettings/`: Unity project configuration
- `Packages/`: Managed by Unity Package Manager

## Build
- In Unity: File → Build Settings… → select target platform → Build.
- Do not commit the `Build`/`Builds` output folders (they are ignored by `.gitignore`).

## Notes
- `.gitignore` is tailored for Unity and IDE caches; it keeps source assets and configs tracked.
- If you add new large binary types, remember to add them to Git LFS tracking.
- Avoid committing files under `Library/`, `Temp/`, `Logs/`, and other generated caches.
