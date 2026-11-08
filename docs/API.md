# Lune API reference (for building a mock)

Everything the Angular frontend calls, with request/response shapes, status codes and rules. It was written from the real backend code, so a mock that follows it will behave like the real API. The database schema is in [DATABASE.md](./DATABASE.md).

Source of truth in the backend repo:

| Area | Routes declared in | DTOs |
|---|---|---|
| Auth | `backend/src/Lune.Auth/Endpoints/AuthEndpoints.cs` | `Lune.Auth/Contracts/AuthContracts.cs` |
| Games | `backend/src/Lune.Games/Endpoints/GamesEndpoints.cs` | `Lune.Games/Contracts/GameContracts.cs` |
| Favorites | `backend/src/Lune.Favorites/Endpoints/FavoritesEndpoints.cs` | `Lune.Favorites/Contracts/FavoriteContracts.cs` |
| Paging | | `Lune.Shared/Paging/PagedResult.cs` |

## 1. Conventions

- **Base URL:** same origin as the frontend. Angular calls relative URLs such as `/api/games`. In development the dev server proxies `/api` to the backend (`frontend/proxy.conf.json`). Point that proxy at the mock.
- **Format:** JSON in and out, **camelCase** property names, `Content-Type: application/json`.
- **Dates:** `addedAtUtc` is an ISO 8601 UTC string. `released` is a plain `YYYY-MM-DD` string or `null`.
- **No CORS** is needed while the frontend and API share an origin (via the proxy).

### Authentication (cookie, not a header)

- Login and register set an **HttpOnly cookie** named `Lune_auth` holding a JWT. The browser sends it automatically. Angular never reads it and never sets an `Authorization` header.
- Attributes: `HttpOnly`, `SameSite=Lax`, `Path=/`, 60 minute expiry. `Secure` only when the request is HTTPS.
- Logout deletes the cookie.
- A mock can use any opaque session value. It only has to check that the cookie maps to a user.

| Route group | Auth needed |
|---|---|
| `/api/auth/register`, `/login`, `/logout` | No |
| `/api/auth/me` | Yes |
| `/api/games/**` | Yes |
| `/api/favorites/**` | Yes |

A protected route without a valid cookie returns **`401` with an empty body**.

### Frontend behavior to preserve

- On startup Angular calls `GET /api/auth/me`. A `401` just means "not logged in".
- A `401` from any non-`/api/auth/*` call clears the user and redirects to `/login`.
- `GET /api/favorites/ids` is fetched whenever a user is logged in.

