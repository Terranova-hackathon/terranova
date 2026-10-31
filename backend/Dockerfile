FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY . .

EXPOSE 3001

CMD ["dotnet", "watch", "run", "--non-interactive", "--project", "src/Lune.Api", "--urls", "http://0.0.0.0:3001"]
