---
applyTo: "**/*.ts, **/*.tsx"
---

## Purpose

This file documents TypeScript guidelines and best-practices for the FabricLibrary frontend and any shared TypeScript code. The goal is readable, maintainable, safe code that integrates with the repo's tooling and CI.

## Brief contract

- Inputs: TypeScript/TSX source files in the `ui/` and shared directories.
- Outputs: Well-typed, linted, formatted code; passing unit tests; predictable build artifacts.
- Error modes: Type errors, lint failures, runtime prop-type mismatches.
- Success criteria: PRs conform to this guide, CI passes (lint + typecheck + tests), and reviewers rarely request basic style/type fixes.

## High-level rules

- Prefer strict typing and avoid `any` except when absolutely necessary; prefer narrow (specific) types.
- Use semicolons at the end of each statement.
- Use single quotes for strings.
- Prefer function-based React components and hooks (no class components).
- Use arrow functions for callbacks and short functions where appropriate.
- Keep files small and focused (one component / one hook / one module each).
- Export the smallest public API necessary; prefer named exports over many default exports for library code.
- Add documentation comments to explain non-obvious logic, public APIs, and complex algorithms.
- Add comments to explain decisions that are not obvious from the code itself.

## Project config (recommended)

- Enable TypeScript strict mode in `tsconfig.json`:
  - "strict": true
  - "noImplicitAny": true
  - "strictNullChecks": true
  - "noUnusedLocals": true
  - "noUnusedParameters": true
  - "noFallthroughCasesInSwitch": true
  - "forceConsistentCasingInFileNames": true
- Target modern ES (e.g., "es2020" or later) unless supporting older browsers.
- Module: "esnext" or "commonjs" depending on build tooling (match existing project).

## Linting and formatting

- Use ESLint + TypeScript parser (@typescript-eslint) and Prettier.
- Share rules:
  - Enforce single quotes and semicolons via Prettier config.
  - Turn on `@typescript-eslint/no-explicit-any` (allow only with comment justification).
  - Enforce consistent import ordering and grouping.
  - Prefer `const` for values that don't change; else `let`.
  - No unused vars (except prefixed `_` when intentionally unused).
- Include lint scripts in `package.json`:
  - "lint": "eslint 'src/**/*.{ts,tsx}'"
  - "format": "prettier --write 'src/**/*.{ts,tsx,json,md}'"

## Imports and module style

- Use absolute or tsconfig path aliases consistently if configured in the project.
- Prefer named imports:
  - Good: import { formatDate } from '../utils/date';
  - Avoid: import moment from 'moment'; (prefer light-weight libs where possible)
- Keep import groups in order: built-ins -> external libs -> internal modules -> styles/assets.

## Type patterns & utilities

- Use discriminated unions for variant types.
- Use `Readonly` / `ReadonlyArray` when exposing immutable APIs.
- Use `Record<K, V>` for map-like objects where appropriate.
- Prefer interface for exported object shapes and types for unions/aliases (team preference).
- Provide small utility types for common needs (e.g., `Nullable<T> = T | null`).

## React-specific rules

- Functional components only. 
- Avoid React.FC when it hides default props typing issues? If the project prefers a pattern, pick one consistently. (If you prefer: type props explicitly and return JSX.Element.)
- Use explicit prop types, avoid implicit `any` props.
- Prefer controlled components and lift state up responsibly.
- Use hooks for logic extraction; create small, focused custom hooks in `hooks/`.
- Keep effects idempotent and list all dependencies; use ESLint-plugin-react-hooks to enforce.
- Memoize expensive calculations with `useMemo` and callbacks with `useCallback` but do not prematurely optimize.
- Testing: write tests for components’ behavior (use React Testing Library + jest).

## Component / folder layout

- Co-locate component + styles + tests in a directory:
  - /src/components/ComponentName/
    - index.tsx (export)
    - ComponentName.tsx
    - ComponentName.module.css / .scss (or styled solution)
    - ComponentName.test.tsx
