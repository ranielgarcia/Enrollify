import { AxiosError } from "axios";

/**
 * ProblemDetails structure returned by ASP.NET Core APIs
 */
export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  errors?: Record<string, string[]>;
  [key: string]: unknown;
}

/**
 * Parsed API error with user-friendly message
 */
export interface ParsedApiError {
  title: string;
  detail: string | null;
  status: number | null;
  validationErrors: Record<string, string[]> | null;
  isNetworkError: boolean;
}

/**
 * Parses an AxiosError and extracts user-friendly error information
 */
export function parseApiError(
  error: AxiosError<ProblemDetails>,
): ParsedApiError {
  // Network error (no response from server)
  if (!error.response) {
    return {
      title: "Network Error",
      detail:
        error.message ||
        "Unable to connect to the server. Please check your internet connection.",
      status: null,
      validationErrors: null,
      isNetworkError: true,
    };
  }

  const { data, status } = error.response;

  // If we have array of errors response (non-ProblemDetails)
  if (Array.isArray(data)) {
    return {
      title: getDefaultErrorTitle(status),
      detail: (data as string[]).join("\n") || null,
      status: status,
      validationErrors: null,
      isNetworkError: false,
    };
  }

  // If we have ProblemDetails response
  if (data && typeof data === "object") {
    return {
      title: data.title || getDefaultErrorTitle(status),
      detail: data.detail || null,
      status: data.status ?? status,
      validationErrors: data.errors || null,
      isNetworkError: false,
    };
  }

  // Fallback for non-ProblemDetails responses
  return {
    title: getDefaultErrorTitle(status),
    detail: null,
    status,
    validationErrors: null,
    isNetworkError: false,
  };
}

/**
 * Returns a default error title based on HTTP status code
 */
function getDefaultErrorTitle(status: number): string {
  switch (status) {
    case 400:
      return "Bad Request";
    case 401:
      return "Unauthorized";
    case 403:
      return "Forbidden";
    case 404:
      return "Not Found";
    case 409:
      return "Conflict";
    case 422:
      return "Validation Error";
    case 500:
      return "Server Error";
    default:
      return "An error occurred";
  }
}

/**
 * Formats validation errors into a single string for display
 */
export function formatValidationErrors(
  errors: Record<string, string[]>,
  includeFields: boolean = false,
): string {
  if (includeFields)
    return Object.entries(errors)
      .flatMap(([field, messages]) => messages.map((msg) => `${field}: ${msg}`))
      .join("\n");

  return Object.values(errors).flat().join("\n");
}

/**
 * Gets a user-friendly error message from a parsed API error
 */
export function getErrorMessage(error: ParsedApiError): string {
  if (error.validationErrors) {
    return formatValidationErrors(error.validationErrors);
  }
  return error.detail || error.title;
}
