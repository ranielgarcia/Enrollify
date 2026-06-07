# Browser Scrollbar & Data Table Viewport Layout Changes

## Overview

This document describes the layout changes made to switch all pages from using custom inner-container scrollbars to the browser's native scrollbar, while preserving the data table's ability to expand and fill the available viewport width and height.

## Problem

The previous layout used `h-svh overflow-hidden` on the top-level `SidebarProvider`, locking the entire portal to the viewport height. This forced every inner content area (e.g. `ManagementPageLayout`, `PageContentContainer`) to handle scrolling via `overflow-y-auto` on nested `<div>` elements — resulting in custom scrollbars instead of the browser's built-in one.

## Solution

1. Remove the viewport lock from the shell layout so the browser scrollbar is used for all pages.
2. Remove `overflow-y-auto` from inner layout containers.
3. Introduce a `useViewportFillHeight` hook that dynamically calculates the remaining viewport height for the data table, so it still fills the available space and scrolls internally.

---

## Changed Files

### 1. `src/routes/portal/route.tsx`

**What changed:** Removed `className="h-svh overflow-hidden"` from `<SidebarProvider>`.

**Why:** The `h-svh` class locked the wrapper to exactly `100svh` and `overflow-hidden` prevented any content from exceeding the viewport, blocking the browser scrollbar entirely. The `SidebarProvider`'s internal wrapper already applies `min-h-svh`, so the layout remains at least full viewport height but can now grow when content overflows.

**Before:**

```tsx
<SidebarProvider className="h-svh overflow-hidden">
```

**After:**

```tsx
<SidebarProvider>
```

---

### 2. `src/components/page-layouts/management-page-layout.tsx`

**What changed:** Removed `overflow-y-auto` from the children wrapper `<div>`.

**Why:** This class created a custom scrollbar on the content area below the page header. With the viewport lock removed at the shell level, this is no longer needed — the browser scrollbar handles page-level scrolling instead.

**Before:**

```tsx
<div className="flex min-h-0 flex-1 flex-col overflow-y-auto py-5">{children}</div>
```

**After:**

```tsx
<div className="flex min-h-0 flex-1 flex-col py-5">{children}</div>
```

---

### 3. `src/components/page-layouts/page-content-container.tsx`

**What changed:** Removed `overflow-y-auto` from the `<main>` element.

**Why:** Same rationale as above — this custom scrollbar is replaced by the browser's native scrollbar. This component is currently unused but was updated for consistency.

**Before:**

```tsx
<main className="flex min-h-0 min-w-0 flex-1 flex-col gap-4 overflow-y-auto pt-0">
```

**After:**

```tsx
<main className="flex min-h-0 min-w-0 flex-1 flex-col gap-4 pt-0">
```

---

### 4. `src/hooks/use-viewport-fill-height.ts` *(new file)*

**What it does:** A React hook that calculates the remaining viewport height from an element's top position to the bottom of the viewport.

**How it works:**

- Uses a `ref` attached to the target element.
- On mount (via `useLayoutEffect`), it calls `getBoundingClientRect().top` to measure the element's distance from the viewport top.
- Computes `height = window.innerHeight - top - bottomOffset`.
- A `ResizeObserver` watches the full ancestor chain, so the height recalculates when any parent resizes (e.g. sidebar collapse/expand transitions).
- A `window.resize` listener handles viewport size changes.
- A guard (`Math.abs(prev - newHeight) < 1`) prevents sub-pixel oscillation loops.

**Parameters:**

| Parameter      | Default | Description                                            |
| -------------- | ------- | ------------------------------------------------------ |
| `bottomOffset` | `0`     | Pixels to reserve below the element (e.g. for padding) |
| `minHeight`    | `200`   | Minimum height in pixels to prevent collapse            |

**Returns:** `{ ref, height }` — attach `ref` to the element; apply `height` as an inline style.

---

### 5. `src/components/data-table/data-table.tsx`

**What changed:**

- Imported and used `useViewportFillHeight` to set an explicit `height` on the data table wrapper.
- Removed `flex-1` and `min-h-0` from the wrapper's class list (no longer needed since the height is now set explicitly rather than derived from a flex chain).

**Why:** Without the viewport-locked flex chain, the data table would grow to its natural content height instead of filling the remaining viewport space. The hook provides a dynamic, explicit height that achieves the same viewport-filling behavior.

**Before:**

```tsx
<div
  className={cn(
    "flex min-h-0 min-w-0 w-full flex-1 flex-col gap-2.5",
    className,
  )}
  {...props}
>
```

**After:**

```tsx
const { ref, height } = useViewportFillHeight<HTMLDivElement>(20);

<div
  ref={ref}
  style={height ? { height } : undefined}
  className={cn(
    "flex min-w-0 w-full flex-col gap-2.5",
    className,
  )}
  {...props}
>
```

The `bottomOffset` of `20` matches the `py-5` (20px) bottom padding on the `ManagementPageLayout` children wrapper, so the data table ends flush with the viewport edge.

---

## Resulting Behavior

| Page Type                          | Scrollbar Behavior                                                                 |
| ---------------------------------- | ---------------------------------------------------------------------------------- |
| Management pages (with data table) | Data table fills remaining viewport height, scrolls internally. No page scrollbar. |
| Non-table pages (home, curriculum) | Content flows naturally. Browser scrollbar appears when content exceeds viewport.  |
| Sidebar collapse/expand            | `useViewportFillHeight` recalculates via `ResizeObserver` on ancestor chain.       |
| Window resize                      | Hook recalculates via `window.resize` listener.                                    |
