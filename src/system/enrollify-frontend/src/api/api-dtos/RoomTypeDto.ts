import type { components } from "@/api/generated/api";

export type RoomTypeDto =
  | components["schemas"]["EnrollifyApplicationRoomTypesDTOsRoomTypeDTO"]
  | null;
