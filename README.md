# Terranova

Lune: ASP.NET Core backend, SQL Server, Angular frontend.

## Run with Docker

Requirements: Docker Engine and the Docker Compose plugin. Nothing else needs to be installed locally.

To run only the frontend and mock API, use:

```bash
docker compose up --build mock-api frontend
```

| Service | URL | Notes |
|---|---|---|
| Frontend (Angular) | [http://localhost:8080](http://localhost:8080) | `/api` is proxied to the mock API |
| Mock API (ASP.NET Core) | [http://localhost:4001](http://localhost:4001) | In-memory, resets on restart |
| Backend (Asp.net Core) | [http://localhost:3001](http://localhost:3001) | Persistant data |

Press `Ctrl+C` to stop, or run `docker compose down`.

### Mock API

[mock-api/](./mock-api) implements everything in [docs/API.md](./docs/API.md) so the frontend can be built without the real backend. It is organised by module (`Auth`, `Games`, `Favorites`, `Shared`), and the game catalog lives in `mock-api/Data/games.json`.

- Demo login: `demo@example.com` / `Passw0rd!` (or register a new account).
- Add `?mock503=1` to a games request, or to `PUT /api/favorites/{id}`, to simulate the game provider being unavailable.
- Data is held in memory, so favorites and new accounts reset when the container restarts.

### Real backend

The SQL Server and ASP.NET Core backend are regular Compose services; no profile is needed. The backend is available at [http://localhost:3001](http://localhost:3001). To run the full stack against it, copy `.env.example` to `.env` and set `SA_PASSWORD`, `JWT_SECRET` and `RAWG_API_KEY`. Then start Compose with the frontend pointed at the backend:

```bash
API_URL=http://backend:3001 docker compose up --build
```

The mock API remains available on port 4001. `API_URL` changes the frontend's `/api` proxy target; without it, the frontend continues to use the mock. The backend is a modular ASP.NET Core API in [backend/src/](./backend/src) (`Lune.Api`, `Auth`, `Games`, `Favorites`, `Shared`). Each module owns its own database schema and applies its EF migrations at startup. All routes live under `/api` (for example `/api/favorites`), and auth is an HttpOnly `Lune_auth` cookie, so log in first (`POST /api/auth/login`) before calling the favorites endpoints. See [docs/API.md](./docs/API.md) and [docs/DATABASE.md](./docs/DATABASE.md).

## Without Docker

Mock API (.NET 10 SDK):

```bash
cd mock-api && dotnet run --urls http://localhost:4001
```

Frontend (Node.js 22):

```bash
cd frontend && npm install && npm start
```


## Dev Dependencies

Backend development requires Microsofts dotnet sdk (version 10.0)

Install with:
```sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-10.0
```
