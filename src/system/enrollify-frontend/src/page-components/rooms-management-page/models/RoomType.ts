import type AuditInfo from "@/api/models/AuditInfo";

export interface RoomType extends AuditInfo {
  id: number;
  name: string;
  description: string;
}
