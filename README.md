# Kayauth (Windows Source Repository)

Clean, Windows-only source repository for the Kayauth app.

## Scope
- Target platform: Windows only
- Target framework: `net8.0-windows10.0.19041.0`
- Architecture: AnyCPU/x64 via standard .NET MAUI Windows build flow

## Prerequisites
- Windows 10/11
- .NET SDK 8.0.x (see `global.json`)
- .NET MAUI workload installed

## Quick Start
```powershell
dotnet workload restore
dotnet restore
dotnet build -f net8.0-windows10.0.19041.0 -c Debug
dotnet run -f net8.0-windows10.0.19041.0
```

## Project Layout
- `Platforms/Windows`: Windows platform entry and manifests
- `Views`, `ViewModels`, `Services`, `Models`: App layers
- `Resources`: Icons, styles, fonts, images

## Intentionally Excluded
- Non-Windows platform folders
- Installer/packaging and store-publish materials
- Build outputs and local cache folders
- Logs, certificates, and machine-local secret files
