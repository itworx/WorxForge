# WorxForge Global Sidebar Implementation Summary

## Overview

This document summarizes the implementation of the global authenticated sidebar and application shell for the WorxForge frontend.

The work was intentionally kept scoped to the shell and navigation architecture. It does not introduce fake user data, a second design system, a second router, or new state management. The sidebar is now owned by the authenticated application layout rather than by feature pages.

## Goals Achieved

- Added a single global sidebar owned by the main authenticated layout.
- Kept navigation configuration centralized and data-driven.
- Reused the existing route constants and router architecture.
- Added a dedicated primary action for Publish Asset.
- Added active-route handling derived from the URL rather than local state.
- Added desktop collapse behavior.
- Added mobile off-canvas drawer behavior.
- Added keyboard and screen-reader-friendly navigation semantics.
- Reused existing theme tokens from the global design system.
- Kept admin navigation separate from the user application shell.

## Architectural Changes

### 1. Main application shell

The authenticated shell is now implemented in `src/layouts/MainLayout.tsx`.

Responsibilities added to `MainLayout`:

- Persistent desktop sidebar placement.
- Collapsed desktop sidebar state.
- Mobile drawer open/close behavior.
- Mobile overlay/backdrop behavior.
- Escape-to-close support.
- Focus handoff to the mobile close button when the drawer opens.
- Route-change close behavior on mobile.
- Page-content containment so feature pages do not manage sidebar spacing.

This keeps the sidebar as part of the application shell instead of duplicating it in feature pages.

### 2. Shared application sidebar

A new shared component was added at `src/components/AppSidebar.tsx`.

Responsibilities of `AppSidebar`:

- Render the navigation UI.
- Read navigation items from centralized config.
- Render the primary Publish Asset action.
- Render primary navigation and lower account links.
- Apply active-state styling using the router location.
- Apply `aria-current="page"` to the active route.
- Show unread notification badge state using `NotificationContext`.
- Support expanded, collapsed, and mobile drawer presentations.

### 3. Centralized navigation metadata

The empty navigation config at `src/navigation/navigation.config.ts` was replaced with a single source of truth for sidebar items.

Metadata added includes:

- `label`
- `path`
- `icon`
- `group`
- `match`
- `excludePaths`
- `emphasized`
- `showNotificationBadge`

This metadata is intentionally minimal and only supports the behavior required by the sidebar.

## Routes and Navigation Changes

### Route constants

The route constants file was extended to add the missing publish/create destination:

- `ROUTES.MY_ASSETS_CREATE = "/my-assets/create"`

This follows the existing route naming convention more closely than inventing a new unrelated top-level path, because the feature page already exists as `CreateAssetPage` and belongs conceptually under the user's owned assets area.

### Router updates

The router in `src/routes/appRouter.tsx` was updated to:

- Register `CreateAssetPage` under the main authenticated route tree.
- Add an index redirect from `/` to `/home`.
- Reuse centralized route constants instead of hardcoding more route strings.

### Final primary navigation

The sidebar now renders these user-facing primary items in the required order:

1. Publish Asset
2. Home
3. Explore
4. My Library
5. My Assets
6. Notifications

The lower account section includes:

- Profile
- Settings

No fake user identity information was added.

## Active State Behavior

Active state is derived from the current router path.

Behavior implemented:

- `Home`, `Explore`, `My Library`, `Notifications`, `Profile`, and `Settings` use exact matching.
- `My Assets` uses prefix matching so nested owned-asset routes can still map naturally to the section.
- `Publish Asset` is treated as its own exact active destination.
- `My Assets` explicitly excludes `/my-assets/create` so `Publish Asset` and `My Assets` do not appear active at the same time.

This avoids duplicate visual selection and keeps the primary action distinct.

## UI and UX Decisions

### Design system usage

The implementation uses the existing token system from `src/styles/global.css`, especially:

- `sidebar`
- `sidebar-foreground`
- `sidebar-primary`
- `sidebar-primary-foreground`
- `sidebar-accent`
- `sidebar-accent-foreground`
- `sidebar-border`
- `sidebar-ring`
- base `background`, `foreground`, `border`, and `ring`

No new color palette or theme layer was added.

### Visual behavior

The sidebar styling was kept aligned with the existing product direction:

