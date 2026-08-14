---
name: reactiveui-mcp
description: Use the ReactiveUI MCP server for source-backed ReactiveUI package selection, architecture guidance, plan review, migration planning, repository capability discovery, and solution scaffolding. Trigger for new or existing .NET ReactiveUI projects across WPF, WinForms, WinUI, MAUI, Blazor, Avalonia, Uno, or AndroidX; when comparing ecosystem libraries; when choosing source generators or ReactiveUI.Primitives variants; or when reviewing a proposed implementation.
---

# ReactiveUI MCP

Use the MCP catalog as the source of truth for ReactiveUI ecosystem guidance. Inspect the user's repository and constraints first, then ask the server only for the information needed for the current decision.

## Follow the core workflow

1. Inspect the local solution, target frameworks, UI platform, current packages, and existing patterns before requesting guidance.
2. State the platform, application kind, requested features, constraints, and existing libraries explicitly in MCP calls.
3. Discover stable catalog or repository ids before retrieving or comparing detailed entries. Never invent ids or package names.
4. Reconcile MCP guidance with the actual repository, installed package versions, public API constraints, and user requirements.
5. Keep review and recommendation requests read-only. Generate or change files only when the user has authorized implementation.
6. After implementation, build and run the repository's tests. Fix warnings in code or configuration; do not suppress them.

## Route the task

- Discover packages and capabilities with `reactiveui_catalog_list` or `reactiveui_catalog_search`, then retrieve the selected stable id with `reactiveui_catalog_get`.
- Inspect capabilities found in ReactiveUI source repositories with `reactiveui_repository_inventory_list`, then retrieve one entry with `reactiveui_repository_inventory_get`.
- Choose a package set and architecture with `reactiveui_recommend`. Supply the complete platform, application kind, features, constraints, and existing libraries.
- Review a proposed implementation or package plan with `reactiveui_review_plan`. Pass the complete plan text plus known platform and library context.
- Compare two known catalog entries with `reactiveui_compare`. Search first when either stable id is uncertain.
- Prepare implementation instructions with `reactiveui_scaffold_prompt` or a structured project plan with `reactiveui_project_blueprint`.
- Plan an upgrade with `reactiveui_migration_plan`. Include the project type, current packages, upgrade goals, and compatibility constraints.
- Design or generate a multi-endpoint solution with `reactiveui_create_solution`; follow the guarded wizard workflow below.
- Use the server prompts `create_reactiveui_scaffold`, `create_reactiveui_test_project`, and `migrate_legacy_reactiveui_project` when the host exposes MCP prompts and the request matches one directly.
- Read `reactiveui://catalog`, `reactiveui://source-repositories`, or `reactiveui://best-practices/project-generation` when a host makes MCP resources easier to consume than the equivalent tools. Use `reactiveui://ecosystem/{id}` only with a discovered id.

Tool names may be namespaced by the MCP host. Match the stable suffix shown above when a prefix is present.

## Guard package and implementation choices

- Prefer a relevant source generator when the required behavior is known at compile time and the repository inventory confirms support. Use manual or runtime wiring for unsupported or genuinely dynamic cases.
- Prefer the lean `ReactiveUI.Primitives` package variant that satisfies the requirement. Select a `.Reactive` variant only when full System.Reactive compatibility is required, such as `Unit`, `IScheduler`, `Subject`, namespace, or public API compatibility.
- Do not mix Fody and source-generator implementations on the same code path. Convert incrementally, inspect generator diagnostics, and build after each migration slice.
- Generated output from one source generator is not generally available as input to another during the same compilation. Avoid cross-generator designs that depend on generated members or attributes.
- Preserve existing public APIs and architectural constraints unless the user explicitly approves a breaking change.
- Treat returned recommendations as source-backed guidance, not permission to mutate the project. Check actual project files before applying them.
- Include the server's rationale, warnings, exclusions, and migration notes in the resulting plan instead of reporting only package names.
- If a response reports an unknown id or missing detail, search or list once, retrieve the matching entry, and continue. Do not repeat failing calls without changing the input.

## Run the solution wizard safely

Call `reactiveui_create_solution` in sequence and retain the selections between calls:

1. `1/start`: solution name and UI endpoints.
2. `2/di`: Splat dependency-injection provider.
3. `3/features`: companion ReactiveUI features.
4. `4/storage`: settings store.
5. `5/application`: application features.
6. `6/views`: theme colors, validation, and endpoint-to-view mappings.
7. `7/blueprint`: inspect the proposed structure.
8. `8/migration`: inspect migration considerations.
9. `9/complete`: finalize the plan.

Always pass a recognized explicit step. Keep `generateFiles` false while exploring. Set it to true only at `9/complete`, only after the user has asked for generation, and only with an explicit, inspected, dedicated new or empty `outputRoot`. Never point generation at an existing repository or a broad workspace root because matching paths can be overwritten. After generation, inspect the created files before restoring, building, or testing them.

## Produce an actionable result

Summarize the decision, selected packages or patterns, rationale, constraints, warnings, and next verification step. When code changes are in scope, implement the smallest coherent change and report the exact build and test evidence.
