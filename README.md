# TODO list — .NET 11

A two-project TODO list app:

- **TodoList.Web**: Blazor WebAssembly interface
- **TodoList.Api**: ASP.NET Core Minimal Web API
- **Storage**: `src/TodoList.Api/Data/todos.json` (no database)

## Run

Install the [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0), then start the API and web client in two terminals:

```powershell
dotnet run --project .\src\TodoList.Api
dotnet run --project .\src\TodoList.Web
```

Open `https://localhost:5002`. The API runs at `https://localhost:7001`; update `src/TodoList.Web/wwwroot/appsettings.json` if you use a different API URL.

## API

`GET`, `POST` `/api/todos` and `GET`, `PUT`, `DELETE` `/api/todos/{id}` are available. Changes are serialized safely to the JSON file by the API process.
