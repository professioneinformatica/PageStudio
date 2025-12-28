---
apply: always
---

# Development Guidelines for PageStudio

To maintain code quality and consistency across the PageStudio project, follow these guidelines.

## Project Configuration

### 1. Code Style and Formatting

- **EditorConfig**: Rider is configured to automatically respect the `.editorconfig` file in the root directory. This
  includes:
    - 4-space indentation.
    - File-scoped namespaces for C#.
    - PascalCase for most identifiers and `I` prefix for interfaces.
    - Expression-bodied members where appropriate.

### 2. PageStudio Solution Structure

- Use the `PageStudio.sln` as the primary entry point.
- `PageStudio.Web` and `PageStudio.Web.Client`: Runs the ASP.NET Core host and the Blazor Client.
- `PageStudio.Desktop`: Runs the MAUI application (ensure the correct target device/simulator is selected).
- `PageStudio.Tests`: Run all tests using Rider's Unit Test Runner.
- `PageStudio.Core`: The main library containing all business logic and shared types.

## Naming and Architecture

- Always favor **Composition over Inheritance** for Page Elements.
- New Page Elements should implement `IPageElement` and ideally inherit from `BasePageElement`.
- Keep the `PageStudio.Core` project free of UI-specific dependencies (e.g., no `Microsoft.AspNetCore.*` unless strictly
  necessary for core logic).
