export const PolicyNames = {
  canViewRoomTypes: "canViewRoomTypes",
  canCreateRoomTypes: "canCreateRoomTypes",
  canUpdateRoomTypes: "canUpdateRoomTypes",
  canDeleteRoomTypes: "canDeleteRoomTypes",
  canViewRooms: "canViewRooms",
  canCreateRooms: "canCreateRooms",
  canUpdateRooms: "canUpdateRooms",
  canDeleteRooms: "canDeleteRooms",
  canViewColleges: "canViewColleges",
  canCreateCollege: "canCreateCollege",
  canUpdateCollege: "canUpdateCollege",
  canDeleteCollege: "canDeleteCollege",
  canViewBuildings: "canViewBuildings",
  canCreateBuilding: "canCreateBuilding",
  canUpdateBuilding: "canUpdateBuilding",
  canDeleteBuilding: "canDeleteBuilding",
} as const;

export type PolicyName = keyof typeof PolicyNames;
