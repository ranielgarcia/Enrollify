import { useState, useCallback } from "react";

interface CrudState<T> {
  isFormOpen: boolean;
  entityToEdit: T | undefined;
  entityToDelete: T | undefined;
  handleEdit: (entity: T) => void;
  handleDelete: (entity: T) => void;
  handleFormOpenChange: (open: boolean) => void;
  handleDeleteDialogOpenChange: (open: boolean) => void;
  openCreateForm: () => void;
}

export function useCrudState<T>(): CrudState<T> {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [entityToEdit, setEntityToEdit] = useState<T | undefined>();
  const [entityToDelete, setEntityToDelete] = useState<T | undefined>();

  const handleEdit = useCallback((entity: T) => {
    setEntityToEdit(entity);
    setIsFormOpen(true);
  }, []);

  const handleDelete = useCallback((entity: T) => {
    setEntityToDelete(entity);
  }, []);

  const handleFormOpenChange = useCallback((open: boolean) => {
    if (!open) setEntityToEdit(undefined);
    setIsFormOpen(open);
  }, []);

  const handleDeleteDialogOpenChange = useCallback((open: boolean) => {
    if (!open) setEntityToDelete(undefined);
  }, []);

  const openCreateForm = useCallback(() => {
    setEntityToEdit(undefined);
    setIsFormOpen(true);
  }, []);

  return {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
    openCreateForm,
  };
}
