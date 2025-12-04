export interface IRolePermission {
  id: number;
  name: string;
  description: string;
  action: string;
  resource: string;
}

export interface IRole {
  id: number;
  name: string;
  description: string;
  permissions: IRolePermission[];
}

export interface UserContext {
  id: number;
  email: string;
  fullName: string;
  roles: IRole[];
}
