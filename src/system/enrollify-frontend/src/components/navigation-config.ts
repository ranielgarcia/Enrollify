import {
  BookOpen,
  Bot,
  Settings2,
  DatabaseIcon,
  Calendar1Icon,
  type LucideIcon,
} from "lucide-react";

import { PolicyNames } from "@/infrastructure/authorization/models/PolicyNames";
import type { PolicyName } from "@/infrastructure/authorization/models/PolicyNames";
import type { FileRouteTypes } from "@/routeTree.gen";

export interface NavSubItemConfig {
  title: string;
  url: FileRouteTypes["to"] & {};
  params?: Record<string, string>;
  viewAuthorizationPolicies: PolicyName[];
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
    title: "Master Data Management",
    url: "/portal/master-data",
    icon: DatabaseIcon,
    isActive: true,
    items: [
      {
        title: "Colleges",
        url: "/portal/master-data/colleges",
        viewAuthorizationPolicies: [PolicyNames.canViewColleges],
      },
      {
        title: "Departments",
        url: "/portal/master-data/departments",
        viewAuthorizationPolicies: [PolicyNames.canViewDepartments],
      },
      {
        title: "Courses",
        url: "/portal/master-data/courses",
        viewAuthorizationPolicies: [PolicyNames.canViewCourses],
      },
      {
        title: "Buildings",
        url: "/portal/master-data/buildings",
        viewAuthorizationPolicies: [PolicyNames.canViewBuildings],
      },
      {
        title: "Rooms",
        url: "/portal/master-data/rooms",
        viewAuthorizationPolicies: [
          PolicyNames.canViewRooms,
          PolicyNames.canViewRoomTypes,
        ],
      },
      {
        title: "Subjects",
        url: "/portal/master-data/subjects",
        viewAuthorizationPolicies: [PolicyNames.canViewSubjects],
      },
    ],
  },
  {
    title: "Curriculum and Scheduling",
    url: "/portal/curriculum-and-scheduling",
    icon: Calendar1Icon,
    isActive: false,
    items: [
      {
        title: "Curriculum",
        url: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
        viewAuthorizationPolicies: [PolicyNames.canViewCurriculums],
      },
      {
        title: "Teachers",
        url: "/portal/curriculum-and-scheduling/teachers",
        viewAuthorizationPolicies: [PolicyNames.canViewTeachers],
      },
    ],
  },
  {
    title: "Models",
    url: "/",
    icon: Bot,
    items: [
      {
        title: "Genesis",
        url: "/login",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Explorer",
        url: "#",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Quantum",
        url: "#",
        viewAuthorizationPolicies: [],
      },
    ],
  },
  {
    title: "Documentation",
    url: "/",
    icon: BookOpen,
    items: [
      {
        title: "Introduction",
        url: "/",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Get Started",
        url: "#",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Tutorials",
        url: "#",
        viewAuthorizationPolicies: [],
      },
      {
        title: "Changelog",
        url: "#",
        viewAuthorizationPolicies: [],
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
