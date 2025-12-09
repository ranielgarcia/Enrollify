/**
 * Common loader data structure for all routes with breadcrumb support.
 * Every route should return this from its loader function.
 */
export interface RouteLoaderData {
  /**
   * The breadcrumb label for this route.
   * Set to `undefined` if this route should not appear in breadcrumbs.
   */
  crumb: string | undefined;
}

/**
 * Type helper for creating typed loader functions.
 * Use this to ensure your loader returns the correct shape.
 *
 * @example
 * ```ts
 * export const Route = createFileRoute("/portal/dashboard")({
 *   loader: (): RouteLoaderData => ({
 *     crumb: "Dashboard",
 *   }),
 *   component: DashboardComponent,
 * });
 * ```
 */
export type RouteLoader = () => RouteLoaderData;

/**
 * Async version of RouteLoader for loaders that need to fetch data.
 *
 * @example
 * ```ts
 * export const Route = createFileRoute("/portal/users/$userId")({
 *   loader: async ({ params }): Promise<RouteLoaderData> => {
 *     const user = await fetchUser(params.userId);
 *     return {
 *       crumb: user.name,
 *     };
 *   },
 *   component: UserComponent,
 * });
 * ```
 */
export type AsyncRouteLoader<T = unknown> = (
  context: T
) => Promise<RouteLoaderData>;
