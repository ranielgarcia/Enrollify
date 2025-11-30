export const Config = {
  Auth: {
    ClientId:
      import.meta.env.VITE_OAUTH_CLIENT_ID ||
      "838fcb31-28a1-4937-a833-531017ecf287",
    Authority:
      import.meta.env.VITE_OAUTH_AUTHORITY ||
      "https://laundrocustomers.ciamlogin.com/",
    RedirectUri:
      import.meta.env.VITE_OAUTH_REDIRECT_URI ||
      "http://localhost:5173/oauth/callback",
    Scope:
      import.meta.env.VITE_OAUTH_SCOPE ||
      "api://e2f2cdfc-8075-44cb-834c-fbd6f3069c4d/user_impersonation",
  },
  API_URL: import.meta.env.VITE_API_URL || "https://localhost:7107/api",
  SIGNIN_FLOW: import.meta.env.VITE_SIGNIN_FLOW || "popup",
  VERBOSE_AUTH_LOGGING: import.meta.env.VITE_VERBOSE_AUTH_LOGGING === "true",
  LOG_SENSITIVE_INFORMATION:
    import.meta.env.VITE_LOG_SENSITIVE_INFORMATION === "true",
} as const;
