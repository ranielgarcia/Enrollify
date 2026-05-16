import type {
  ExtendedColumnFilter,
  ExtendedColumnSort,
} from "@/types/data-table";
import {
  ClassSectionSchema,
  ClassSectionWithOfferingsSchema,
  type ClassSection,
  type ClassSectionWithOfferings,
} from "../models/class-section";
import { pagedResultSchema, type PagedResult } from "../models/paged-result";
import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import { toast } from "sonner";

const queryKeys = {
  base: () => ["sections"],
  paginated: (page: number, pageSize: number) => [
    ...queryKeys.base(),
    page,
    pageSize,
  ],
  filter: (
    page: number,
    pageSize: number,
    filters: ExtendedColumnFilter<ClassSection>[],
    sort: ExtendedColumnSort<ClassSection>[],
    joinOperator: string,
  ) => [
    ...queryKeys.base(),
    "search",
    page,
    pageSize,
    filters,
    sort,
    joinOperator,
  ],
  detail: (id: number) => [...queryKeys.base(), "detail", id],
  create: () => [...queryKeys.base(), "create"],
  update: (id: number) => [...queryKeys.base(), "update", id],
  delete: (id: number) => [...queryKeys.base(), "delete", id],
};

const pagedSectionsSchema = pagedResultSchema(ClassSectionSchema);

// Mock data for development — remove initialData when the backend is ready
const MOCK_SECTIONS: ClassSection[] = [
  {
    id: 1,
    name: "BSCS-1A",
    yearLevel: 1,
    studentCapacity: 40,
    course: { id: 1, code: "BSCS", name: "Bachelor of Science in Computer Science" },
    academicTerm: { id: 1, termNumber: 1, name: "1st Semester 2024-2025" },
    adviser: { id: 1, firstName: "Maria", lastName: "Santos" },
    isActive: true,
    createdAt: "2024-08-01",
    updatedAt: null,
    createdBy: null,
    updatedBy: null,
    deletedBy: null,
    deletedAt: null,
  },
  {
    id: 2,
    name: "BSIT-2B",
    yearLevel: 2,
    studentCapacity: 35,
    course: { id: 2, code: "BSIT", name: "Bachelor of Science in Information Technology" },
    academicTerm: { id: 1, termNumber: 1, name: "1st Semester 2024-2025" },
    adviser: { id: 2, firstName: "Juan", lastName: "Dela Cruz" },
    isActive: true,
    createdAt: "2024-08-01",
    updatedAt: null,
    createdBy: null,
    updatedBy: null,
    deletedBy: null,
    deletedAt: null,
  },
];

const MOCK_PAGED_SECTIONS: PagedResult<ClassSection> = {
  items: MOCK_SECTIONS,
  page: 1,
  pageSize: 10,
  totalCount: MOCK_SECTIONS.length,
  totalPages: 1,
};

