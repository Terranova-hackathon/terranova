FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# Placeholder: the ASP.NET Core solution has not been added yet. The container
# idles until a .csproj shows up under backend/, then runs it with hot reload.
COPY . .

EXPOSE 3001

# Waits for a project to exist so the container doesn't crash-loop while backend/ is empty.
CMD ["sh", "-c", "if ls /app/src/*/*.csproj >/dev/null 2>&1 || ls /app/*.csproj >/dev/null 2>&1; then dotnet watch run --non-interactive --urls http://0.0.0.0:3001; else echo 'No ASP.NET Core project in backend/ yet.'; sleep infinity; fi"]
