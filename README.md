# Inventory Management System

A full-stack inventory management application for maintaining items, recording stock receipts/issues, reviewing balances, and producing stock and transaction reports.

## Technologies

- **Frontend:** React 19, TypeScript, Vite, React Router, Decimal.js, ESLint, and Prettier.
- **Backend:** ASP.NET Core Minimal API on .NET 10, C#, FluentValidation, Entity Framework Core, and Scalar OpenAPI UI.
- **Data and reporting:** SQL Server LocalDB, EF Core SQL Server/InMemory providers, RDLC reports via ReportViewerCore.
- **Testing:** xUnit unit, integration, and BDD test projects.

## Prerequisites

- Node.js (current LTS recommended) and npm.
- .NET 10 SDK.
- SQL Server LocalDB (the default development configuration uses `(localdb)\\MSSQLLocalDB`).

## Setup

1. Clone or download the repository(https://github.com/hossainalazad90/InventoryManagement).
2. Configure the backend connection string if LocalDB is unavailable. Update `backend/src/Inventory.Api/appsettings.json` or use a local override such as `appsettings.Local.json` (ignored by Git).
3. Configure the frontend API endpoint. Create `frontend/inventory-web/.env` with:

   ```env
   VITE_API_URL=https://localhost:7024/api/v1
   ```

4. Install frontend dependencies:

   ```powershell
   cd frontend/inventory-web
   npm install
   ```

5. Restore backend dependencies:

   ```powershell
   cd backend
   dotnet restore InventoryManagement.sln
   ```

## Run locally

Start the API first. It creates and seeds the development database on startup.

```powershell
cd backend
dotnet run --project src/Inventory.Api
```

The API runs at `https://localhost:7024` (and `http://localhost:5106`). In Development, its interactive API reference is available at `https://localhost:7024/scalar/v1`.

In another terminal, start the frontend:

```powershell
cd frontend/inventory-web
npm run dev
```

Open the Vite address shown in the terminal (normally `http://localhost:5173`).

Useful checks:

```powershell
# Frontend
cd frontend/inventory-web
npm run lint
npm run build

# Backend
cd backend
dotnet test InventoryManagement.sln
```

## Implementation approach

The backend follows a layered structure:

- **Domain** contains inventory entities and business concepts.
- **Application** exposes DTOs, validation, and services for items, stock, transactions, and reports.
- **Infrastructure** implements persistence, database initialization/seeding, and RDLC report generation.
- **API** maps minimal API endpoint groups under `/api/v1`, configures CORS, centralizes exception handling, and exposes OpenAPI in development.

The frontend is a single-page React application with route-based pages for the dashboard, items, stock, transactions, and reports. Typed service modules call the API through a shared client, while the UI keeps navigation and page concerns separate from HTTP access. Stock movements are captured from receive and issue transactions, allowing balances and reports to be derived from the movement data rather than manually edited totals.

## Assumptions

- Local development uses SQL Server LocalDB and the included startup seed data.
- The frontend and API run on the default development URLs listed above; `VITE_API_URL` may be changed for another environment.
- The API is intended to be run in the Development environment while using the local Scalar API reference.
- Database scripts under `backend/database` are available for environments that prefer explicit SQL setup.

## Limitations

- Authentication and authorization are not implemented; the development CORS policy allows all origins, methods, and headers.
- Database setup uses `EnsureCreated` and seed data rather than a production migration/deployment workflow.
- LocalDB is Windows-oriented and is not suitable for a shared or production database.
- Report rendering depends on the included RDLC templates and its .NET-compatible report viewer library.
- Environment-specific deployment, secret management, monitoring, and CI/CD configuration are outside the current scope.
