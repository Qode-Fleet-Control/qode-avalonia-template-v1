# Built by .github/workflows/deploy.yml and pushed to Artifact Registry.
#
# A JOB image: its default command runs the headless UI test suite (Avalonia.Headless —
# the real windows and controls on Avalonia's headless platform, no display server) plus
# the unit tests, and exits 0 when they all pass. It serves no HTTP; a desktop app has
# nothing to serve on $PORT.
#   - single stage on the SDK image: `dotnet test` needs the SDK at run time;
#   - the project files are copied and restored first, so the package layer is cached
#     until a dependency changes; the build happens at image build, the run only tests;
#   - runs as the image's built-in non-root `app` user ($APP_UID).
FROM mcr.microsoft.com/dotnet/sdk:10.0
ARG BUILD_ID=""
ENV BUILD_ID=$BUILD_ID DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /src
RUN chown app:app /src
USER $APP_UID
COPY --chown=app:app AvaloniaApp.slnx global.json ./
COPY --chown=app:app src/AvaloniaApp/AvaloniaApp.csproj src/AvaloniaApp/
COPY --chown=app:app tests/AvaloniaApp.Tests/AvaloniaApp.Tests.csproj tests/AvaloniaApp.Tests/
RUN dotnet restore AvaloniaApp.slnx
COPY --chown=app:app . .
RUN dotnet build AvaloniaApp.slnx --no-restore -c Release
CMD ["dotnet", "test", "--solution", "AvaloniaApp.slnx", "--no-build", "-c", "Release"]
