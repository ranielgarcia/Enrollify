import { ClassSectionStatusEnum, type ClassSectionStatus } from "@/api/models/class-section";
import type { ClassSectionV2 } from "@/api/models/class-section-v2";
import type { SectionStats } from "@/api/models/section-stats";

const MOCK_COURSES = [
  { id: 1, code: "BSCS", name: "Bachelor of Science in Computer Science" },
  { id: 2, code: "BSIT", name: "Bachelor of Science in Information Technology" },
  { id: 3, code: "BSEE", name: "Bachelor of Science in Electrical Engineering" },
  { id: 4, code: "BSCE", name: "Bachelor of Science in Civil Engineering" },
];

const MOCK_TEACHERS = [
  { id: 1, firstName: "John", lastName: "Smith", email: "john.smith@example.com" },
  { id: 2, firstName: "Jane", lastName: "Doe", email: "jane.doe@example.com" },
  { id: 3, firstName: "Robert", lastName: "Jones", email: "robert.jones@example.com" },
  { id: 4, firstName: "Maria", lastName: "Garcia", email: "maria.garcia@example.com" },
  { id: 5, firstName: "David", lastName: "Lee", email: "david.lee@example.com" },
  undefined, // some sections without advisers
];

const MOCK_TERMS = [
  { id: 1, termNumber: 1, termName: "1st Semester" },
  { id: 2, termNumber: 2, termName: "2nd Semester" },
];

const STATUSES: ClassSectionStatus[] = [
  { name: "Draft", value: ClassSectionStatusEnum.Draft, description: "Draft" },
  { name: "Open", value: ClassSectionStatusEnum.Open, description: "Open for Enrollment" },
  { name: "Cancelled", value: ClassSectionStatusEnum.Cancelled, description: "Cancelled" },
];

function generateValidationSummary(status: ClassSectionStatus) {
  // Draft sections may have errors/conflicts, Open/Cancelled sections have none
  if (status.value === ClassSectionStatusEnum.Open || status.value === ClassSectionStatusEnum.Cancelled) {
    return {
      totalOfferings: Math.floor(Math.random() * 3) + 1,
      offeringsWithErrors: 0,
      offeringsWithConflicts: 0,
      missingTeacherCount: 0,
      missingRoomCount: 0,
      missingScheduleCount: 0,
    };
  }

  // Draft sections may have validation issues
  const totalOfferings = Math.floor(Math.random() * 3) + 1;
  return {
    totalOfferings,
    offeringsWithErrors: Math.random() > 0.6 ? Math.floor(Math.random() * totalOfferings) : 0,
    offeringsWithConflicts: Math.random() > 0.7 ? Math.floor(Math.random() * totalOfferings) : 0,
    missingTeacherCount: Math.random() > 0.7 ? Math.floor(Math.random() * 2) : 0,
    missingRoomCount: Math.random() > 0.8 ? 1 : 0,
    missingScheduleCount: Math.random() > 0.75 ? Math.floor(Math.random() * 2) : 0,
  };
}

export function generateMockClassSections(collegeId: string, count: number = 30): ClassSectionV2[] {
  const sections: ClassSectionV2[] = [];

  for (let i = 0; i < count; i++) {
    const course = MOCK_COURSES[Math.floor(Math.random() * MOCK_COURSES.length)];
    const status = STATUSES[Math.floor(Math.random() * STATUSES.length)];
    const adviser = MOCK_TEACHERS[Math.floor(Math.random() * MOCK_TEACHERS.length)];
    const validationSummary = generateValidationSummary(status);

    sections.push({
      id: parseInt(collegeId) * 1000 + i,
      name: `${course.code} ${String.fromCharCode(65 + (i % 5))}`,
      sectionCode: String.fromCharCode(65 + (i % 5)),
      intendedYearLevel: (i % 4) + 1,
      fullName: `${course.code} ${String.fromCharCode(65 + (i % 5))} - ${course.name}`,
      course,
      curriculum: { id: 1, version: "2024" },
      academicTerm: MOCK_TERMS[Math.floor(Math.random() * MOCK_TERMS.length)],
      cohortAcademicYear: { id: 1, academicYearTitle: "2025-2026" },
      adviser: adviser ? { ...adviser } : undefined,
      status,
      unresolvedErrorsCount: validationSummary.offeringsWithErrors + validationSummary.missingTeacherCount,
      isEligibleForOpenEnrollment: status.value === ClassSectionStatusEnum.Draft,
      validationSummary,
      createdAt: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000),
      updatedAt: new Date(),
      createdBy: "System",
      updatedBy: "System",
    });
  }

  return sections;
}

export function generateMockSectionStats(): SectionStats {
  const draftCount = Math.floor(Math.random() * 15) + 5;
  const openCount = Math.floor(Math.random() * 50) + 20;
  const cancelledCount = Math.floor(Math.random() * 5) + 1;

  return {
    totalDraft: draftCount,
    totalOpen: openCount,
    totalCancelled: cancelledCount,
    sectionsWithUnresolvedErrors: Math.floor(draftCount * 0.3),
    sectionsWithConflicts: Math.floor(draftCount * 0.2),
    unscheduledCount: Math.floor(draftCount * 0.4),
  };
}

export function generateMockColleges() {
  return [
    { id: "1", name: "College of Engineering", abbreviation: "CED" },
    { id: "2", name: "College of Science", abbreviation: "COS" },
    { id: "3", name: "College of Liberal Arts", abbreviation: "CLA" },
    { id: "4", name: "College of Business", abbreviation: "COB" },
    { id: "5", name: "College of Education", abbreviation: "COE" },
  ];
}
