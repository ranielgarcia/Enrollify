import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerRoomPolicies = (registry: PolicyRegistry) => {
  // Room Types policies
  registry.register(
    new PolicyBuilder("canViewRoomTypes")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "RoomTypes")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canCreateRoomTypes")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "RoomTypes")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canUpdateRoomTypes")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "RoomTypes")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canDeleteRoomTypes")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Delete"), "RoomTypes")
      .requireAll()
      .build()
  );

  // Rooms
  registry.register(
    new PolicyBuilder("canViewRooms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Rooms")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canCreateRooms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Rooms")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canUpdateRooms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Rooms")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canDeleteRooms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Delete"), "Rooms")
      .requireAll()
      .build()
  );
};
