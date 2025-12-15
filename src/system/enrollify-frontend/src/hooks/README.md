# Custom React Hooks Documentation

This folder contains custom React hooks for API communication with automatic type inference and Azure AD authentication.

## Table of Contents

- [useAppQuery](#useappquery) - For GET requests
- [useAppMutation](#useappmutation) - For POST, PUT, DELETE requests

---

## useAppQuery

A type-safe hook for making GET requests with automatic Azure AD token management.

### Import

```typescript
import useAppQuery from "@/hooks/use-app-query-v2";
```

### Basic Usage

```typescript
// Simple GET request
const { data, isLoading, error } = useAppQuery({
  path: "/api/room-types",
});

// With query parameters (when your GET endpoint has query params defined)
const { data, isLoading, error } = useAppQuery({
  path: "/api/room-types",
  params: {
    pageNumber: 1,
    pageSize: 10,
  },
});
```

### Parameters

| Parameter           | Type                          | Required | Default | Description                                                    |
| ------------------- | ----------------------------- | -------- | ------- | -------------------------------------------------------------- |
| `path`              | `ApiPath`                     | ✅       | -       | The API endpoint path (type-safe from OpenAPI spec)            |
| `params`            | `QueryParamsForPath<P>`       | ❌       | -       | Query parameters (type-safe based on path's GET operation)     |
| `queryOptions`      | `UseQueryOptions`             | ❌       | -       | TanStack Query options (enabled, staleTime, refetchInterval, etc.) |
| `forceRefreshToken` | `boolean`                     | ❌       | `false` | Force refresh the Azure AD token                               |

> **Note:** The `params` type is inferred from the GET operation's query parameters defined in your OpenAPI spec, not from path-level parameters.

### Advanced Examples

```typescript
// With custom query options
const { data, isLoading } = useAppQuery({
  path: "/api/room-types",
  params: {
    id: roomTypeId, // Query param as defined in your OpenAPI spec
  },
  queryOptions: {
    enabled: !!roomTypeId, // Only run query when roomTypeId exists
    staleTime: 5 * 60 * 1000, // 5 minutes
    refetchOnWindowFocus: false,
  },
});

// With custom queryKey for cache management
const { data } = useAppQuery({
  path: "/api/room-types",
  params: { departmentId: deptId },
  queryOptions: {
    queryKey: ["room-types", "department", deptId],
  },
});

// Force token refresh (useful after permission changes)
const { data } = useAppQuery({
  path: "/api/me",
  forceRefreshToken: true,
});
```

**Important Notes for useAppQuery:**
- The `path` should match exactly what's in your OpenAPI spec (e.g., `/api/room-types`, `/api/me`)
- Query parameters (like `id`, `pageNumber`, `pageSize`) go in the `params` prop
- Don't use path templates like `/api/room-types/{id}` - use `params: { id: value }` instead
- The `params` type is automatically inferred from the GET operation in your OpenAPI spec

### Return Value

Returns all properties from TanStack Query's `useQuery`:

- `data` - The response data (fully typed)
- `isLoading` - Loading state
- `error` - Error object
- `refetch` - Function to manually refetch
- `isFetching` - Fetching state (including background refetches)
- And more...

---

## useAppMutation

A type-safe hook for making POST, PUT, and DELETE requests with automatic Azure AD token management.

### Import

```typescript
import useAppMutation from "@/hooks/use-app-mutation-v2";
```

### Basic Usage

```typescript
// POST request (default)
const { mutate, isPending } = useAppMutation({
  mutationKey: "createRoomType",
  path: "/api/room-types",
});

// Execute mutation
mutate({ name: "Lecture Hall", description: "Large classroom for lectures" });
```

### Parameters

| Parameter                  | Type                                  | Required | Default   | Description                                                    |
| -------------------------- | ------------------------------------- | -------- | --------- | -------------------------------------------------------------- |
| `path`                     | `ApiPath`                             | ✅       | -         | The API endpoint path (type-safe from OpenAPI spec)            |
| `mutationKey`              | `string \| readonly unknown[]`        | ✅       | -         | Unique key for the mutation (for cache invalidation)           |
| `httpVerb`                 | `"post" \| "put" \| "delete"`         | ❌       | `"post"`  | HTTP method to use                                             |
| `params`                   | `QueryParamsForPathVerb<P, V>`        | ❌       | -         | Query parameters (type-safe based on path AND httpVerb)        |
| `isMultipart`              | `boolean`                             | ❌       | `false`   | Set to `true` for file uploads (multipart/form-data)           |
| `onUploadProgressCallBack` | `(e: AxiosProgressEvent) => void`     | ❌       | -         | Callback for upload progress (useful for progress bars)        |
| `forceRefreshToken`        | `boolean`                             | ❌       | `false`   | Force refresh the Azure AD token                               |

> **Note:** The `params` type is inferred based on BOTH the `path` AND `httpVerb`. Different HTTP verbs on the same path may have different query parameters defined in the OpenAPI spec.

---

## HTTP Verb Examples

### POST Request (Create)

```typescript
const { mutate, mutateAsync, isPending, isError, error } = useAppMutation({
  mutationKey: "createRoomType",
  path: "/api/room-types",
  httpVerb: "post", // Optional, "post" is the default
});

// Using mutate (fire-and-forget with callbacks)
mutate(
  { name: "Lecture Hall", description: "Large classroom for lectures" },
  {
    onSuccess: (data) => {
      toast.success("Room type created successfully!");
      queryClient.invalidateQueries({ queryKey: ["/api/room-types"] });
    },
    onError: (error) => {
      toast.error(`Failed to create room type: ${error.message}`);
    },
  }
);

// Using mutateAsync (returns Promise, useful with async/await)
const handleSubmit = async (formData: RoomTypeFormData) => {
  try {
    const result = await mutateAsync(formData);
    console.log("Created room type:", result);
  } catch (error) {
    console.error("Failed:", error);
  }
};
```

### PUT Request (Update)

For PUT requests, the `params` prop is used to pass query parameters (like `id`) that are defined in your OpenAPI spec. The path remains the same as defined in your API spec (e.g., `/api/room-types`), not a templated path like `/api/room-types/{id}`.

```typescript
// The params type is inferred from the OpenAPI spec for the PUT operation
// For /api/room-types PUT, the spec defines: parameters: { query: { id: number } }
const { mutate, mutateAsync, isPending } = useAppMutation({
  mutationKey: ["updateRoomType", roomTypeId],
  path: "/api/room-types",
  httpVerb: "put",
  params: {
    id: roomTypeId, // This becomes ?id=123 in the URL
  },
});

// Execute update - only the request body is passed to mutate
mutate(
  { name: "Updated Room Type Name", description: "Updated description" },
  {
    onSuccess: () => {
      toast.success("Room type updated successfully!");
      queryClient.invalidateQueries({ queryKey: ["/api/room-types"] });
    },
  }
);
```

**Important Notes for PUT:**
- The `path` should match exactly what's in your OpenAPI spec (e.g., `/api/room-types`)
- Query parameters (like `id`) go in the `params` prop, not in the path
- The `params` type is automatically inferred based on the path AND httpVerb combination
- The request body (passed to `mutate`/`mutateAsync`) is separate from query params

### DELETE Request

```typescript
const { mutate, isPending } = useAppMutation({
  mutationKey: ["deleteRoomType", roomTypeId],
  path: "/api/room-types",
  httpVerb: "delete",
  params: {
    id: roomTypeId,
  },
});

// Execute delete - pass empty object or required body as per your API spec
mutate(
  {},
  {
    onSuccess: () => {
      toast.success("Room type deleted successfully!");
      queryClient.invalidateQueries({ queryKey: ["/api/room-types"] });
    },
  }
);
```

---

## Advanced Examples

### File Upload with Progress

```typescript
const [uploadProgress, setUploadProgress] = useState(0);

const { mutate, isPending } = useAppMutation({
  mutationKey: "uploadDocument",
  path: "/api/documents/upload",
  isMultipart: true,
  onUploadProgressCallBack: (progressEvent) => {
    const progress = progressEvent.total
      ? Math.round((progressEvent.loaded * 100) / progressEvent.total)
      : 0;
    setUploadProgress(progress);
  },
});

// Create FormData and execute upload
const handleFileUpload = (file: File) => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("description", "My document");

  mutate(formData as any, {
    onSuccess: (data) => {
      toast.success("File uploaded successfully!");
      setUploadProgress(0);
    },
  });
};

// In your JSX
<>
  {isPending && <ProgressBar value={uploadProgress} />}
  <input type="file" onChange={(e) => handleFileUpload(e.target.files[0])} />
</>
```

### With Query Parameters

```typescript
// Query params are typed based on path + httpVerb combination
// TypeScript will enforce the correct params shape
const { mutate } = useAppMutation({
  mutationKey: "assignTeacher",
  path: "/api/subjects/teachers",
  httpVerb: "post",
  params: {
    semester: "2024-1",
    override: true,
  },
});

mutate({ teacherId: "teacher-123" });
```

### Conditional Params (e.g., Create vs Update in same component)

When you have a form that handles both create and update, you may need to conditionally provide params:

```typescript
// For update operations where params are required
const { mutateAsync: updateRoomTypeAsync } = useAppMutation({
  httpVerb: "put",
  path: "/api/room-types",
  params: roomTypeToUpdate
    ? { id: roomTypeToUpdate.id }
    : undefined,
  mutationKey: `update-room-type-${roomTypeToUpdate?.id}`,
});

// For create operations (no params needed)
const { mutateAsync: createRoomTypeAsync } = useAppMutation({
  httpVerb: "post",
  path: "/api/room-types",
  mutationKey: "create-room-type",
});

// Usage in form submit
if (isEditing) {
  await updateRoomTypeAsync({ name, description });
} else {
  await createRoomTypeAsync({ name, description });
}
```

### Force Token Refresh

```typescript
// Useful when user permissions have changed and you need a fresh token
const { mutate } = useAppMutation({
  mutationKey: "updateRoomType",
  path: "/api/room-types",
  httpVerb: "put",
  params: { id: roomTypeId },
  forceRefreshToken: true,
});
```

### Integration with TanStack Query Client

```typescript
import { useQueryClient } from "@tanstack/react-query";

const MyComponent = () => {
  const queryClient = useQueryClient();

  const { mutate } = useAppMutation({
    mutationKey: "createRoomType",
    path: "/api/room-types",
  });

  const handleCreate = (data: RoomTypeFormData) => {
    mutate(data, {
      onSuccess: (newRoomType) => {
        // Invalidate and refetch room types list
        queryClient.invalidateQueries({ queryKey: ["/api/room-types"] });

        // Or optimistically update the cache
        queryClient.setQueryData(["/api/room-types"], (old: RoomType[]) => [
          ...old,
          newRoomType,
        ]);
      },
    });
  };
};
```

---

## Return Value

Both hooks return all properties from their respective TanStack Query hooks:

### useAppMutation Returns

- `mutate` - Function to trigger the mutation
- `mutateAsync` - Async version that returns a Promise
- `isPending` - Loading state
- `isError` - Error state
- `error` - Error object (typed as `AxiosError`)
- `data` - Response data (after successful mutation)
- `reset` - Function to reset mutation state
- And more...

---

## Type Safety

Both hooks leverage TypeScript and your OpenAPI-generated types to provide:

1. **Path autocomplete** - Only valid API paths are allowed
2. **Request body validation** - Request payloads are typed based on the endpoint
3. **Response typing** - Response data is automatically typed
4. **Query params validation** - Only valid query parameters for each endpoint and HTTP verb

```typescript
// TypeScript will show an error if the path doesn't exist
useAppQuery({ path: "/api/invalid-path" }); // ❌ Error

// TypeScript will validate the request body shape
useAppMutation({
  mutationKey: "createRoomType",
  path: "/api/room-types",
}).mutate({ invalidField: "value" }); // ❌ Error - invalid field

// Response data is fully typed
const { data } = useAppQuery({ path: "/api/room-types" });
data?.forEach((roomType) => {
  console.log(roomType.name); // ✅ Autocomplete works
  console.log(roomType.invalidProp); // ❌ Error - property doesn't exist
});

// Params are typed based on path + httpVerb
useAppMutation({
  mutationKey: "updateRoomType",
  path: "/api/room-types",
  httpVerb: "put",
  params: { id: 123 }, // ✅ TypeScript knows 'id' is required for PUT
});
```

---

## Error Handling

Both hooks integrate with Azure AD authentication and handle token refresh automatically:

```typescript
const { error, isError } = useAppMutation({
  mutationKey: "createRoomType",
  path: "/api/room-types",
});

// Error is typed as AxiosError
if (isError) {
  // Access response data
  const errorData = error.response?.data;
  const statusCode = error.response?.status;

  // Handle specific status codes
  if (statusCode === 400) {
    // Validation error
  } else if (statusCode === 403) {
    // Forbidden
  } else if (statusCode === 404) {
    // Not found
  }
}
```

---

## Migration from v1 Hooks

If you're migrating from `use-app-query.ts` or `use-app-mutation.ts`:

1. Update imports to use v2 versions
2. The v2 hooks have improved type inference from OpenAPI specs
3. Parameter names remain mostly the same
4. Response and request types are now automatically inferred

```typescript
// Before (v1)
import useAppQuery from "@/hooks/use-app-query";
import useAppMutation from "@/hooks/use-app-mutation";

// After (v2)
import useAppQuery from "@/hooks/use-app-query-v2";
import useAppMutation from "@/hooks/use-app-mutation-v2";
```