- Keep exports from `index.ts` for convenient imports: export { default as ComponentName } from './ComponentName';

## Styling & Material UI

- For Material UI (MUI) usage:
  - Prefer MUI system and styled components or theme-based styling.
  - Use theme tokens rather than hard-coded values.
  - Keep responsive rules within components or shared layout utilities.
- Provide design tokens in a central `theme/` folder and reference them across components.

## Error handling & async patterns

- Use async/await and handle errors explicitly with try/catch.
- Surface network errors and provide user-friendly messages.
- Centralize API calls in a `services/` module; return typed responses.
- Use abort signals for cancellable fetches when appropriate.

## Tests and typesafety in CI

- Tests: Jest + React Testing Library for UI; small unit tests for utils.
- Coverage: Enforce a baseline coverage (% configured in CI) but don’t require 100%.
- CI: run `eslint`, `typescript` type check (`tsc --noEmit`), and `jest --coverage` in PRs.

## Small examples

- React component (preferred style):
  - const ButtonPrimary = ({ label }: { label: string }): JSX.Element => {
      return <button>{label}</button>;
    };

- Narrowing and discriminated union:
  - type Shape =
      | { kind: 'circle'; radius: number }
      | { kind: 'square'; size: number };
    - function area(s: Shape) {
        if (s.kind === 'circle') return Math.PI * s.radius ** 2;
        return s.size ** 2;
      }

## PR checklist (add to PR template)
- [ ] Code compiles and typechecks (`tsc --noEmit`).
- [ ] ESLint passes and code formatted (`npm run lint` / `npm run format`).
- [ ] Unit tests added/updated; CI passes.
- [ ] New logic has small, readable tests (component + interaction).
- [ ] No new hard-coded theme/size values — use tokens.
- [ ] I documented public API types and decisions in the PR description.

## When to use `any` or `// @ts-ignore`
- Very rarely. If you must:
  - Add an inline justification comment and prefer to create an issue to remove the exception later.
  - Example: // TODO: narrow this type — using `any` to unblock migration (issue #123).

## Performance and bundle size
- Avoid large polyfills or heavy libs unless necessary.
- Prefer code-splitting for large routes and components.
- Tree-shakeable imports (import only what you need).

## Accessibility
- Use semantic HTML where possible.
- Ensure components accept `className`, `id`, and `aria-*` props when relevant.
- Use automated accessibility checks in tests (axe).

## Documentation and JSDoc
- Document tricky public functions/components with short JSDoc comments including parameter types and return type description.
- Add short examples when behavior is non-obvious.

## Common edge cases to watch
- Null/undefined props — use `strictNullChecks` and guard accordingly.
- Large lists — virtualize long lists for performance.
- Slow network — show fallback UI and loading states.
- Race conditions with async updates — use abort controllers or proper cleanup.

## Recommended toolchain (examples)
- TypeScript (latest supported stable)
- ESLint with @typescript-eslint
- Prettier
- Jest + React Testing Library
- Optional: Husky + lint-staged to run format/lint on commit

## Appendix — quick config snippets

- tsconfig.json (high level)
  - {
      "compilerOptions": {
        "target": "es2020",
        "module": "esnext",
        "strict": true,
        "moduleResolution": "node",
        "esModuleInterop": true,
        "forceConsistentCasingInFileNames": true,
        "skipLibCheck": true
      }
    }

- ESLint essentials
  - Use `eslint:recommended`, `plugin:@typescript-eslint/recommended`, `plugin:react/recommended`, and `plugin:react-hooks/recommended`.

## Follow-ups / optional additions
- Add a short migration guide if converting JS -> TS files.
- Add a pre-commit `lint-staged` configuration to enforce formatting.
- Provide a small starter `tsconfig.json` + `.eslintrc` in the repo if missing.

---