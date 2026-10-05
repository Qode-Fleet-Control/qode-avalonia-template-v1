# Avalonia template

Provisioned from [`Qode-Fleet-Control/fleet-template-v1`](https://github.com/Qode-Fleet-Control/fleet-template-v1) — the fleet
lifecycle contract (`bin/`, `fleet.conf`, `compose.yaml`, deploy workflows) with the stock
Avalonia MVVM desktop app (Avalonia 12.1.3, CommunityToolkit.Mvvm, .NET 10) and a headless UI
test suite laid on top.

    AvaloniaApp.slnx                 the solution (app + tests)
    global.json                      `dotnet test` uses Microsoft.Testing.Platform
    src/AvaloniaApp/                 the desktop app (App.axaml, Views/, ViewModels/)
    tests/AvaloniaApp.Tests/         xUnit.net v3 + Avalonia.Headless.XUnit
    Dockerfile                       SDK image; builds at image build, CMD runs the tests
    compose.yaml                     the fleet's docker runtime (service `app`, no ports)

**This repo is not a service.** A desktop app has nothing to serve on `$PORT`, so on the fleet
the job is the test suite: `[AvaloniaFact]` tests that build the real `App` on Avalonia's
headless platform (`TestAppBuilder.cs`), show the real `MainWindow`, type into it with
`KeyTextInput` and check the two-way binding to `MainViewModel` — no display server, no GPU —
plus plain `[Fact]` view-model tests. `START_CMD` and `DOCKER_START_CMD` are empty.

## Origin

Generated 2026-10-05 with the official template packs, inside the official SDK image
(.NET SDK 10.0.401):

    docker run --rm -u $(id -u):$(id -g) -e HOME=/tmp -v "$PWD":/w -w /w \
      mcr.microsoft.com/dotnet/sdk:10.0 bash -c '
        dotnet new install Avalonia.Templates
        dotnet new install xunit.v3.templates
        dotnet new sln -n AvaloniaApp
        dotnet new avalonia.mvvm -n AvaloniaApp -o src/AvaloniaApp
        dotnet new xunit3 -n AvaloniaApp.Tests -o tests/AvaloniaApp.Tests --framework net10.0
        cd tests/AvaloniaApp.Tests
        dotnet add package Avalonia.Headless.XUnit --version 12.1.3
        dotnet add reference ../../src/AvaloniaApp
        cd /w && dotnet sln add src/AvaloniaApp tests/AvaloniaApp.Tests'

The headless test setup follows Avalonia's "Headless Testing with xUnit" docs
(`[assembly: AvaloniaTestApplication]`, `UseHeadless(...)`, `[AvaloniaFact]`).

## Running it

**On the fleet / with docker**

    bin/run                          # = docker compose build (nothing to start)
    docker compose run --rm app      # runs the tests; exit 0 = all passed

**Without docker** (needs the .NET 10 SDK on PATH)

    dotnet test                                  # the headless UI tests + unit tests
    dotnet run --project src/AvaloniaApp         # the app itself (needs a display)
    FLEET_RUNTIME=process bin/run                # = dotnet restore + dotnet build

| step | process runtime | docker runtime |
|---|---|---|
| install | `dotnet restore AvaloniaApp.slnx` | — |
| build | `dotnet build AvaloniaApp.slnx --no-restore` | `docker compose build` |
| run the job | `dotnet test` | `docker compose run --rm app` |

## Deviations from the stock generator output, and why

- **`xunit.v3.mtp-v2` pinned to 3.2.2, not the `xunit3` template's 4.0.1.**
  Avalonia.Headless.XUnit 12.1.3 is built against xunit.v3 3.2.x; with 4.x every
  `[AvaloniaFact]` fails discovery with `MissingMethodException`
  (`TestIntrospectionHelper.GetTestCaseDetails`). Move up when Avalonia does.
- `UnitTest1.cs` replaced by `TestAppBuilder.cs`, `MainWindowTests.cs`, `MainViewModelTests.cs`.
  The app itself is the stock template output, unchanged.
- Projects under `src/` and `tests/`, not the repo root: .NET writes build output to each
  project's `bin/`/`obj/`, which at the root would collide with the fleet's `bin/` scripts.
  Named `AvaloniaApp`, not `App`, so the namespace does not shadow the template's `App` class.
- Added: `Dockerfile`, `compose.yaml`, `.dockerignore`, a compact `.gitignore` (the stock
  `dotnet new gitignore` ignores every `bin/` — including the fleet's), `.env.example`,
  `fleet.conf`, `bin/`, `.github/workflows/`, `docs/fleet-lifecycle.md`.
- No NuGet lock file: the generators do not create one.

## Verified

**The docker runtime has NOT been verified yet.** On 2026-10-05 the shared docker host's disk
stayed at 0-5G free (under the 6G floor for a build) for over five hours, so `docker compose build`
was never run for this repo. Run the checks below once before trusting the image.

What did pass, inside `mcr.microsoft.com/dotnet/sdk:10.0` (.NET SDK 10.0.401): `dotnet test` →
4 tests (3 headless `[AvaloniaFact]` UI tests + 1 view-model test), 4 passed — after pinning
xunit.v3 to 3.2.2 (with the template's 4.0.1 the 3 UI tests failed discovery).

Still to run: `docker compose build && docker compose run --rm app` (expect exit 0).
