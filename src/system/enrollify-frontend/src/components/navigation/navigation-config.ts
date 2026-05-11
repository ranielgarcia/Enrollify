import { DatabaseIcon } from "lucide-react";

import { PolicyNames } from "@/infrastructure/authorization/models/PolicyNames";
import { ModuleIcons } from "@/config/module-icons";
import type { NavMainItemProp } from "./nav-main";

export const navigationItems: NavMainItemProp[] = [
  {
    title: "Master Data",
    url: "/portal/master-data",
    icon: DatabaseIcon,
    isActive: false,
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
      {
        title: "Class Sections",
        url: "/portal/curriculum-and-scheduling/sections",
        viewAuthorizationPolicies: [PolicyNames.canViewClassSections],
        icon: ModuleIcons.sections,
      },
    ],
  },
];
