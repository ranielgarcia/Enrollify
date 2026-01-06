export const AcademicSystemEnum = {
  Semester: 2,
  Trimester: 3,
};

export type AcademicSystemName = keyof typeof AcademicSystemEnum;
export type AcademicSystemValue =
  (typeof AcademicSystemEnum)[AcademicSystemName];

// Type-safe Scope that ensures id and name correspond correctly
export type AcademicSystem = {
  [K in AcademicSystemName]: { value: (typeof AcademicSystemEnum)[K]; name: K };
}[AcademicSystemName];

export const createAcademicSystem = <T extends AcademicSystemName>(
  name: T
): Extract<AcademicSystem, { name: T }> =>
  ({
    value: AcademicSystemEnum[name],
    name,
  }) as Extract<AcademicSystem, { name: T }>;
