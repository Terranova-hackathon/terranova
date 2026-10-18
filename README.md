# Terranova

## Run with Docker

Requirements: Docker Engine and the Docker Compose plugin. You do not need to install Node.js or pnpm on your machine for this workflow; the images install pnpm 10.32.1 and the project packages. Create a `.env` file in the project root with `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `NODE_ENV`, `API_PORT`, `JWT_SECRET`, and `PROJECT_NAME` set.


Build the services and start the project:

```bash
docker compose build
docker compose up
```

Open the frontend at [http://localhost:3000](http://localhost:3000). Press `Ctrl+C` to stop the services, or run `docker compose down`.


## With out Docker
For development without Docker, install Node.js 20 for the backend and Node.js 22 for the frontend, then install pnpm 10.32.1:

```bash
npm install --global pnpm@10.32.1
```