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
} as const;

export type PolicyName = keyof typeof PolicyNames;
