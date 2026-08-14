/**
 * Resolves the API base URL.
 *
 * During local development the Angular dev server runs on port 4200 and talks to the
 * API container/host directly (CORS is enabled for this origin). In the shipped image
 * nginx serves the app and proxies /api to the API container, so a relative URL is used.
 */
export function resolveApiBaseUrl(): string {
  return window.location.port === '4200' ? 'http://localhost:8080' : '';
}
