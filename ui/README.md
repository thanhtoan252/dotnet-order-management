# Order Management UI

Angular 21 single-page application for the Order Management Microservices demo. The UI talks only to the API Gateway, not directly to individual services.

## Stack

| Area | Technology |
|---|---|
| Framework | Angular 21 standalone components, zoneless change detection, Angular Router |
| UI | PrimeNG 21, PrimeIcons, Lucide Angular, Tailwind CSS 4 |
| Server state | TanStack Angular Query |
| Forms | Angular Reactive Forms |
| Validation | Zod schemas and custom form validators |
| HTTP | Angular HttpClient with auth and error interceptors |
| Tests | Vitest through Angular CLI |

PrimeNG uses an Aura preset customized in `src/app/theme.ts` with an emerald primary color ramp. Global styles in `src/styles.css` put PrimeNG in a CSS layer before Tailwind utilities so layout and spacing utilities can override component defaults.

## Features

- Login form backed by `/api/auth/login`
- Authenticated app shell with responsive sidebar and topbar
- Products page with CRUD actions and `.xlsx` import
- Orders page with create, cancel, confirm, ship, and deliver workflows
- Inventory page with receive and adjust stock actions
- Admin-only users page for Keycloak user, password, and role management
- PrimeNG DynamicDialog-based forms and confirmation dialogs
- PrimeNG toast notifications through `NotificationService`
- API cache, loading, error, and mutation state via TanStack Angular Query

## Project Layout

```text
src/app/
  core/
    auth/          auth service, guards, token storage, permissions
    http/          interceptors and API error helpers
    layout/        shell, sidebar, topbar, navigation model
  features/
    auth/          login route and form
    products/      catalog pages, tables, dialogs, API/query layer
    orders/        order pages, tables, dialogs, API/query layer
    inventory/     stock pages, tables, dialogs, API/query layer
    users/         admin user management pages, tables, dialogs, API/query layer
  shared/
    ui/            shared dialogs
    validation/    reusable validation helpers
```

Each feature keeps its route definition, page component, data access, models, validation schemas, and feature-specific components together.

## Configuration

The development API base URL is in `src/environments/environment.ts`:

```ts
apiBaseUrl: 'http://localhost:8080/api'
```

Docker serves the built UI through Nginx and proxies API calls to the gateway according to `nginx.conf`.

## Development

Install dependencies:

```bash
npm install
```

Start the local development server on port 3000:

```bash
npm start
```

Build the app:

```bash
npm run build
```

Run unit tests:

```bash
npm test
```

The app expects the gateway and supporting services to be available from the repository-level Docker Compose setup.
