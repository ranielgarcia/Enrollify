import type React from "react";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { X } from "lucide-react";
import {
  SearchableSelect,
  type SearchableSelectOption,
} from "@/components/searchable-select";
import { Label } from "@/components/ui/label";

interface CurriculumFormProps {
  onClose: () => void;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onSave: (curriculum: any) => void;
}

export function CurriculumForm({ onClose, onSave }: CurriculumFormProps) {
  const [formData, setFormData] = useState({
    courseId: "",
    effectiveYear: new Date().getFullYear().toString(),
    version: "",
    status: "DRAFT",
    description: "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({
      id: Date.now().toString(),
      ...formData,
    });
  };

  const coursesOptions: SearchableSelectOption[] = [
    { value: "1", label: "Computer Science" },
    { value: "2", label: "Information Technology" },
  ];

  return (
    <Card className="border-2 border-primary">
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle>Create New Curriculum</CardTitle>
        <Button variant="ghost" size="sm" onClick={onClose}>
          <X className="size-4" />
        </Button>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="course">Course:</Label>
              <SearchableSelect
                options={coursesOptions}
                value={formData.courseId}
                onValueChange={(v) => setFormData({ ...formData, courseId: v })}
                name="course"
                placeholder="Select Course"
                searchPlaceholder="Search Course..."
                emptyMessage="No Course found"
              />
            </div>
            <div>
              <label className="text-sm font-medium block mb-1">
                Effective Year
              </label>
              <Input
                type="number"
                value={formData.effectiveYear}
                onChange={(e) =>
                  setFormData({ ...formData, effectiveYear: e.target.value })
                }
                required
              />
            </div>
            <div>
              <label className="text-sm font-medium block mb-1">
                Version Identifier
              </label>
              <Input
                placeholder="e.g., 2024-A"
                value={formData.version}
                onChange={(e) =>
                  setFormData({ ...formData, version: e.target.value })
                }
                required
              />
            </div>
            <div className="md:col-span-2">
              <label className="text-sm font-medium block mb-1">
                Description
              </label>
              <Textarea
                placeholder="Curriculum details..."
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                rows={3}
              />
            </div>
          </div>
          <div className="flex gap-2 justify-end pt-4">
            <Button variant="outline" type="button" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Create Curriculum</Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
