# Farm Marketplace Monorepo

Production-structured starter for:
- Seller Flutter app
- Buyer Flutter app
- Shared ASP.NET Core Web API
- Shared PostgreSQL database
- Standalone Blazor Web Portal

## Workspace Layout

- `backend/`
  - `FarmMarketplace.slnx`
  - `src/FarmMarketplace.Api`
  - `src/FarmMarketplace.Application`
  - `src/FarmMarketplace.Domain`
  - `src/FarmMarketplace.Infrastructure`
  - `src/FarmMarketplace.Contracts`
- `database/`
  - `migrations/001_init_marketplace.sql`
  - `seeds/001_seed_reference_data.sql`
   - `docker/`
      - `.env`
      - `docker-compose.yml`
      - `initdb/000_run_sql.sh`
- `portal/`
   - `FarmMarketplace.Portal.slnx`
   - `FarmMarketplace.Portal/`
- `mobile/`
  - `FarmMarketplace.SellerApp/`
  - `FarmMarketplace.BuyerApp/`
  - `shared_api/`

## Backend Stack

- ASP.NET Core Web API
- Dapper + Npgsql
- JWT + refresh tokens
- FluentValidation
- Serilog
- Swagger/OpenAPI
- Feature-aligned controllers and service interfaces

## API Modules Included

- Auth
- Seller Profiles
- Buyer Profiles
- Listings + Listing Images
- Subscriptions
- Enquiries
- Notifications
- Reference Data
- Dashboard Summaries

## Current Endpoint Surface

- POST `/api/auth/register`
- POST `/api/auth/login`
- POST `/api/auth/refresh`

- GET `/api/seller/profile/me`
- PUT `/api/seller/profile/me`

- GET `/api/buyer/profile/me`
- PUT `/api/buyer/profile/me`

- POST `/api/listings`
- GET `/api/listings/my`
- PATCH `/api/listings/{listingId}/status`
- POST `/api/listings/{listingId}/images/upload` (multipart/form-data)

- GET `/api/subscriptions/plans`
- GET `/api/subscriptions/active`
- POST `/api/subscriptions`

- POST `/api/enquiries`
- GET `/api/enquiries/received`
- GET `/api/enquiries/sent`
- PATCH `/api/enquiries/{enquiryId}/status`

- GET `/api/notifications/my`

- GET `/api/reference/regions`
- GET `/api/reference/districts?regionId={id}`
- GET `/api/reference/categories`

- GET `/api/dashboard/seller-summary`

## Setup

### 1. Local PostgreSQL with Docker

The repository includes a local PostgreSQL 16 setup for development in [database/docker/docker-compose.yml](database/docker/docker-compose.yml).

#### Start DB

From [database/docker](database/docker), run:

```bash
docker compose up -d
```

This starts:

- PostgreSQL 16
- container `farm-marketplace-postgres`
- database `farm_marketplace`
- persistent named volume `farm_marketplace_pgdata`

The container uses the values in [database/docker/.env](database/docker/.env):

- `POSTGRES_DB=farm_marketplace`
- `POSTGRES_USER=farm_user`
- `POSTGRES_PASSWORD=FarmMarket@123`
- `POSTGRES_PORT=5432`

On first initialization only, PostgreSQL runs the existing SQL scripts from:

- [database/migrations/001_init_marketplace.sql](database/migrations/001_init_marketplace.sql)
- [database/seeds/001_seed_reference_data.sql](database/seeds/001_seed_reference_data.sql)

The execution order is stable:

1. all SQL files in `database/migrations`
2. all SQL files in `database/seeds`

The helper script at [database/docker/initdb/000_run_sql.sh](database/docker/initdb/000_run_sql.sh) ensures the mounted folders are executed in that order during first database creation.

#### View logs

```bash
docker compose logs -f postgres
```

#### Stop DB

```bash
docker compose down
```

#### Reset DB completely

```bash
docker compose down -v
docker compose up -d
```

Use the reset only when you want PostgreSQL to recreate the database and rerun the initialization scripts from scratch.

#### Connect with psql

```bash
docker exec -it farm-marketplace-postgres psql -U farm_user -d farm_marketplace
```

#### Verify schemas and tables

Run these commands inside `psql`:

```sql
\dn
\dt auth.*
\dt seller.*
\dt marketplace.*
\dt billing.*
\dt messaging.*
```

### 2. Backend

1. Open `backend/`.
2. For local Docker PostgreSQL development, use this connection string in [backend/src/FarmMarketplace.Api/appsettings.Development.json](backend/src/FarmMarketplace.Api/appsettings.Development.json):
   - `Host=localhost;Port=5432;Database=farm_marketplace;Username=farm_user;Password=FarmMarket@123`
3. Keep production secrets out of source control. Only use this value for local development, and use environment variables or user secrets for other environments.
4. Configure `Jwt:Key` for local development if needed.
3. Run:
   - `dotnet restore FarmMarketplace.slnx`
   - `dotnet build FarmMarketplace.slnx`
   - `dotnet run --project src/FarmMarketplace.Api`
4. Open Swagger at `/swagger`.

### 3. Flutter Shared API Package

1. Open `mobile/shared_api`.
2. Run `flutter pub get`.

### 4. Portal (Standalone)

1. Open `portal/`.
2. Run:
   - `dotnet restore FarmMarketplace.Portal.slnx`
   - `dotnet build FarmMarketplace.Portal.slnx`
   - `dotnet run --project FarmMarketplace.Portal/FarmMarketplace.Portal.csproj`

### 5. Seller App

1. Open `mobile/FarmMarketplace.SellerApp`.
2. Run `flutter pub get`.
3. Run with API URL:
   - `flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000`

### 6. Buyer App

1. Open `mobile/FarmMarketplace.BuyerApp`.
2. Run `flutter pub get`.
3. Run with API URL:
   - `flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000`

## Notes

- The Docker PostgreSQL initialization scripts only run on first database creation. If the volume already exists, reset with `docker compose down -v` before recreating the container.
- `auth.users` is the single identity source for all roles.
- `seller.seller_profiles` extends seller-specific data.
- `messaging.buyer_profiles` extends buyer-specific data.
- One shared database and one shared authentication system for both apps.
- SQL in infrastructure services is parameterized and async.
- Foundation is prepared for future push notifications and payment gateway integrations.
