---
name: enrollify-resize-drawer
description: >
  Step-by-step guide for resizing right-side Drawer components in the Enrollify
  frontend. Use this skill when asked to make a form drawer wider, half-screen,
  near-full-screen, or full-screen. Covers how the vaul drawer primitive applies
  width, why sm:max-w-sm must be overridden, and the correct data-attribute
  className pattern to use on DrawerContent.
---

# Enrollify Resize Drawer Skill

Use this skill whenever a right-side form drawer (`direction="right"`) needs to
be resized. The technique differs from Dialog resizing — vaul applies width via
`data-[vaul-drawer-direction=right]:` data-attribute variants, and the default
`sm:max-w-sm` cap must be explicitly overridden.

---

## How the Default Width Works

The drawer primitive at `src/components/ui/drawer.tsx` applies this to every
right-direction drawer:

```
data-[vaul-drawer-direction=right]:w-3/4
data-[vaul-drawer-direction=right]:sm:max-w-sm   ← caps at ~384px on sm+
```

So even if you pass `w-[600px]`, the `sm:max-w-sm` cap will win on `sm` screens
and larger. **You must override both `w-*` and `sm:max-w-*` together.**

---

## Width Reference Table

| Goal                | className on `<DrawerContent>`                                                                    |
| ------------------- | ------------------------------------------------------------------------------------------------- |
| Default (no change) | _(omit className)_                                                                                |
| Slightly wider      | `"data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none"` |
| Standard wide form  | `"data-[vaul-drawer-direction=right]:w-[560px] data-[vaul-drawer-direction=right]:sm:max-w-none"` |
| Half-screen         | `"data-[vaul-drawer-direction=right]:w-1/2 data-[vaul-drawer-direction=right]:sm:max-w-none"`     |
| Near-full-screen    | `"data-[vaul-drawer-direction=right]:w-[85vw] data-[vaul-drawer-direction=right]:sm:max-w-none"`  |
| Full-screen         | `"data-[vaul-drawer-direction=right]:w-screen data-[vaul-drawer-direction=right]:sm:max-w-none"`  |

---

## Step-by-Step

1. Open the target `*-form-drawer.tsx` file.
2. Locate the `<DrawerContent>` element (no className by default).
3. Add a `className` with the desired width variant + `sm:max-w-none` override.
4. Run `npm run test:ts` from `src/system/enrollify-frontend/` to verify.

**Example — half-screen drawer:**

```tsx
<DrawerContent className="data-[vaul-drawer-direction=right]:w-1/2 data-[vaul-drawer-direction=right]:sm:max-w-none">
```

**Example — near-full-screen drawer:**

```tsx
<DrawerContent className="data-[vaul-drawer-direction=right]:w-[85vw] data-[vaul-drawer-direction=right]:sm:max-w-none">
```

**Example — full-screen drawer:**

```tsx
<DrawerContent className="data-[vaul-drawer-direction=right]:w-screen data-[vaul-drawer-direction=right]:sm:max-w-none">
```

---

## When to Use Each Width

| Width         | Use case                                                           |
| ------------- | ------------------------------------------------------------------ |
| `480–560px`   | Forms with 4–6 fields, no side-by-side layout needed               |
| `w-1/2` (50%) | Forms with many fields or a preview panel alongside the form       |
| `w-[85vw]`    | Complex multi-section forms, embedded data tables, or rich editors |
| `w-screen`    | Full-page workflows where the drawer replaces the whole view       |

---

## Reference Files

- Canonical drawer: `src/page-components/buildings-management-page/building-form-drawer.tsx`
- UI primitive: `src/components/ui/drawer.tsx`
  - Right-direction defaults are on the `data-[vaul-drawer-direction=right]:` lines inside `DrawerContent`
