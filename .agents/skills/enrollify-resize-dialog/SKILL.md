---
name: enrollify-resize-dialog
description: >
  Quick reference for resizing Dialog and Drawer components in the Enrollify
  frontend. Use this skill when asked to make a dialog bigger, smaller, wider,
  taller, or to switch between a Dialog and a Sheet/Drawer. Covers DialogContent
  max-width classes, height control, scrollable content, and when to prefer a
  Drawer (Sheet) over a Dialog.
---

# Enrollify Resize Dialog / Drawer Skill

Use this skill whenever a dialog or modal needs to be resized — larger, smaller,
taller, or converted to a different surface (drawer/sheet).

---

## 1. Dialog Width (`DialogContent` className)

The width of a `DialogContent` is set via a single Tailwind class on the
`<DialogContent>` element. Replace the existing `sm:max-w-*` class with the
desired size:

| Class                | Approx. Width | When to use                          |
| -------------------- | ------------- | ------------------------------------ |
| `sm:max-w-sm`        | ~384 px       | Tiny confirmations                   |
| `sm:max-w-md`        | ~448 px       | Default (current project default)    |
| `sm:max-w-lg`        | ~512 px       | Slightly wider forms                 |
| `sm:max-w-xl`        | ~576 px       | Medium forms with multiple fields    |
| `sm:max-w-2xl`       | ~672 px       | Rich content / side-by-side sections |
| `sm:max-w-3xl`       | ~768 px       | Dashboards / data-heavy dialogs      |
| `sm:max-w-4xl`       | ~896 px       | Wide multi-column layouts            |
| `sm:max-w-5xl`       | ~1024 px      | Near full-width on desktop           |
| `w-[90vw] max-w-5xl` | 90% viewport  | Truly adaptive wide dialogs          |

**Example — make a dialog wider:**

```tsx
<DialogContent className="sm:max-w-2xl">
```

---

## 2. Dialog Height Control

By default, dialogs have no explicit height and grow with content. To cap height
and enable scrolling inside the dialog body:

```tsx
<DialogContent className="sm:max-w-2xl max-h-[80vh] flex flex-col">
  <DialogHeader>...</DialogHeader>

  {/* scrollable body */}
  <div className="flex-1 overflow-y-auto py-4 space-y-4">{/* content */}</div>

  <DialogFooter>...</DialogFooter>
</DialogContent>
```

Common max-height values: `max-h-[60vh]`, `max-h-[75vh]`, `max-h-[80vh]`, `max-h-[90vh]`.

---

## 3. Drawer (right-side sheet) Width

Drawer components used as form drawers (`direction="right"`) have their width
set on `<DrawerContent>`. The project's form drawers (e.g., `building-form-drawer.tsx`)
use the default width from `src/components/ui/drawer.tsx`.

To override the width, add a className on `<DrawerContent>`:

```tsx
<DrawerContent className="w-[480px] sm:w-[560px]">
```

Common Drawer widths:
| Class | Description |
|---|---|
| `w-[400px]` | Narrow (current default) |
| `w-[480px] sm:w-[540px]` | Standard form drawer |
| `w-[560px] sm:w-[640px]` | Wide form drawer (many fields) |
| `w-[40vw]` | Viewport-relative |
| `max-w-md` / `max-w-lg` | Tailwind max-width cap |

---

## 4. When to Switch from Dialog → Drawer

Prefer a **right-side Drawer** (Sheet) over a Dialog when:

- The form/content has more than ~5 fields
- The user needs to reference the page content behind the panel
- The action is a CRUD form (create/edit)

Prefer a **Dialog** when:

- Confirming a destructive action (delete dialogs)
- Displaying contextual info or small selectors
- Content is ≤ 4 fields or read-only

The project already uses `Drawer direction="right"` for all management-page form
drawers. See `building-form-drawer.tsx` as the canonical reference.

---

## 5. Step-by-Step Resize Process

1. Identify the component file (usually `*-dialog.tsx` or `*-form-drawer.tsx`).
2. For a **Dialog**: locate `<DialogContent className="...">` and swap the
   `sm:max-w-*` class.
3. For a **Drawer**: locate `<DrawerContent>` and add/update `className` with a
   `w-[Xpx]` value.
4. If the content is tall, add `max-h-[Xvh] flex flex-col` to `DialogContent`
   and wrap the body in `<div className="flex-1 overflow-y-auto">`.
5. Run `npm run test:ts` from `src/system/enrollify-frontend/` to verify no
   TypeScript errors were introduced.

---

## 6. Reference Files

- Dialog example: `src/components/enrollment-context/enrollment-context-dialog.tsx`
- Drawer example: `src/page-components/buildings-management-page/building-form-drawer.tsx`
- UI primitives: `src/components/ui/dialog.tsx`, `src/components/ui/drawer.tsx`
