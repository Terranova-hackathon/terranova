# Terranova

Lune: ASP.NET Core backend, SQL Server, Angular frontend.

## Run with Docker

Requirements: Docker Engine and the Docker Compose plugin. Nothing else needs to be installed locally.

Copy `.env.example` to `.env`. The default (mock) setup needs no other values.

```bash
docker compose up --build
```

| Service | URL | Notes |
|---|---|---|
| Frontend (Angular) | [http://localhost:8080](http://localhost:8080) | `/api` is proxied to the mock API |
| Mock API (ASP.NET Core) | [http://localhost:4001](http://localhost:4001) | In-memory, resets on restart |

Press `Ctrl+C` to stop, or run `docker compose down`.

### Mock API

[mock-api/](./mock-api) implements everything in [docs/API.md](./docs/API.md) so the frontend can be built without the real backend. It is organised by module (`Auth`, `Games`, `Favorites`, `Shared`), and the game catalog lives in `mock-api/Data/games.json`.

- Demo login: `demo@example.com` / `Passw0rd!` (or register a new account).
- Add `?mock503=1` to a games request, or to `PUT /api/favorites/{id}`, to simulate the game provider being unavailable.
- Data is held in memory, so favorites and new accounts reset when the container restarts.

### Real backend

The SQL Server and ASP.NET Core services sit behind the `real` profile. Set `SA_PASSWORD`, `JWT_SECRET` and `RAWG_API_KEY` in `.env`, then:

```bash
API_URL=http://backend:3001 docker compose --profile real up --build
```

The backend is published on port 3001 (the mock keeps 4001). It is a modular ASP.NET Core API in [backend/src/](./backend/src) (`Lune.Api`, `Auth`, `Games`, `Favorites`, `Shared`). Each module owns its own database schema and applies its EF migrations at startup. All routes live under `/api` (for example `/api/favorites`), and auth is an HttpOnly `Lune_auth` cookie, so log in first (`POST /api/auth/login`) before calling the favorites endpoints. See [docs/API.md](./docs/API.md) and [docs/DATABASE.md](./docs/DATABASE.md).

## Without Docker

Mock API (.NET 10 SDK):

```bash
cd mock-api && dotnet run --urls http://localhost:4001
```

Frontend (Node.js 22):

```bash
cd frontend && npm install && npm start
```