export const filterSectionsPaginatedOptions = (
  page: number,
  pageSize: number,
  filters: ExtendedColumnFilter<ClassSection>[],
  sort: ExtendedColumnSort<ClassSection>[],
  joinOperator: string,
) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createQueryOptions({
    path: "/api/sections/filter/{page}/{pageSize}" as any,
    pathParams: { page, pageSize },
    params: {
      Filters: filters.length ? JSON.stringify(filters) : undefined,
      Sort: sort.length ? JSON.stringify(sort) : undefined,
      JoinOperator: joinOperator,
    } as any,
    options: {
      queryKey: queryKeys.filter(page, pageSize, filters, sort, joinOperator),
      staleTime: 1000 * 60 * 2,
      // Remove initialData when the backend is ready
      initialData: MOCK_PAGED_SECTIONS as any,
      select: (pagedResults: any): PagedResult<ClassSection> => {
        if (
          !pagedResults ||
          (typeof pagedResults === "string" && pagedResults === "")
        ) {
          return { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
        }
        const data =
          typeof pagedResults === "string"
            ? JSON.parse(pagedResults)
            : pagedResults;
        return pagedSectionsSchema.parse(data);
      },
    },
  });

// Mock section detail data — remove when backend is ready
const MOCK_SECTION_WITH_OFFERINGS: ClassSectionWithOfferings = {
  ...MOCK_SECTIONS[0]!,
  offerings: [
    {
      id: 1,
      subject: { id: 1, code: "CS101", title: "Programming 1", units: 3 },
      teacher: { id: 1, firstName: "Maria", lastName: "Santos" },
      room: { id: 1, roomNumber: "Rm 101", building: { id: 1, name: "Main Building" } },
      dayPattern: "MW",
      daysPerWeek: 2,
      hoursPerDay: 1.5,
      maxNumberOfStudents: null,
      isActive: true,
      createdAt: "2024-08-01",
      updatedAt: null,
      createdBy: null,
      updatedBy: null,
      deletedBy: null,
      deletedAt: null,
      schedules: [
        {
          id: 1,
          classSectionSubjectOfferingId: 1,
          dayOfWeek: "MON",
          startTime: "09:00",
          endTime: "10:30",
          isActive: true,
        },
        {
          id: 2,
          classSectionSubjectOfferingId: 1,
          dayOfWeek: "WED",
          startTime: "09:00",
          endTime: "10:30",
          isActive: true,
        },
      ],
      conflicts: [],
    },
    {
      id: 2,
      subject: { id: 2, code: "MATH101", title: "Calculus 1", units: 3 },
      teacher: { id: 3, firstName: "Pedro", lastName: "Reyes" },
      room: { id: 2, roomNumber: "Rm 202", building: { id: 2, name: "Science Bldg" } },
      dayPattern: "TTh",
      daysPerWeek: 2,
      hoursPerDay: 1.5,
      maxNumberOfStudents: null,
      isActive: true,
      createdAt: "2024-08-01",
      updatedAt: null,
      createdBy: null,
      updatedBy: null,
      deletedBy: null,
      deletedAt: null,
      schedules: [
        {
          id: 3,
          classSectionSubjectOfferingId: 2,
          dayOfWeek: "TUE",
          startTime: "11:00",
          endTime: "12:30",
          isActive: true,
        },
        {
          id: 4,
          classSectionSubjectOfferingId: 2,
          dayOfWeek: "THU",
          startTime: "11:00",
          endTime: "12:30",
          isActive: true,
        },
      ],
      conflicts: [
        {
          id: "c1",
          type: "TEACHER_DOUBLE_BOOKED",
          severity: "error",
          message: "Prof. Reyes is already teaching PE101 on THU 11:00–12:30 in BSCS-2A",
          day: "THU",
          startTime: "11:00",
          endTime: "12:30",
          affectedOfferings: [
            { id: 5, subject: { code: "PE101", title: "Physical Education 1" }, section: { name: "BSCS-2A" } },
          ],
        },
      ],
    },
  ],
};

export const getSectionWithOfferingsOptions = (id: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createQueryOptions({
    path: "/api/sections/{id}" as any,
    pathParams: { id } as any,
    options: {
      queryKey: queryKeys.detail(id),
      staleTime: 1000 * 60 * 2,
      // Remove initialData when the backend is ready
      initialData: MOCK_SECTION_WITH_OFFERINGS as any,
      select: (data: any): ClassSectionWithOfferings => {
        if (!data || (typeof data === "string" && data === "")) {
          return MOCK_SECTION_WITH_OFFERINGS;
        }
        const parsed = typeof data === "string" ? JSON.parse(data) : data;
        return ClassSectionWithOfferingsSchema.parse(parsed);
      },
    },
  });

export const createSectionOptions = () =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "post",
    path: "/api/sections" as any,
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class section created successfully"),
    },
  });

export const updateSectionOptions = (id: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "put",
    path: "/api/sections/{id}" as any,
    pathParams: { id } as any,
    mutationKey: queryKeys.update(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class section updated successfully"),
    },
  });

export const deleteSectionOptions = (id: number) =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "delete",
    path: "/api/sections/{id}" as any,
    pathParams: { id } as any,
    mutationKey: queryKeys.delete(id),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class section deleted successfully"),
    },
  });

// Bulk initialize types
export interface BulkInitializePayload {
  courseId: number;
  curriculumId: number;
  numberOfSections: number;
}

export interface BulkInitializeClassSectionsRequest {
  academicTermId: number;
  yearLevel: number;
  requestPayload: BulkInitializePayload[];
}

export const bulkInitializeSectionsOptions = () =>
  // @ts-ignore - path will be registered in api.ts when backend is implemented
  createMutationOptions({
    httpVerb: "post",
    path: "/api/class-sections/bulk-initialize" as any,
    mutationKey: [...queryKeys.base(), "bulk-initialize"],
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Class sections bulk initialized successfully"),
    },
  });
