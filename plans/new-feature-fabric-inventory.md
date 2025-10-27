# Fabric Inventory — Planning document

## Summary
This project is a web application for managing a personal inventory of fabric. Users sign in with Google, add fabric records (including image, name, quantity, unit, purchase location, notes, and tags), and manage/share their inventory with others on a read-only basis. The stack will be React + Material-UI for the frontend and .NET (ASP.NET Core) for the backend. Images will be stored in cloud object storage; provider to be selected.

## Goals
- Fast, simple MVP that allows an Owner to sign in with Google and perform CRUD on their fabric inventory.
- Support fractional quantities and multiple units of measure (MVP units: yard, fat quarter, scrap, none).
- Allow Owners to share read-only access with viewers.
- Store images in cloud object storage, served securely (signed URLs / CDN).
- Provide export (CSV/JSON) and a clear path to future features (multiple images, imports, offline sync).

## Personas
- Owner (Home Sewist): personal inventory, needs quick add/edit, tagging, and search.
- Viewer (Shared Guest): read-only access to a shared inventory.
- Admin (future): platform-level management and support.

## Assumptions
- Single-user-per-account (no multi-tenant teams in MVP).
- Google sign-in only for MVP; additional providers possible later.
- One image per fabric in MVP; multiple images planned later.
- Images stored in cloud object storage (provider TBD). Design is provider-agnostic initially.
- Fractional quantities allowed; units are canonical enum values with future extensibility.
- Basic last-write-wins concurrency for MVP.

## Core user stories (MVP)
1. Authentication — Google sign-in
   - Owner can sign in with Google and land on their dashboard.
   - Backend validates Google ID tokens and links/creates user.

2. Add fabric
   - Owner can add name, image, quantity (fractional), unit, purchase location, notes, tags.
   - Quantity > 0 for applicable units; unit chosen from canonical list.

3. Edit fabric
   - Owner can update any field, replace image, and save changes.

4. Delete fabric
   - Owner can delete fabric with confirmation; images scheduled for deletion/archival.

5. View gallery & detail
   - Gallery shows cards with thumbnail, name, quantity+unit, tags.
   - Detail shows full image, notes, purchase info, and tags.

6. Search & filter
   - Search by name and tags; filters by unit and purchase location; sorting options.

7. Share inventory (basic)
   - Owner can grant read-only access to a person (invite by email or share link).
   - Viewer sees read-only UI; cannot modify.

8. Export inventory
   - Owner can export CSV or JSON including image URLs (not embedded).

## Acceptance criteria (sample)
- Add/Edit: name required, quantity numeric (decimal), unit one of enum values, tags optional.
- Images: types jpg/png/webp, size limit ~8–12 MB (configurable).
- Sharing: explicit owner grant required; no public access by default.
- Exports: include all fabric fields and image references.

## Data model (high-level)
- User
  - id (UUID), google_sub, email, display_name, settings (jsonb), created_at, updated_at.

- Fabric
  - id (UUID), user_id (FK), name, quantity (numeric), unit (enum), purchase_location, purchase_date, price, notes, created_at, updated_at, deleted_at.

- Image (metadata)
  - id (UUID), fabric_id (FK), storage_key, content_type, size_bytes, width, height, uploaded_at, thumbnail_key (optional).

- Tag / FabricTag
  - tags table and many-to-many join.

- ShareGrant
  - id (UUID), owner_user_id, grantee_user_id (nullable), grantee_email, permission, created_at, accepted_at, expires_at.

Notes:
- Use UUIDs for public-safe IDs.
- Use numeric(12,4) for quantity (adjust scale as needed).
- Soft deletes recommended for fabrics and images.

## Example DB schema (Postgres, simplified)
- users, fabrics, images, tags, fabric_tags, share_grants.
- Recommend Postgres for JSONB support and robust tooling. Use `numeric` for fractional quantities.

## API contract (high-level)
Authentication:
- POST /api/auth/google
  - Body: { id_token }
  - Validates Google token, returns app session token (optional).

Fabrics:
- GET /api/fabrics?q=&tag=&unit=&purchase_location=&sort=&page=&pageSize=
  - Paginated list with thumbnails.

