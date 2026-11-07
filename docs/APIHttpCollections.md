# Games API: example requests and responses

A collection of requests for `/api/games/**`, each with the response underneath. Use it as a checklist for writing tests. The requests also work in the VS Code REST Client or any `.http` runner.

```http
@baseUrl = http://localhost:3001
```

Notes:

- The games endpoints need no login.
- Long bodies are trimmed with `…`.
- **Error responses with an empty body** come from the framework (bad number, bad route, wrong method). Assert the status only.
- Not implemented yet: the `genres`, `platforms`, `developers`, `publishers`, `ordering` and `metacriticMin`/`metacriticMax` parameters on `GET /api/games`. They are accepted and ignored (see [Ignored parameters](#ignored-parameters)).

---

## 1. Search games: `GET /api/games`

| Param | Default | Rules |
|---|---|---|
| `search` | none | Trimmed. Blank means the default list (most rated first). |
| `page` | `1` | Must be an integer. `0` and negatives are echoed back and return the first page. |
| `pageSize` | `20` | Must be an integer. Clamped to 1-50. |

### Default list

```http
GET {{baseUrl}}/api/games
```

```http
200 OK
Content-Type: application/json
```

```json
{
  "items": [
    {
      "id": 1020,
      "name": "Grand Theft Auto V",
      "released": "2013-09-17",
      "backgroundImage": "https://images.igdb.com/igdb/image/upload/t_720p/co2lbd.jpg",
      "rating": 4.44,
      "metacritic": 88,
      "genres": ["Shooter", "Racing", "Adventure"],
      "platforms": ["Xbox Series X|S", "PlayStation 3", "PlayStation 4", "PC (Microsoft Windows)"]
    },
    {
      "id": 1942,
      "name": "The Witcher 3: Wild Hunt",
      "released": "2015-05-19",
      "backgroundImage": "https://images.igdb.com/igdb/image/upload/t_720p/coaarl.jpg",
      "rating": 4.64,
      "metacritic": 92,
      "genres": ["Role-playing (RPG)", "Adventure"],
      "platforms": ["PC (Microsoft Windows)", "PlayStation 4", "Xbox One"]
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3
}
```

### Search by name

```http
GET {{baseUrl}}/api/games?search=zelda
```

```http
200 OK
```

Same shape as above, in IGDB relevance order.

### Page and page size

```http
GET {{baseUrl}}/api/games?search=zelda&page=2&pageSize=10
```

```http
200 OK
```

```json
{ "items": [ … ], "page": 2, "pageSize": 10, "totalCount": 45, "totalPages": 5 }
```

### Page size is clamped

```http
GET {{baseUrl}}/api/games?pageSize=999
```

```http
200 OK
```

```json
{ "items": [ … ], "page": 1, "pageSize": 50, "totalCount": 45, "totalPages": 1 }
```

```http
GET {{baseUrl}}/api/games?pageSize=0
```

```http
200 OK
```

```json
{ "items": [ … ], "page": 1, "pageSize": 1, "totalCount": 45, "totalPages": 45 }
```

### Blank search is the default list

```http
GET {{baseUrl}}/api/games?search=%20%20
```

```http
200 OK
```

Same body as the default list.

### Quotes and backslashes are escaped

```http
GET {{baseUrl}}/api/games?search=a%22b%5Cc
```

```http
200 OK
```

The search text `a"b\c` is escaped before it goes to IGDB, so it cannot change the query.

### No results

```http
GET {{baseUrl}}/api/games?search=zzzzzzzzzzzz
```

```http
200 OK
```

```json
{ "items": [], "page": 1, "pageSize": 20, "totalCount": 0, "totalPages": 0 }
```

### Page below 1 is echoed back

```http
GET {{baseUrl}}/api/games?page=0
```

```http
200 OK
```

```json
{ "items": [ … ], "page": 0, "pageSize": 20, "totalCount": 45, "totalPages": 3 }
```

The items are the first page. `page=-3` behaves the same and echoes `-3`.

### Errors

```http
GET {{baseUrl}}/api/games?page=abc
```

```http
400 Bad Request
```

Empty body. The same happens for `page=`, `page=1.5`, `page=99999999999` and `pageSize=abc`.

```http
POST {{baseUrl}}/api/games
```

```http
405 Method Not Allowed
```

Empty body.

```http
GET {{baseUrl}}/api/games?search=zelda
```

```http
503 Service Unavailable
Content-Type: application/problem+json
```

```json
{
  "title": "Game data is currently unavailable.",
  "status": 503,
  "detail": "The game data service could not be reached."
}
```

This is returned when IGDB is down or returns an error. See [503 details](#5-503-details) for the other messages.

### Ignored parameters

Each of these returns `200` with the same body as the default list. The parameter has no effect yet.

```http
GET {{baseUrl}}/api/games?genres=action
GET {{baseUrl}}/api/games?platforms=6
GET {{baseUrl}}/api/games?developers=nintendo&publishers=nintendo
GET {{baseUrl}}/api/games?ordering=bogus
GET {{baseUrl}}/api/games?metacriticMin=80&metacriticMax=10
```

---

## 2. Game details: `GET /api/games/{id}`

`id` must be an integer.

### Known game

```http
GET {{baseUrl}}/api/games/1020
```

```http
200 OK
Content-Type: application/json
```

```json
{
  "id": 1020,
  "name": "Grand Theft Auto V",
  "released": "2013-09-17",
  "backgroundImage": "https://images.igdb.com/igdb/image/upload/t_720p/co2lbd.jpg",
  "rating": 4.44,
  "metacritic": 88,
  "description": "…",
  "website": "https://www.igdb.com/games/grand-theft-auto-v",
  "genres": ["Shooter", "Racing", "Adventure"],
  "platforms": ["PC (Microsoft Windows)", "PlayStation 4", "Xbox One"],
  "developers": ["Rockstar North"],
  "publishers": ["Rockstar Games"]
}
```

`description`, `website`, `released` and `backgroundImage` are `null` when IGDB has no value. `developers` and `publishers` have no duplicates.

### Errors

```http
GET {{baseUrl}}/api/games/999999999
```

```http
404 Not Found
```

Empty body. The id is valid, but IGDB has no such game.

| Request | Status | Why |
|---|---|---|
| `GET {{baseUrl}}/api/games/0` | 404 | No game with id 0 |
| `GET {{baseUrl}}/api/games/-5` | 404 | No game with id -5 |
| `GET {{baseUrl}}/api/games/abc` | 404 | Route does not match (not an integer) |
| `GET {{baseUrl}}/api/games/1.5` | 404 | Route does not match |
| `GET {{baseUrl}}/api/games/2147483648` | 404 | Route does not match (over the int limit) |
| `GET {{baseUrl}}/api/games/1020/extra` | 404 | Route does not match |
| `POST {{baseUrl}}/api/games/1020` | 405 | Only `GET` is mapped |

All have an empty body, so a test cannot tell a malformed id from an unknown one.

```http
GET {{baseUrl}}/api/games/1020
```

```http
503 Service Unavailable
Content-Type: application/problem+json
```

```json
{
  "title": "Game data is currently unavailable.",
  "status": 503,
  "detail": "The game data service could not be reached."
}
```

---

## 3. Filter options: `GET /api/games/filters`

No parameters.

```http
GET {{baseUrl}}/api/games/filters
```

```http
200 OK
Content-Type: application/json
```

```json
{
  "genres": [
    { "value": "adventure", "name": "Adventure" },
    { "value": "arcade", "name": "Arcade" },
    { "value": "card-and-board-game", "name": "Card & Board Game" },
    { "value": "fighting", "name": "Fighting" }
  ],
  "platforms": [
    { "value": "6", "name": "PC (Microsoft Windows)" },
    { "value": "48", "name": "PlayStation 4" }
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

- `genres` and `platforms` are trimmed here. `genres` is the full IGDB list. `platforms` is a fixed list of 16 current and last-generation platforms.
- Genre `value` is a slug. Platform `value` is the numeric id **as a string**.
- `orderings` is always exactly these six, in this order.

### Errors

| Request | Status | Body |
|---|---|---|
| `GET {{baseUrl}}/api/games/FILTERS` | 200 | Same as above (routes are case-insensitive) |
| `POST {{baseUrl}}/api/games/filters` | 405 | Empty |
| IGDB unreachable | 503 | 503 problem body |

---

## 4. Company search: `GET /api/games/companies`

| Param | Required | Rules |
|---|:-:|---|
| `kind` | Yes | `developer` or `publisher`. Trimmed, case-insensitive. |
| `search` | Yes | 2-60 characters after trimming. |

Returns at most 10 companies, sorted by `gamesCount` (descending), then `name`. `gamesCount` counts games **developed** for `developer` and games **published** for `publisher`. `slug` is IGDB's own value, so treat it as opaque.

### Search developers

```http
GET {{baseUrl}}/api/games/companies?kind=developer&search=Nintendo
```

```http
200 OK
Content-Type: application/json
```

```json
[
  { "slug": "nintendo", "name": "Nintendo", "gamesCount": 674 },
  { "slug": "nintendo-entertainment-analysis-and-development", "name": "Nintendo Entertainment Analysis & Development", "gamesCount": 149 },
  { "slug": "nintendo-r-d1", "name": "Nintendo R&D1", "gamesCount": 111 }
]
```

The real response has 10 items. It is trimmed here.

### Search publishers

```http
GET {{baseUrl}}/api/games/companies?kind=publisher&search=Nintendo
```

```http
200 OK
```

```json
[
  { "slug": "nintendo", "name": "Nintendo", "gamesCount": 2889 },
  { "slug": "nintendo-of-america", "name": "Nintendo of America", "gamesCount": 44 },
  { "slug": "nintendo-of-europe", "name": "Nintendo of Europe", "gamesCount": 40 }
]
```

The same company can appear in both lists with a different `gamesCount`.

### Case and whitespace are ignored

```http
GET {{baseUrl}}/api/games/companies?kind=%20PUBLISHER%20&search=%20%20nintendo%20%20
```

```http
200 OK
```

Same body as the publisher search above.

### Length limits

```http
GET {{baseUrl}}/api/games/companies?kind=developer&search=ab
```

```http
200 OK
```

Two characters is the minimum. 60 characters is the maximum.

### No matches

```http
GET {{baseUrl}}/api/games/companies?kind=developer&search=zzzzzzzz
```

```http
200 OK
```

```json
[]
```

### Special and Unicode characters

```http
GET {{baseUrl}}/api/games/companies?kind=developer&search=a%22b%5Cc
GET {{baseUrl}}/api/games/companies?kind=developer&search=%E3%83%8B%E3%83%B3%E3%83%86%E3%83%B3%E3%83%89%E3%83%BC
```

```http
200 OK
```

Quotes and backslashes are escaped before they go to IGDB. Unicode is accepted.

### Validation errors

All return `400` with `Content-Type: application/problem+json`. Both parameters are checked, so both errors can appear together.

```http
GET {{baseUrl}}/api/games/companies
```

```http
400 Bad Request
Content-Type: application/problem+json
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "kind": ["kind must be 'developer' or 'publisher'."],
    "search": ["search must be between 2 and 60 characters."]
  }
}
```

```http
GET {{baseUrl}}/api/games/companies?kind=developer
```

```http
400 Bad Request
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "search": ["search must be between 2 and 60 characters."] }
}
```

```http
GET {{baseUrl}}/api/games/companies?search=Nintendo
```

```http
400 Bad Request
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "kind": ["kind must be 'developer' or 'publisher'."] }
}
```

More `400` cases:

| Request | `errors` has |
|---|---|
| `?kind=studio&search=Nintendo` | `kind` |
| `?kind=&search=` | `kind` and `search` |
| `?kind=developer&search=a` | `search` |
| `?kind=developer&search=%20a%20` | `search` (length 1 after trimming) |
| `?kind=developer&search=` + 61 × `a` | `search` |
| `?kind=developer&kind=publisher&search=Nintendo` | `kind` (repeated values are joined, so they don't match) |

### Other errors

| Request | Status | Body |
|---|---|---|
| `POST {{baseUrl}}/api/games/companies` | 405 | Empty |
| IGDB unreachable | 503 | 503 problem body |

---

## 5. 503 details

Any games endpoint returns this when the data provider can't be used:

```http
503 Service Unavailable
Content-Type: application/problem+json
```

```json
{
  "title": "Game data is currently unavailable.",
  "status": 503,
  "detail": "<one of the messages below>"
}
```

| Cause | `detail` |
|---|---|
| IGDB returned an error status, or the network failed | `The game data service could not be reached.` |
| `Igdb__ClientId` is empty | `The IGDB client id is not configured (Igdb__ClientId).` |
| `Igdb__ClientId` or `Igdb__ApiSecret` is empty when requesting a token | `The IGDB credentials are not configured (Igdb__ClientId, Igdb__ApiSecret).` |
| Twitch could not issue a token | `Could not get an IGDB access token from Twitch.` |
| Twitch returned an empty token | `Twitch returned an empty IGDB access token.` |
| Companies response was empty | `The game data service returned an empty company response.` |

---

## 6. Other

```http
GET {{baseUrl}}/api/games/unknown
```

```http
404 Not Found
```

Empty body.

```http
GET {{baseUrl}}/health
```

```http
200 OK
Content-Type: text/plain
```

```
ok
```

---

## 7. Caching (for service tests)

`IgdbGamesService` caches in memory. Keys are trimmed and lowercased.

| Method | Lifetime | Key | Upstream calls on a miss |
|---|---|---|---|
| Search | 5 min | search, page, pageSize | 2 (`games`, `games/count`) |
| Details | 1 hour | id | 1 |
| Companies | 30 min | kind, search | 1 |
| Filters | 12 hours | fixed | 2 (`genres`, `platforms`) |

- `Zelda` and ` zelda ` share one entry. A different page or `kind` is a separate entry.
- `null` results are not cached, so an unknown game id is looked up again on every request.
- Errors are not cached, so a `503` does not stick.
