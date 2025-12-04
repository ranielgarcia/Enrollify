https://stackblitz.com/~/github.com/kurochenko/tanstack-router-breadcrumbs-example?file=src/components/BreadcrumbNav.tsx:L1-L36

```javascript
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import { isMatch, Link, useMatches } from '@tanstack/react-router';

export const BreadcrumbNav = () => {
const matches = useMatches();
const matchesWithCrumbs = matches.filter((match) =>
  isMatch(match, 'loaderData.crumb'),
);

const items = matchesWithCrumbs.map(({ pathname, loaderData }) => {
  return {
    href: pathname,
    label: loaderData?.crumb,
  };
});

  return (
    <Breadcrumb>
      <BreadcrumbList>
        {items.map((item, index) => (
          <BreadcrumbItem key={index}>
            <Link to={item.href} className="breadcrumb-link">
              {item.label}
            </Link>
            {index < items.length - 1 && <BreadcrumbSeparator />}
          </BreadcrumbItem>
        ))}
      </BreadcrumbList>
    </Breadcrumb>
  );
};

```


```javascript
import { createFileRoute, Outlet } from "@tanstack/react-router";
import { findOrganizationByID } from "@/api/getOrganizationByID.tsx";

const RouteComponent = () => <Outlet />;

export const Route = createFileRoute("/organizations/$orgId")({
  component: RouteComponent,
  loader: ({params}) => {
    const { orgId } = params;
    const organization = findOrganizationByID(orgId);
    return {
      crumb: organization!.name,
    }
  },
});

```


```javascript
import { createFileRoute } from "@tanstack/react-router";
import { EmployeeDetail } from "@/components/EmployeeDetail.tsx";
import { getEmployeeByID } from "@/api/getEmployeeByID.tsx";

export const Route = createFileRoute(
  "/organizations/$orgId/employees/$employeeId",
)({
  component: RouteComponent,
  loader: ({ params }) => {
    const { employeeId } = params;
    const employee = getEmployeeByID(employeeId);
    return {
      crumb: `${employee!.name} ${employee!.surname}`,
    };
  },
});

function RouteComponent() {
  return <EmployeeDetail />;
}
```