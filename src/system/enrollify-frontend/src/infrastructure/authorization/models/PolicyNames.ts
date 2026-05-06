export const PolicyNames = {
  canViewRoomTypes: "canViewRoomTypes",
  canCreateRoomTypes: "canCreateRoomTypes",
  canUpdateRoomTypes: "canUpdateRoomTypes",
  canDeleteRoomTypes: "canDeleteRoomTypes",

  canViewRooms: "canViewRooms",
  canCreateRooms: "canCreateRooms",
  canUpdateRooms: "canUpdateRooms",
  canDeleteRooms: "canDeleteRooms",

  canViewColleges: "canViewColleges",
  canCreateCollege: "canCreateCollege",
  canUpdateCollege: "canUpdateCollege",
  canDeleteCollege: "canDeleteCollege",

  canViewBuildings: "canViewBuildings",
  canCreateBuilding: "canCreateBuilding",
  canUpdateBuilding: "canUpdateBuilding",
  canDeleteBuilding: "canDeleteBuilding",

  canViewDepartments: "canViewDepartments",
  canCreateDepartment: "canCreateDepartment",
  canUpdateDepartment: "canUpdateDepartment",
  canDeleteDepartment: "canDeleteDepartment",

  canViewCourses: "canViewCourses",
  canCreateCourse: "canCreateCourse",
  canUpdateCourse: "canUpdateCourse",
  canDeleteCourse: "canDeleteCourse",

  canViewSubjects: "canViewSubjects",
  canCreateSubject: "canCreateSubject",
  canUpdateSubject: "canUpdateSubject",
  canDeleteSubject: "canDeleteSubject",

  canViewCurriculums: "canViewCurriculums",
  canCreateCurriculum: "canCreateCurriculum",
  canUpdateCurriculum: "canUpdateCurriculum",
  canArchiveCurriculum: "canArchiveCurriculum",

  canViewSubjectEquivalenceGroups: "canViewSubjectEquivalenceGroups",
  canCreateSubjectEquivalenceGroup: "canCreateSubjectEquivalenceGroup",
  canUpdateSubjectEquivalenceGroup: "canUpdateSubjectEquivalenceGroup",
  canDeleteSubjectEquivalenceGroup: "canDeleteSubjectEquivalenceGroup",

  canViewTeachers: "canViewTeachers",
  canCreateTeacher: "canCreateTeacher",
  canUpdateTeacher: "canUpdateTeacher",
  canDeleteTeacher: "canDeleteTeacher",

  canViewAcademicYearsAndTerms: "canViewAcademicYearsAndTerms",
  canCreateAcademicYearAndTerms: "canCreateAcademicYearAndTerms",
  canUpdateAcademicYearAndTerms: "canUpdateAcademicYearAndTerms",
  canDeleteAcademicYearAndTerms: "canDeleteAcademicYearAndTerms",

  canViewClassSections: "canViewClassSections",
  canCreateClassSection: "canCreateClassSection",
  canUpdateClassSection: "canUpdateClassSection",
  canDeleteClassSection: "canDeleteClassSection",

  canViewOfferings: "canViewOfferings",
  canCreateOffering: "canCreateOffering",
  canUpdateOffering: "canUpdateOffering",
  canDeleteOffering: "canDeleteOffering",

  canViewSchedules: "canViewSchedules",
  canCreateSchedule: "canCreateSchedule",
  canDeleteSchedule: "canDeleteSchedule",
} as const;

export type PolicyName = keyof typeof PolicyNames;
