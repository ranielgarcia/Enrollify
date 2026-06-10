import { useQuery } from "@tanstack/react-query";
import { getAcademicYearTimeLineWindowOptions } from "@/api/collections/academic-year-collection";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { useAuthenticationContext } from "@/infrastructure/authentication/authentication-context";
import { ModuleIcons } from "@/config/module-icons";
import { Link } from "@tanstack/react-router";
import {
  CalendarDays,
  ArrowRight,
  LayoutDashboard,
  ChevronRight,
} from "lucide-react";
import { formatDate } from "@/lib/date-utils";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";

interface QuickLinkProps {
  href: string;
  label: string;
  description: string;
  icon: React.ElementType;
}

function QuickLink({ href, label, description, icon: Icon }: QuickLinkProps) {
  return (
    <Link
      to={href}
      className="group flex items-center gap-3 rounded-lg border bg-card px-4 py-3 transition-colors hover:bg-accent/40"
    >
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary/10">
        <Icon className="h-4 w-4 text-primary" />
      </div>
      <div className="min-w-0 flex-1 space-y-0.5">
        <p className="text-sm font-medium leading-none">{label}</p>
        <p className="text-xs text-muted-foreground truncate">{description}</p>
      </div>
      <ChevronRight className="size-4 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
    </Link>
  );
}

export default function HomePage() {
  const { user } = useAuthenticationContext();
  const { data: timeline } = useQuery(
    getAcademicYearTimeLineWindowOptions({ IncludeFutureYears: true }),
  );
  const { data: courses } = useQuery(getAllCoursesOptions());

  const currentAY = timeline?.current;
  const displayName = user?.fullName ?? user?.email ?? null;

  const quickLinks: QuickLinkProps[] = [
    {
      href: "/portal/master-data/colleges",
      label: "Colleges",
      description: "Manage college records",
      icon: ModuleIcons.colleges,
    },
    {
      href: "/portal/master-data/departments",
      label: "Departments",
      description: "Manage academic departments",
      icon: ModuleIcons.departments,
    },
    {
      href: "/portal/master-data/courses",
      label: "Courses",
      description: "Manage degree programs",
      icon: ModuleIcons.courses,
    },
    {
      href: "/portal/master-data/buildings",
      label: "Buildings",
      description: "Manage campus buildings",
      icon: ModuleIcons.buildings,
    },
    {
      href: "/portal/master-data/rooms",
      label: "Rooms",
      description: "Manage classrooms and labs",
      icon: ModuleIcons.rooms,
    },
    {
      href: "/portal/master-data/subjects",
      label: "Subjects",
      description: "Manage subject catalog",
      icon: ModuleIcons.subjects,
    },
    {
      href: "/portal/curriculum-and-scheduling/curriculum/{-$curriculumId}",
      label: "Curriculum Builder",
      description: "Design and manage curricula",
      icon: ModuleIcons.curriculum,
    },
    {
      href: "/portal/curriculum-and-scheduling/teachers",
      label: "Teachers",
      description: "Manage faculty and workloads",
      icon: ModuleIcons.teachers,
    },
    {
      href: "/portal/curriculum-and-scheduling/academic-year",
      label: "Academic Year",
      description: "Manage academic calendar",
      icon: ModuleIcons.academicYear,
    },
    {
      href: "/portal/curriculum-and-scheduling/sections",
      label: "Class Sections",
      description: "Manage section assignments",
      icon: ModuleIcons.sections,
    },
  ];

  return (
    <main className="flex min-h-0 min-w-0 flex-1 flex-col px-4 lg:px-6 py-6 space-y-6">
      {/* Welcome hero */}
      <div className="relative overflow-hidden rounded-xl border bg-linear-to-br from-primary/5 via-background to-background p-6 shadow-sm">
        <div className="pointer-events-none absolute -right-8 -top-8 h-40 w-40 rounded-full bg-primary/8 blur-2xl" />
        <div className="relative flex items-start justify-between gap-4">
          <div className="flex items-center gap-4">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 ring-1 ring-primary/20">
              <LayoutDashboard className="h-5 w-5 text-primary" />
            </div>
            <div className="space-y-1">
              <h2 className="text-xl font-bold tracking-tight">
                {displayName ? `Welcome back, ${displayName}` : "Welcome to Enrollify"}
              </h2>
              <p className="text-sm text-muted-foreground">
                Manage your academic programs, curricula, and class scheduling
                from one place.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Current Academic Year */}
      {currentAY && (
        <div className="space-y-3">
          <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground px-0.5">
            Current Academic Year
          </p>
          <div className="relative overflow-hidden rounded-lg border bg-card px-4 py-3">
            <div className="flex items-center gap-3">
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                <CalendarDays className="h-4 w-4 text-primary" />
              </div>
              <div className="flex-1 min-w-0 space-y-0.5">
                <div className="flex items-center gap-2 flex-wrap">
                  <p className="text-sm font-semibold leading-none">
                    {currentAY.academicYearTitle ??
                      `AY ${currentAY.startYear}–${currentAY.endYear}`}
                  </p>
                  <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-emerald-500/15 text-emerald-600 border border-emerald-500/25 hover:bg-emerald-500/15">
                    ● Active
                  </Badge>
                </div>
                <div
                  className={cn(
                    "flex items-center gap-2 text-muted-foreground text-xs",
                  )}
                >
                  <span className="font-medium text-foreground">
                    {formatDate(currentAY.startDate)}
                  </span>
                  <ArrowRight className="size-3" />
                  <span className="font-medium text-foreground">
                    {formatDate(currentAY.endDate)}
                  </span>
                </div>
              </div>
              {courses && courses.length > 0 && (
                <div className="text-right shrink-0">
                  <p className="text-lg font-bold">{courses.length}</p>
                  <p className="text-xs text-muted-foreground">
                    {courses.length === 1 ? "course" : "courses"}
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Quick Navigation */}
      <div className="space-y-3">
        <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground px-0.5">
          Quick Navigation
        </p>
        <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
          {quickLinks.map((link) => (
            <QuickLink key={link.href} {...link} />
          ))}
        </div>
      </div>
    </main>
  );
}