- Clean and minimal structure.
- Compact spacing consistent with the rest of the app.
- Light borders.
- Low shadow usage.
- Moderate radius.
- Short transitions.
- No aggressive transforms or decorative animation.

### Publish Asset treatment

`Publish Asset` is visually emphasized as a primary action using sidebar primary tokens while remaining part of the same navigation system.

### Collapsed desktop mode

The desktop sidebar supports icon-only collapse.

Collapsed behavior includes:

- Preserved navigation functionality.
- Preserved active states.
- Preserved accessible names via `aria-label`.
- Labels exposed through `title` for compact mode.
- Consistent icon sizing.

### Mobile drawer behavior

On mobile, the sidebar becomes an off-canvas drawer.

Behavior implemented:

- Hidden by default.
- Opened by a shell-level menu trigger.
- Closed by the backdrop.
- Closed by the close button.
- Closed by `Escape`.
- Closed automatically after navigation.
- Focus moved into the drawer when opened.
- Main content marked inert while the drawer is open.
- Body scrolling prevented while the drawer is open.

## Accessibility Work

The shell/sidebar implementation includes these accessibility considerations:

- Semantic navigation regions.
- Logical keyboard tab order.
- Focus-visible ring styling.
- `aria-current="page"` on active navigation links.
- Decorative icons marked with `aria-hidden`.
- Accessible labels for the mobile open/close controls.
- Accessible labels preserved in collapsed mode.
- Mobile dialog semantics for the off-canvas drawer.

## Notification Integration

The sidebar reads unread notification count from `NotificationContext`.

Behavior:

- If `unreadCount > 0`, a badge is shown.
- If `unreadCount === 0`, no unnecessary badge is shown.
- No separate notification state was added inside the sidebar.

This remains architecture-ready even though the notification context is still a placeholder today.

## Files Added or Changed

### Added

- `src/components/AppSidebar.tsx`
- `src/layouts/MainLayout.test.tsx`
- `docs/archive/GLOBAL_SIDEBAR_IMPLEMENTATION.md`

### Changed

- `src/layouts/MainLayout.tsx`
- `src/navigation/navigation.config.ts`
- `src/constants/routes.constants.ts`
- `src/routes/appRouter.tsx`
- `vite.config.ts`

## Testing and Validation

### Build validation

The frontend build passed successfully using:

- `npm run build`

### Test validation

Focused shell tests were added and passed using:

- `npm run test`

The tests cover:

- Publish Asset active state without also activating My Assets.
- Collapsed sidebar accessibility behavior.
- Mobile drawer open, focus handoff, and close-on-navigation behavior.

### Lint validation

Lint completed successfully using:

- `npm run lint`

The only remaining lint output was the existing fast-refresh warning pattern in:

- `src/contexts/AuthContext.tsx`
- `src/contexts/SessionContext.tsx`
- `src/contexts/NotificationContext.tsx`

These warnings were pre-existing and are not caused by the sidebar implementation.

## Vitest Environment Adjustment

The default test command initially failed in this Windows environment because Vitest worker forks timed out while starting the new test file.

To keep the standard project test command stable, `vite.config.ts` was updated to set:

- `test.pool = "threads"`

This was a minimal environment-level fix so that `npm run test` passes normally without requiring a custom command.

## Known Limitation

The authenticated shell is implemented correctly at the layout/router level, but the current auth flow in the repository is still placeholder-only.

Specifically:

- `ProtectedRoute` still hardcodes unauthenticated behavior.
- `AuthContext` still exposes placeholder state.
- `NotificationContext` still exposes placeholder unread count.

Because of that, the shell is structurally ready but the app will still behave according to the existing placeholder auth implementation until that separate work is completed.

This limitation was intentionally left untouched to keep scope focused on the shell/sidebar task only.

## Outcome

WorxForge now has a centralized, reusable, token-driven global sidebar architecture for authenticated user routes.

The result is:

- layout-owned rather than feature-owned
- responsive across desktop and mobile
- accessible by keyboard and screen readers
- router-driven for active state
- consistent with existing routing and design-token architecture
- separated cleanly from admin navigation
- covered by focused automated tests

## TODO

- Add a `Logout` button to the sidebar account section and wire it to the upcoming login/auth flow once authentication state and logout handling are implemented.
