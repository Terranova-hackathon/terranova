FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY MockApi.csproj ./
RUN dotnet restore

COPY . .

EXPOSE 4001

# The source is bind-mounted in compose, so edits hot-reload.
CMD ["dotnet", "watch", "run", "--non-interactive", "--urls", "http://0.0.0.0:4001"]
