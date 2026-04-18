import {
  Settings2,
  DatabaseIcon,
  Calendar1Icon,
  SchoolIcon,
  type LucideIcon,
  Castle,
  GraduationCap,
  BuildingIcon,
  DoorOpen,
  BookOpen,
  Scroll,
  BookUser,
} from "lucide-react";

import { PolicyNames } from "@/infrastructure/authorization/models/PolicyNames";
import type { PolicyName } from "@/infrastructure/authorization/models/PolicyNames";
import type { FileRouteTypes } from "@/routeTree.gen";

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
        icon: SchoolIcon,
      },
      {
        title: "Departments",
        url: "/portal/master-data/departments",
        viewAuthorizationPolicies: [PolicyNames.canViewDepartments],
        icon: Castle,
      },
      {
        title: "Courses",
        url: "/portal/master-data/courses",
        viewAuthorizationPolicies: [PolicyNames.canViewCourses],
        icon: GraduationCap,
      },
      {
        title: "Buildings",
        url: "/portal/master-data/buildings",
        viewAuthorizationPolicies: [PolicyNames.canViewBuildings],
        icon: BuildingIcon,
      },
      {
        title: "Rooms",
        url: "/portal/master-data/rooms",
        viewAuthorizationPolicies: [
          PolicyNames.canViewRooms,
          PolicyNames.canViewRoomTypes,
        ],
        icon: DoorOpen,
      },
      {
        title: "Subjects",
        url: "/portal/master-data/subjects/{-$page}/{-$pageSize}",
        viewAuthorizationPolicies: [PolicyNames.canViewSubjects],
        icon: BookOpen,
      },
    ],
  },
  {
    title: "Curriculum & Scheduling",
    url: "/portal/curriculum-and-scheduling",
    icon: Calendar1Icon,
    isActive: false,
    items: [
      {
        title: "Curriculum",
        url: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
        viewAuthorizationPolicies: [PolicyNames.canViewCurriculums],
        icon: Scroll,
      },
      {
        title: "Teachers",
        url: "/portal/curriculum-and-scheduling/teachers",
        viewAuthorizationPolicies: [PolicyNames.canViewTeachers],
        icon: BookUser,
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
