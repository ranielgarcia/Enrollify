import { Settings2, DatabaseIcon, type LucideIcon } from "lucide-react";

import { PolicyNames } from "@/infrastructure/authorization/models/PolicyNames";
import type { PolicyName } from "@/infrastructure/authorization/models/PolicyNames";
import type { FileRouteTypes } from "@/routeTree.gen";
import { ModuleIcons } from "@/config/module-icons";

export interface NavSubItemConfig {
  title: string;
  url: FileRouteTypes["to"] & {};
  params?: Record<string, string>;
  viewAuthorizationPolicies: PolicyName[];
  icon?: LucideIcon;
}

export interface NavMainItemConfig {
  title: string;
  url: FileRouteTypes["to"] & {};
  icon: LucideIcon;
  isActive?: boolean;
  items: NavSubItemConfig[];
}

export const navigationItems: NavMainItemConfig[] = [
  {
    title: "Master Data",
    url: "/portal/master-data",
    icon: DatabaseIcon,
    isActive: true,
    items: [
      {
        title: "Colleges",
        url: "/portal/master-data/colleges",
        viewAuthorizationPolicies: [PolicyNames.canViewColleges],
        icon: ModuleIcons.colleges,
      },
      {
        title: "Departments",
        url: "/portal/master-data/departments",
        viewAuthorizationPolicies: [PolicyNames.canViewDepartments],
        icon: ModuleIcons.departments,
      },
      {
        title: "Courses",
        url: "/portal/master-data/courses",
        viewAuthorizationPolicies: [PolicyNames.canViewCourses],
        icon: ModuleIcons.courses,
      },
      {
        title: "Buildings",
        url: "/portal/master-data/buildings",
        viewAuthorizationPolicies: [PolicyNames.canViewBuildings],
        icon: ModuleIcons.buildings,
      },
      {
        title: "Rooms",
        url: "/portal/master-data/rooms",
        viewAuthorizationPolicies: [
          PolicyNames.canViewRooms,
          PolicyNames.canViewRoomTypes,
        ],
        icon: ModuleIcons.rooms,
      },
      {
        title: "Subjects",
        url: "/portal/master-data/subjects",
        viewAuthorizationPolicies: [PolicyNames.canViewSubjects],
        icon: ModuleIcons.subjects,
      },
    ],
  },
  {
    title: "Curriculum & Scheduling",
    url: "/portal/curriculum-and-scheduling",
    icon: ModuleIcons.curriculum,
    isActive: false,
    items: [
      {
        title: "Curriculum",
        url: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
        viewAuthorizationPolicies: [PolicyNames.canViewCurriculums],
        icon: ModuleIcons.curriculum,
      },
      {
        title: "Teachers",
        url: "/portal/curriculum-and-scheduling/teachers",
        viewAuthorizationPolicies: [PolicyNames.canViewTeachers],
        icon: ModuleIcons.teachers,
      },
      {
        title: "Academic Year",
        url: "/portal/curriculum-and-scheduling/academic-year",
        viewAuthorizationPolicies: [PolicyNames.canViewAcademicYearsAndTerms],
        icon: ModuleIcons.academicYear,
      },
    ],
  },
  {
    title: "Settings",
    url: "/",
    icon: Settings2,
    items: [
      {
        title: "General",
        url: "/",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Team",
        url: "#",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Billing",
        url: "#",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Limits",
        url: "#",
        viewAuthorizationPolicies: [],
      },
    ],
  },
];