### Error format (ProblemDetails)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["The field Password must be a string or array type with a minimum length of '8'."]
  },
  "traceId": "00-..."
}
```

- The frontend shows `errors` (joined) first, then `detail`, then `title`. `type` and `traceId` can be anything.
- Validation errors (`400`) have `errors`: `field -> string[]`. Other errors have `title` and optionally `detail`.
- Auth body validation keys are **PascalCase** (`Email`, `Password`). Games query validation keys are **camelCase** (`genres`, `ordering`).

### Status codes

| Code | Meaning |
|---|---|
| 200 / 201 / 204 | OK with body / account created / OK no body |
| 400 | Validation failed |
| 401 | Not logged in, or bad credentials |
| 404 | Game not found, or unknown `/api/*` route |
| 409 | Favorites limit reached |
| 500 | `{ "title": "An unexpected error occurred.", "status": 500 }` |
| 503 | Game data provider (RAWG) unavailable or not configured |

---

## 2. Endpoint summary

| Method | Path | Auth | Purpose |
|---|---|:-:|---|
| GET | `/api/auth/me` | Yes | Current user (session check) |
| POST | `/api/auth/register` | No | Create account and sign in |
| POST | `/api/auth/login` | No | Sign in |
| POST | `/api/auth/logout` | No | Sign out |
| POST | `/api/auth/delete-account` | Yes | Delete the current account |
| GET | `/api/games` | Yes | Search, filter, sort and page games |
| GET | `/api/games/filters` | Yes | Options for filter dropdowns |
| GET | `/api/games/companies` | Yes | Developer/publisher autocomplete |
| GET | `/api/games/featured` | Yes | One randomly selected popular game (mock returns a fixed highlight) |
| GET | `/api/games/{id}` | Yes | One game's details |
| GET | `/api/favorites` | Yes | Current user's favorites |
| GET | `/api/favorites/ids` | Yes | Favorited game ids only |
| PUT | `/api/favorites/{igdbId}` | Yes | Add a favorite |
| DELETE | `/api/favorites/{igdbId}` | Yes | Remove a favorite |
| GET | `/health` | No | Text `Healthy` |

Any other `/api/*` path returns an empty `404`. Non-`/api` paths without a file extension return Angular's `index.html`.

---

## 3. Auth

### Type `User`

```json
{ "id": "7c1d2f0e-6b1a-4a39-9f1e-2c3f6a5b8d10", "email": "ann@example.com", "displayName": "Ann" }
```

`displayName` may be `null`. `id` is a GUID string.

### `GET /api/auth/me`

- **200:** `User`
- **401:** no or invalid cookie, or the user no longer exists (empty body)

### `POST /api/auth/register`

```json
{ "email": "ann@example.com", "password": "Passw0rd!", "displayName": "Ann" }
```

| Field | Rules |
|---|---|
| `email` | required, valid email |
| `password` | required, min 8, plus the Identity rules below |
| `displayName` | optional |

Identity rules: one digit, one lowercase, one uppercase, one non-alphanumeric character.

- **201:** `User`, header `Location: /api/auth/me`, `Set-Cookie: Lune_auth=...`
- **400:** `errors` keyed by field name for basic validation, or by Identity error code for rule failures. Email is also the username, so a duplicate yields both codes:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "DuplicateUserName": ["Username 'ann@example.com' is already taken."],
    "DuplicateEmail": ["Email 'ann@example.com' is already taken."]
  }
}
```

Other codes: `PasswordTooShort`, `PasswordRequiresDigit`, `PasswordRequiresLower`, `PasswordRequiresUpper`, `PasswordRequiresNonAlphanumeric`.

### `POST /api/auth/login`

```json
{ "email": "ann@example.com", "password": "Passw0rd!" }
```

Both required, email must be valid (else **400** with `errors.Email` / `errors.Password`).

- **200:** `User` plus `Set-Cookie`
- **401:** wrong password, unknown email or locked account, always the same body:

```json
{ "title": "Invalid email or password.", "status": 401 }
```

Real lockout: 5 wrong passwords lock the account for 5 minutes. Optional to mock.

### `POST /api/auth/logout`

Angular sends `{}`. No auth required.

- **204:** no body, cookie deleted

### `POST /api/auth/delete-account`

Requires authentication. The account identified by the auth cookie is deleted; no user ID is accepted from the request.

- **204:** account deleted, no body, cookie deleted
- **401:** no valid cookie, or the account no longer exists
- **400:** Identity rejected the deletion; `errors` contains the Identity error codes and descriptions

---

## 4. Games

### Types

```ts
interface GameSummary {
  id: number;                 // game ID from the configured provider
  name: string;
  released: string | null;    // "2013-09-17"
  backgroundImage: string | null;
  rating: number;             // 0-5
  metacritic: number | null;  // 0-100
  genres: string[];           // names
  platforms: string[];        // names
}

interface GameDetails extends GameSummary {
  description: string | null; // plain text
  website: string | null;
  developers: string[];
  publishers: string[];
}

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;         // ceil(totalCount / pageSize)
}

interface FilterOption { value: string; name: string; }
interface CompanyOption { slug: string; name: string; gamesCount: number; }
```

### `GET /api/games`

Searches the game catalog. All query parameters are optional.

| Param | Type | Default | Rules |
|---|---|---|---|
| `search` | string | none | Trimmed; blank values are ignored. |
| `page` | int | `1` | Values below 1 are treated as 1. |
| `pageSize` | int | `20` | Clamped to 1-40. |
| `genres` | comma-separated slugs | none | Up to 10 distinct slugs. Values are trimmed and lowercased. Must match `^[a-z0-9][a-z0-9_-]{0,79}$`. |
| `platforms` | comma-separated positive integers | none | Up to 10 distinct platform IDs. |
| `developers` | comma-separated company slugs | none | Up to 10 distinct slugs; same format and normalization as `genres`. |
| `publishers` | comma-separated company slugs | none | Up to 10 distinct slugs; same format and normalization as `genres`. |
| `ordering` | string | none | One of the six values returned by `/api/games/filters`. Leading `-` means descending. |
| `metacriticMin` | int | none | Inclusive minimum score from 0 to 100. With IGDB, this filters IGDB's aggregated critic rating, which is exposed as `metacritic`. |
| `metacriticMax` | int | none | Inclusive maximum score from 0 to 100; must be greater than or equal to `metacriticMin`. With IGDB, this filters IGDB's aggregated critic rating. |

#### How search and filters combine

- Multiple values for one list filter are alternatives (OR). For example, `genres=action,adventure` matches either genre.
- Different filters are combined (AND). For example, `genres=action&platforms=6` requires both a matching genre and platform.
- `search` is combined with the selected filters.
- The IGDB provider excludes games without a known first release date or total rating from search results.
- If no search term is provided, the IGDB provider excludes games with no rating count and defaults to most-rated first.
- With a search term, the IGDB provider matches a substring of the game name and defaults to name order when `ordering` is omitted. This may return different matches than IGDB's full-text search.
- `ordering` values mean: `-rating` highest rating first; `-metacritic` highest critic score first; `-released` newest release first; `released` oldest release first; `-added` most popular first; `name` A-Z.

Use `/api/games/filters` to discover available genre slugs and platform IDs, and `/api/games/companies` to discover developer and publisher slugs. The set of options depends on the configured provider.

#### Example requests

These examples use the backend's HTTP development URL. When using Postman or a browser, replace `{{baseUrl}}` with `http://localhost:3001`.

```http
@baseUrl = http://localhost:3001

### Default listing (page 1, 20 results)
GET {{baseUrl}}/api/games

### Search by text
GET {{baseUrl}}/api/games?search=zelda

### Search with paging
GET {{baseUrl}}/api/games?search=mario&page=2&pageSize=10

### Match either genre, and require one of the selected platforms
GET {{baseUrl}}/api/games?genres=action,adventure&platforms=6,167

### Sort by rating and restrict the score range
GET {{baseUrl}}/api/games?ordering=-rating&metacriticMin=70&metacriticMax=95

### Search, filter, sort, and page results together
GET {{baseUrl}}/api/games?search=zelda&genres=action,adventure&platforms=130&ordering=-released&metacriticMin=80&page=1&pageSize=20

### Get the available genre, platform, and ordering options
GET {{baseUrl}}/api/games/filters

### Filter by developer (use a slug returned by the companies endpoint)
GET {{baseUrl}}/api/games?developers=nintendo

### Filter by publisher
GET {{baseUrl}}/api/games?publishers=nintendo

### A-Z ordering
GET {{baseUrl}}/api/games?ordering=name

### Check validation: unsupported ordering
GET {{baseUrl}}/api/games?ordering=popular

### Check validation: invalid platform ID
GET {{baseUrl}}/api/games?platforms=pc

### Check validation: incompatible score bounds
GET {{baseUrl}}/api/games?metacriticMin=90&metacriticMax=50
```

- **200:** `PagedResult<GameSummary>`

```json
{
  "items": [
    {
      "id": 22511,
      "name": "The Legend of Zelda: Breath of the Wild",
      "released": "2017-03-03",
      "backgroundImage": "https://media.rawg.io/media/games/cc1/cc196a5ad763955d6532cdba236f730c.jpg",
      "rating": 4.38,
      "metacritic": 97,
      "genres": ["Action", "Adventure"],
      "platforms": ["Nintendo Switch", "Wii U"]
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

No matches return **200** with an empty `items` array and `totalCount: 0`. `totalPages` is calculated from the result count and page size.

- **400:** validation problem with an `errors` object keyed by parameter name. Invalid slugs, more than 10 distinct values, invalid platform IDs, unsupported orderings, scores outside 0-100, or a minimum greater than the maximum are rejected.
- **503:** see below.

### `GET /api/games/filters`

Returns the filter options available from the configured provider and the supported ordering choices.

- **200:**

```json
{
  "genres": [
    { "value": "action", "name": "Action" },
    { "value": "indie", "name": "Indie" },
    { "value": "adventure", "name": "Adventure" },
    { "value": "role-playing-games-rpg", "name": "RPG" }
  ],
  "platforms": [
    { "value": "1", "name": "PC" },
    { "value": "2", "name": "PlayStation" },
    { "value": "3", "name": "Xbox" },
    { "value": "7", "name": "Nintendo" }
  ],
  "orderings": [
    { "value": "-rating", "name": "Rating (high to low)" },
    { "value": "-metacritic", "name": "Metacritic (high to low)" },
    { "value": "-released", "name": "Release date (newest)" },
    { "value": "released", "name": "Release date (oldest)" },
    { "value": "-added", "name": "Most popular" },
    { "value": "name", "name": "Name (A-Z)" }
  ]
}
```

- Genre `value` is a slug. Platform `value` is a numeric ID **as a string**. Pass these values as `genres` / `platforms` on `GET /api/games`.
- `orderings` contains the six supported values: `-rating`, `-metacritic`, `-released`, `released`, `-added`, and `name`.
- **503** if the provider is unavailable.

### `GET /api/games/companies`

| Param | Required | Rules |
|---|:-:|---|
| `kind` | Yes | `developer` or `publisher` (case-insensitive) |
| `search` | Yes | 2-60 chars after trimming |

- Example queries:
```
@baseUrl = http://localhost:3001

### Search developers
GET {{baseUrl}}/api/games/companies?kind=developer&search=Nintendo

### Search publishers
GET {{baseUrl}}/api/games/companies?kind=publisher&search=Nintendo

### Use a returned developer slug in a game search
GET {{baseUrl}}/api/games?developers=nintendo
```

- **200:** up to **10** `CompanyOption`:

```json
[
  { "slug": "nintendo", "name": "Nintendo", "gamesCount": 1294 },
  { "slug": "nintendo-epd", "name": "Nintendo EPD", "gamesCount": 45 }
]
```

- **400:** `errors.kind` and/or `errors.search`
- **503**

Company slugs come from the configured provider. With IGDB, `gamesCount` is the number of games
associated with the requested role (developed or published).

### `GET /api/games/featured`

- **200:** `GameDetails` (same shape as `GET /api/games/{id}`)
- **404:** no eligible games are available
- **503:** same as the other games routes

The real backend selects randomly from up to 50 popular games (ordered by rating count in IGDB or rating in RAWG). The mock returns its fixed highlighted game.

### `GET /api/games/{id}`

`id` must be an integer.

- **200:** `GameDetails`

```json
{
  "id": 3498,
  "name": "Grand Theft Auto V",
  "released": "2013-09-17",
  "backgroundImage": "https://media.rawg.io/media/games/456/456dea5e1c7e3cd07060c14e96612001.jpg",
  "rating": 4.47,
  "metacritic": 92,
  "genres": ["Action", "Adventure"],
  "platforms": ["PC", "PlayStation 5", "Xbox One"],
  "description": "Rockstar Games went bigger, since their previous installment...",
  "website": "http://www.rockstargames.com/V/",
  "developers": ["Rockstar North"],
  "publishers": ["Rockstar Games"]
}
```

- **404:** unknown game (empty body)
- **503**

### Games 503 body

```json
{
  "title": "Game data is currently unavailable.",
  "status": 503,
  "detail": "The RAWG API key is not configured (Rawg__ApiKey)."
}
```

---

## 5. Favorites

A per-user bookmark. The server stores a **snapshot** of the game summary (name, image, release date, rating, Metacritic, genres, platforms) when saved. All of it comes from the game catalog, never from the request. The user always comes from the cookie.

```ts
interface Favorite {
  id: number;            // game id
  name: string;
  BackgroundImage: string | null;  // the game's backgroundImage
  released: string | null;  // "2013-09-17"
  rating: number;           // 0-5
  metacritic: number | null; // 0-100
  genres: string[];         // names
  platforms: string[];      // names
  addedAtUtc: string;       // "2026-10-28T12:04:41.123Z"
}
```

### `GET /api/favorites`
- **200:** `Favorite[]`, **newest first**, max 1000, `[]` if none
- **401**

### `GET /api/favorites/ids`
- **200:** `number[]`, e.g. `[3498, 22511]`
- **401**

### `PUT /api/favorites/{rawgId}`

Angular sends `{}`. **Idempotent.**

| Result | Status | Body |
|---|---|---|
| Added or already a favorite | 204 | none |
| `rawgId <= 0` or game doesn't exist | 404 | empty |
| User already has 1000 | 409 | `{ "title": "Favorites limit reached.", "status": 409 }` |
| Provider unavailable | 503 | games 503 body |
| Not logged in | 401 | empty |

### `DELETE /api/favorites/{rawgId}`
- **204** always when logged in (idempotent)
- **401**
