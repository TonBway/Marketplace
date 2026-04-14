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

### 1. PostgreSQL

1. Create database `farm_marketplace`.
2. Run:
   - `database/migrations/001_init_marketplace.sql`
   - `database/seeds/001_seed_reference_data.sql`

### 2. Backend

1. Open `backend/`.
2. Configure `src/FarmMarketplace.Api/appsettings.Development.json`:
   - ConnectionStrings:MarketplaceDb
   - Jwt:Key
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

- `auth.users` is the single identity source for all roles.
- `seller.seller_profiles` extends seller-specific data.
- `messaging.buyer_profiles` extends buyer-specific data.
- One shared database and one shared authentication system for both apps.
- SQL in infrastructure services is parameterized and async.
- Foundation is prepared for future push notifications and payment gateway integrations.
