import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { BookOpen, Users, DoorOpen, Clock, Plus } from "lucide-react";

export default function HomePage() {
  const [courses, setCourses] = useState(5);
  const [teachers, setTeachers] = useState(12);
  const [rooms, setRooms] = useState(8);
  const [timeslots, setTimeslots] = useState(24);

  const stats = [
    {
      label: "Active Courses",
      value: courses,
      icon: BookOpen,
      color: "bg-blue-500/10 text-blue-600",
    },
    {
      label: "Teachers",
      value: teachers,
      icon: Users,
      color: "bg-teal-500/10 text-teal-600",
    },
    {
      label: "Available Rooms",
      value: rooms,
      icon: DoorOpen,
      color: "bg-purple-500/10 text-purple-600",
    },
    {
      label: "Timeslots",
      value: timeslots,
      icon: Clock,
      color: "bg-orange-500/10 text-orange-600",
    },
  ];

  return (
    <main>
      <div className="p-4 md:p-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Course Scheduling
          </h1>
          <p className="text-muted-foreground">
            Manage your college course schedule and resources
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {stats.map((stat) => {
            const Icon = stat.icon;
            return (
              <Card key={stat.label}>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-muted-foreground">
                        {stat.label}
                      </p>
                      <p className="text-2xl font-bold text-foreground mt-1">
                        {stat.value}
                      </p>
                    </div>
                    <div className={`p-3 rounded-lg ${stat.color}`}>
                      <Icon className="size-6" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Courses Section */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Recent Courses</CardTitle>
                  <CardDescription>
                    Active courses for the current semester
                  </CardDescription>
                </div>
                <Button size="sm" className="gap-2">
                  <Plus className="size-4" />
                  New Course
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {[
                  {
                    name: "Data Structures",
                    code: "CS101",
                    section: "3 sections",
                  },
                  {
                    name: "Web Development",
                    code: "CS203",
                    section: "2 sections",
                  },
                  {
                    name: "Algorithms",
                    code: "CS102",
                    section: "4 sections",
                  },
                  {
                    name: "Database Systems",
                    code: "CS204",
                    section: "2 sections",
                  },
                ].map((course) => (
                  <div
                    key={course.code}
                    className="flex items-center justify-between p-3 rounded-lg border border-border hover:bg-muted transition-colors cursor-pointer"
                  >
                    <div>
                      <p className="font-medium text-foreground">
                        {course.name}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {course.code} • {course.section}
                      </p>
                    </div>
                    <Button variant="ghost" size="sm">
                      Edit
                    </Button>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Quick Links */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Quick Links</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Button
                variant="outline"
                className="w-full justify-start gap-2 bg-transparent"
              >
                <Plus className="size-4" />
                Add Teacher
              </Button>
              <Button
                variant="outline"
                className="w-full justify-start gap-2 bg-transparent"
              >
                <Plus className="size-4" />
                Add Room
              </Button>
              <Button
                variant="outline"
                className="w-full justify-start gap-2 bg-transparent"
              >
                <Plus className="size-4" />
                View Schedule
              </Button>
              <Button
                variant="outline"
                className="w-full justify-start gap-2 bg-transparent"
              >
                <Plus className="size-4" />
                Generate Report
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Information Section */}
        <Card className="mt-6">
          <CardHeader>
            <CardTitle>System Overview</CardTitle>
            <CardDescription>
              Your course scheduling dashboard provides centralized management
              for all academic resources
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h4 className="font-semibold text-foreground mb-2">
                  Key Features
                </h4>
                <ul className="space-y-2 text-sm text-muted-foreground">
                  <li>✓ Manage courses and class sections</li>
                  <li>✓ Assign teachers to classes</li>
                  <li>✓ Allocate rooms and resources</li>
                  <li>✓ Create flexible timeslots</li>
                  <li>✓ Avoid scheduling conflicts</li>
                  <li>✓ Generate detailed reports</li>
                </ul>
              </div>
              <div>
                <h4 className="font-semibold text-foreground mb-2">
                  Current Semester
                </h4>
                <div className="space-y-3 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Start Date</span>
                    <span className="font-medium text-foreground">
                      September 1, 2024
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">End Date</span>
                    <span className="font-medium text-foreground">
                      December 15, 2024
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Weeks</span>
                    <span className="font-medium text-foreground">
                      16 weeks
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Status</span>
                    <span className="font-medium text-green-600">Active</span>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </main>
  );
}
