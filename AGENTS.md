# Repository Guidelines

## Project Structure & Module Organization
This repository is a Unity 2022.3 project (`ProjectSettings/ProjectVersion.txt` -> `2022.3.62f3c1`). Core gameplay code lives in `Assets/Scripts`:
- `Counter/`, `UI/`, `Interface/` for runtime systems
- `TestNewWorkUI/` for ad-hoc networking experiments
- manager classes at `Assets/Scripts` root (for example `GameManager.cs`, `DeliverManager.cs`)

Content and data are split across:
- `Assets/Scenes` (`MainMenu`, `LoadingScene`, `GameScene`)
- `Assets/Prefabs` and `Assets/_Assets/PrefabsVisuals`
- `Assets/ScriptsObj` for ScriptableObject assets (recipes, kitchen objects, audio)
- `Packages/manifest.json` for package dependencies (URP, NGO, Input System, Test Framework)

Do not commit generated Unity folders: `Library/`, `Temp/`, `Logs/`, `obj/`.

## Build, Test, and Development Commands
Open locally with Unity Hub using editor `2022.3.62f3c1`, then run from `Assets/Scenes/GameScene.unity`.

Example Windows batch commands:
```powershell
Unity.exe -projectPath "E:\UnityLearn\Kitchen" -buildWindows64Player "_Build\Kitchen.exe" -quit -batchmode
Unity.exe -projectPath "E:\UnityLearn\Kitchen" -runTests -testPlatform EditMode -testResults "TestResults\editmode.xml" -quit -batchmode
Unity.exe -projectPath "E:\UnityLearn\Kitchen" -runTests -testPlatform PlayMode -testResults "TestResults\playmode.xml" -quit -batchmode
```
First command builds; second and third run EditMode/PlayMode tests.

## Coding Style & Naming Conventions
Use C# with 4-space indentation and braces on new lines (current codebase style). Follow Unity naming conventions:
- `PascalCase` for classes, methods, properties, enums
- `camelCase` for private fields and locals
- Prefix serialized private fields with `[SerializeField] private`
- Keep one `MonoBehaviour`/`NetworkBehaviour` per file; filename must match class name

Keep comments short and intent-focused. Preserve `.meta` files for every moved/renamed asset.

## Testing Guidelines
Unity Test Framework is installed, but dedicated test assemblies are minimal today. Add new tests under:
- `Assets/Tests/EditMode`
- `Assets/Tests/PlayMode`

Name test files `FeatureNameTests.cs` and test methods `MethodName_Condition_ExpectedResult`.
Prioritize delivery flow, recipe matching, and NGO sync edge cases.

## Commit & Pull Request Guidelines
Recent history uses short, task-focused messages (often Chinese), for example: `完成菜品UI显示` and `修复餐盘元素bug`.

Use this format consistently:
- `<scope>: <change summary>` (e.g., `network: sync late-join recipe list`)
- One logical change per commit

PRs should include:
- what changed and why
- related issue/task link
- test evidence (manual steps or test output)
- screenshots/video for UI or scene changes