- POST /api/fabrics
  - Body: { name, quantity, unit, purchase_location?, purchase_date?, price?, notes?, tags?: [], image_id? }
  - Returns created fabric (201).

- GET /api/fabrics/{id}
  - Returns full fabric with image metadata.

- PUT /api/fabrics/{id}
  - Update fields.

- DELETE /api/fabrics/{id}
  - Soft-delete (204).

Images:
- POST /api/images/presign
  - Body: { filename, contentType, sizeBytes } -> returns { imageId, uploadUrl, storageKey }.

- POST /api/images/complete
  - Body: { imageId, storageKey, width, height, sizeBytes } -> attach metadata to Fabric.

Tags:
- GET /api/tags (autocomplete)

Sharing:
- POST /api/share
  - Body: { fabricIds?, granteeEmail } -> creates ShareGrant and queues invitation.

Export:
- GET /api/export?format=csv|json -> returns downloadable payload.

Error handling:
- 400 validation, 401 unauthorized, 403 forbidden, 404 not found, 500 server error.

## Image storage & upload flow
- Preferred: direct client upload with signed URLs (backend issues presigned URL, client uploads; backend finalizes metadata).
- Alternative: proxied upload through backend (simpler client, more backend cost).
- Thumbnail generation: background job (serverless or worker).
- CDN recommended for delivery (use signed URLs/cookies for private images).

Provider guidance:
- Azure Blob Storage + Azure CDN integrates well with .NET/.NET hosting on Azure.
- AWS S3 + CloudFront is a broadly supported alternative.
- GCS is an option (especially if you prefer Google Cloud).
- For MVP, design provider-agnostic storage abstraction; pick provider later.

## UI pages & components (React + MUI)
- Login page (Google sign-in)
- Dashboard (summary, recent fabrics)
- Gallery (cards with filters/search)
- Fabric detail page (full info, image, edit/delete for Owner)
- Add/Edit Fabric modal or page (form with validation)
- Settings (account, export, share management)
- Shared view (read-only)

Components:
- FabricCard, FabricList, FabricForm, ImageUploader, TagInput, SearchBar, FilterPanel, ShareDialog.

## Milestones & rough estimates (MVP)
Milestone 1 — Project setup & auth (1 week)
- Repo skeleton, CI, Google OAuth integration, basic user model.

Milestone 2 — CRUD & DB (1–2 weeks)
- Fabric model, API endpoints, React forms, create/read/update/delete flows.

Milestone 3 — Images & upload (1 week)
- Presigned uploads, image metadata, thumbnailing pipeline.

Milestone 4 — Search/filters, tags, export (1 week)
- Server-side search, filters, CSV/JSON export.

Milestone 5 — Sharing & invite flow (1 week)
- ShareGrant, invite emails, read-only access.

Buffer & polish (1 week)
- Tests, documentation, basic accessibility, small UX polish.

Total MVP estimate: ~5–7 weeks (single developer, part-time assumptions). Adjust based on team size and parallel work.

## Tests & CI
- Unit tests for backend models and API (xUnit / NUnit for .NET).
- Integration tests for core APIs (EF Core + test database or Dockerized Postgres).
- Frontend tests: Jest + React Testing Library for components.
- CI: GitHub Actions pipeline to run lint, build, and tests.

## Security & privacy
- Verify Google ID tokens server-side.
- Private image storage by default; use signed URLs for access.
- Rate-limit presign and upload endpoints.
- Provide account deletion and data export paths.

## Next steps
- Pick cloud provider for storage (or keep provider-agnostic for now; recommend Azure for smooth .NET integration).
- Implement Auth architecture and token/session strategy.
- Create repo skeleton (`/ui`, `/backend`, `/plans`, `/docs`) and add this file to `/plans`.
- Start Milestone 1 (Auth + skeleton).

## Open questions to resolve before implementation
- Exact default units and whether unit conversion is required (MVP: no conversion; just canonical unit values).
- Share invitation flow: email invite vs. shareable link (MVP: email invites tied to Google account).
- Image retention policy on replacements/deletions.
