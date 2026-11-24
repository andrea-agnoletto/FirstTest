# Repository Guidelines

## Project Structure & Module Organization

- `src/TodoList.Api` contains the ASP.NET Core minimal API, request models, and JSON storage service. Runtime data lives in `Data/todos.json`.
- `src/TodoList.Web` contains the Blazor WebAssembly client: `Pages/` and `Layout/` hold Razor components, `Services/` contains the API client, and `wwwroot/` holds static assets and client configuration.
- `src/TodoList.Tests` contains NUnit storage and HTTP endpoint tests. `TodoList.sln` groups the projects.

## Build, Test, and Development Commands

Install the .NET 11 SDK, then run these commands from the repository root:

```powershell
dotnet build .\TodoList.sln
dotnet test .\src\TodoList.Tests\TodoList.Tests.csproj
dotnet run --project .\src\TodoList.Api
dotnet run --project .\src\TodoList.Web
```

The first command builds the solution; the second runs the tests. Run the last two in separate terminals to start the API and client. The defaults are `https://localhost:7001` and `https://localhost:5002`, respectively. VS Code also offers the `TODO: Start API and Web` compound launch configuration.

## Coding Style & Naming Conventions

Follow the existing C# and Razor style: four-space indentation in C# and descriptive PascalCase names for types, public members, and test methods; use camelCase for local variables and parameters. Keep nullable reference types enabled and use async APIs for file and HTTP operations. Keep implicit usings disabled. Place new components, models, and services in their corresponding directories. No repository-specific formatter configuration is currently defined.

## Testing Guidelines

Use NUnit for new tests and name test methods after the behavior and expected result. `ApiTest.cs` exercises JSON storage; `HttpEndPointTest.cs` exercises routes through an in-process test host. Use isolated temporary data for storage tests and never modify the checked-in `todos.json` during tests. Run the test project after changing API behavior. Coverlet is available, but no coverage threshold is defined.

## Commit & Pull Request Guidelines

Recent commit subjects include both imperative descriptions and a `feat:` prefix. Use a short, descriptive subject, optionally with a conventional prefix such as `feat:` or `fix:`. Pull requests should describe the change and its verification, link relevant issues, and include screenshots for visible UI changes.

## Configuration & Data

The web client API URL is configured in `src/TodoList.Web/wwwroot/appsettings.json`; adjust API CORS settings when changing client origins. Treat `todos.json` as application data and avoid committing unintended local edits or secrets.
