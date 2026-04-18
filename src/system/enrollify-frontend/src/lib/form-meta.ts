export type FormMeta = {
  submitAction: "create" | "update" | null;
  formAction: "close" | "stayopen" | null;
};

export const defaultFormMeta: FormMeta = {
  submitAction: null,
  formAction: null,
};
