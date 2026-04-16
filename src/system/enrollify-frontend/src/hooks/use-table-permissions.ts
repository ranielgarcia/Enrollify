import { useEffect, useState } from "react";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";

interface TablePermissions {
  canUpdate: boolean;
  canDelete: boolean;
}

export function useTablePermissions(
  updatePolicy: string,
  deletePolicy: string
): TablePermissions {
  const { checkPolicy } = useAuthorization();
  const [permissions, setPermissions] = useState<TablePermissions>({
    canUpdate: false,
    canDelete: false,
  });

  useEffect(() => {
    let mounted = true;

    const checkPolicies = async () => {
      const [canUpdate, canDelete] = await Promise.all([
        checkPolicy(updatePolicy),
        checkPolicy(deletePolicy),
      ]);
      if (mounted) {
        setPermissions({ canUpdate, canDelete });
      }
    };

    checkPolicies();
    return () => { mounted = false; };
  }, [checkPolicy, updatePolicy, deletePolicy]);

  return permissions;
}
