# Repository Guidelines

## Project Structure & Module Organization
`Facet.sln` is the main solution. Shipping libraries live under `src/`: `Facet` is the source generator, `Facet.Attributes` holds public attributes, `Facet.Mapping*` covers mapping helpers, `Facet.Extensions*` adds LINQ and EF Core support, and `Facet.Dashboard` contains the dashboard package and HTML templates. Tests live under `test/Facet.Tests` with feature folders such as `UnitTests/`, `DiagnosticTests/`, `TestModels/`, and `Utilities/`. Keep long-form docs in `docs/` and repo images in `assets/`.

## Build, Test, and Development Commands
Use the repo root unless noted otherwise.

- `dotnet restore Facet.sln` restores central package-managed dependencies.
- `dotnet build Facet.sln -c Debug` builds all projects for local development.
- `dotnet test test/Facet.Tests -c Debug` runs the xUnit suite.
- `dotnet build test/FacetTest.sln` builds the isolated test solution used by the test scripts.
- `dotnet pack src/Facet/Facet.csproj -c Release` creates a NuGet package; use `Release` so symbols and SourceLink settings match repo defaults.
- `test\\run-tests.bat` runs the Windows-friendly test flow described in `test/README.md`.

## Coding Style & Naming Conventions
Follow existing C# conventions: 4-space indentation, file-scoped or concise namespace declarations where already used, `PascalCase` for types and public members, `camelCase` for locals/parameters, and one top-level type per file when practical. Nullable reference types and XML docs are enabled centrally in `Directory.Build.props`; do not disable them casually. Keep generator, analyzer, and extension code separated by folder and project boundary.

## Testing Guidelines
Tests use xUnit with FluentAssertions and EF Core in-memory/SQLite helpers. Name tests `MethodUnderTest_ShouldExpectedBehavior_WhenCondition`. Place analyzer diagnostics under `DiagnosticTests/` and runtime behavior checks under `UnitTests/`. When changing generation or mapping behavior, add or update focused tests before merging.

## Commit & Pull Request Guidelines
Recent history favors short imperative subjects, often with release or dependency bump wording such as `fix TypeAnalyzer` or `Bump version to 5.8.3`. Keep commits scoped to one change. PRs should summarize behavior changes, mention affected packages/projects, link related issues, and note the commands you ran. Include screenshots only when changing dashboard output or generated UI artifacts.
