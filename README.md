# TransTrans

TransTrans is a Blazor Server remake of the original WPF prototype.

## Play locally

```powershell
dotnet run --project TransTrans.csproj
```

Open the shown local URL, create a room, and share the room code with another browser session.

## Host online

This is a Blazor Server app, so it needs an ASP.NET Core host such as Azure App Service, Render, Fly.io, Railway, or a VPS. A `Dockerfile` and `render.yaml` are included for container hosting.

```bash
docker build -t transtrans .
docker run -p 8080:8080 transtrans
```

On Render, create a new Blueprint from this repository. Render will read `render.yaml`, build the Docker image, and provide a public URL.

## Current features

- Two-player room-code matches
- Shared cauldron, hands, research, crafting, item use, and turn flow
- Web handling for selection effects such as Thunder, Obsidian, Clay, Moss, and Pottery
- Storm destroys both players' hands
