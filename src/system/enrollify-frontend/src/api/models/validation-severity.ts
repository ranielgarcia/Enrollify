// Reminder: Be mindful when changing the values of existing enums, as they are stored in the database and
// the value is considered an identifier (Id value).
// This enum must be kept in sync with `ValidationSeverityEnum` in the backend API
export const ValidationSeverities = {
  Info: 1,
  Warning: 2,
  Error: 3,
} as const;

export type ValidationSeverityName = keyof typeof ValidationSeverities;
export type ValidationSeverityId =
  (typeof ValidationSeverities)[ValidationSeverityName];

// Type-safe ValidationSeverity that ensures id and name correspond correctly
export type ValidationSeverity = {
  [K in ValidationSeverityName]: {
    id: (typeof ValidationSeverities)[K];
    name: K;
  };
}[ValidationSeverityName];

// Helper function to create a ValidationSeverity from just the name
export const createValidationSeverity = <T extends ValidationSeverityName>(
  name: T,
): Extract<ValidationSeverity, { name: T }> =>
  ({
    id: ValidationSeverities[name],
    name,
  }) as Extract<ValidationSeverity, { name: T }>;
