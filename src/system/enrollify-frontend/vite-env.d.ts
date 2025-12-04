interface ViteTypeOptions {
  // By adding this line, you can make the type of ImportMetaEnv strict
  // to disallow unknown keys.
  // strictImportMetaEnv: unknown
}

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  readonly VITE_OAUTH_CLIENT_ID: string;
  readonly VITE_OAUTH_AUTHORITY: string;
  readonly VITE_OAUTH_REDIRECT_URI: string;
  readonly VITE_OAUTH_SCOPE: string;
  readonly VITE_SIGNIN_FLOW: string;
  readonly VITE_VERBOSE_AUTH_LOGGING: string;
  readonly VITE_LOG_SENSITIVE_INFORMATION: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
