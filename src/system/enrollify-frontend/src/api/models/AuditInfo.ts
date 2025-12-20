import type BasicUserInfo from "./BasicUserInfo";

export default interface AuditInfo {
  createdBy?: BasicUserInfo | null;
  createdAt: Date;
  updatedBy?: BasicUserInfo | null;
  updatedAt?: Date;
  deletedBy?: BasicUserInfo | null;
  deletedAt?: Date;
  isActive: boolean;
}
