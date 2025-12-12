export default interface BasicUserInfo {
  id?: number | undefined;
  email?: string | undefined;
  firstName?: string | undefined;
  lastName?: string | undefined;
}

export default interface AuditInfo {
  createdBy?: BasicUserInfo | null;
  createdAt: Date;
  updatedBy?: BasicUserInfo | null;
  updatedAt?: Date;
  deletedBy?: BasicUserInfo | null;
  deletedAt?: Date;
  isActive: boolean;
}
