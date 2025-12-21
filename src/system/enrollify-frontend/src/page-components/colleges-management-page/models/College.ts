import type AuditInfo from "@/api/models/AuditInfo";

export interface College extends AuditInfo {
  id: number;
  name: string;
  description: string;
  code: string;
  dean: string;
}
