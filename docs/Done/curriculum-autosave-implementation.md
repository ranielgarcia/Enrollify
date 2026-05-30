# Curriculum Builder Auto-Save Implementation Plan

## Context
The `MultiYearSubjectGridEditor` component needs an auto-save mechanism to persist curriculum changes without requiring manual save clicks.

---

## Option 1: Debounced Save on Grid Change

**Approach**: Use `useEffect` with a debounce timer that triggers save whenever `grid` state changes.

```tsx
useEffect(() => {
  const timer = setTimeout(() => {
    saveCurriculumContentAsync({ grid });
  }, 1500); // 1.5s debounce

  return () => clearTimeout(timer);
}, [grid]);
```

**Pros**:
- Simple implementation
- Immediate feedback to users
- No additional state needed

**Cons**:
- Saves on every small change (potentially many API calls)
- No dirty state tracking
- Saves even if no actual changes from initial data

---

## Option 2: Dirty State with Debounced Save

**Approach**: Track if the grid has been modified from initial state, only auto-save when dirty.

```tsx
const [isDirty, setIsDirty] = useState(false);

// Mark dirty on any grid mutation
const updateGrid = (updater) => {
  setGrid(updater);
  setIsDirty(true);
};

useEffect(() => {
  if (!isDirty) return;
  
  const timer = setTimeout(() => {
    saveCurriculumContentAsync({ grid });
    setIsDirty(false);
  }, 2000);

  return () => clearTimeout(timer);
}, [grid, isDirty]);
```

**Pros**:
- Only saves when changes exist
- Can show "unsaved changes" indicator
- Fewer unnecessary API calls

**Cons**:
- More state to manage
- Slightly more complex logic
- Need to update all mutation functions

---

## Option 3: Save on Blur/Focus Loss

**Approach**: Save when user leaves the component or switches tabs.

```tsx
useEffect(() => {
  const handleBlur = () => {
    if (isDirty) saveCurriculumContentAsync({ grid });
  };

  window.addEventListener('blur', handleBlur);
  document.addEventListener('visibilitychange', handleBlur);

  return () => {
    window.removeEventListener('blur', handleBlur);
    document.removeEventListener('visibilitychange', handleBlur);
  };
}, [grid, isDirty]);
```

**Pros**:
- Minimal API calls
- Natural save points
- Works with browser close (with beforeunload)

**Cons**:
- User may lose work if they don't blur
- Less immediate feedback
- Doesn't cover all edge cases

---

## Option 4: Optimistic UI with Background Sync (React Query)

**Approach**: Leverage React Query's mutation with optimistic updates and retry logic.

```tsx
const { mutate, isPending } = useMutation({
  ...saveCurriculumContentOptions(curriculum.id),
  onMutate: async (newData) => {
    // Cancel outgoing refetches
    await queryClient.cancelQueries({ queryKey: ['curriculum', curriculum.id] });
  },
  retry: 3,
});

// Debounced auto-save
useDebouncedEffect(() => {
  if (isDirty) mutate({ grid });
}, [grid], 2000);
```

**Pros**:
- Built-in retry on failure
- Loading/error states from React Query
- Consistent with existing patterns

**Cons**:
- Requires custom `useDebouncedEffect` hook
- More complex setup
- Need to handle optimistic rollback

---

## Recommendation

**Option 2 (Dirty State with Debounced Save)** is recommended because:
1. Balances simplicity with efficiency
2. Enables "unsaved changes" UI indicator
3. Prevents unnecessary saves on initial load
4. Easy to extend with save status feedback

### Additional Considerations
- Add a visual indicator (e.g., "Saving...", "Saved ✓", "Save failed")
- Consider `beforeunload` event to warn about unsaved changes
- Debounce duration: 1.5-2 seconds is typical

---

## Estimated Effort
| Option | Complexity | Time Estimate |
|--------|------------|---------------|
| 1      | Low        | 30 min        |
| 2      | Medium     | 1 hour        |
| 3      | Medium     | 1 hour        |
| 4      | High       | 2 hours       |
