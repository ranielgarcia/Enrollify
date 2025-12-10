# Usage examples

## Example 1: Policy Registration

```typescript
// defaultPolicies.ts
export const registerDefaultPolicies = (registry: PolicyRegistry) => {
  // Simple role-based policy
  registry.register(
    new PolicyBuilder('AdminOnly')
      .requireRole('Admin', 'SuperAdmin')
      .build()
  );

  // Permission-based policy
  registry.register(
    new PolicyBuilder('CanManageStudents')
      .requirePermission('students.manage')
      .build()
  );

  // Scoped permission policy
  registry.register(
    new PolicyBuilder('CanManageDepartmentStudents')
      .requirePermission('students.manage')
      .requireScope('department')
      .build()
  );

  // Complex multi-requirement policy (AND)
  registry.register(
    new PolicyBuilder('EnrollmentManager')
      .requireRole('Registrar', 'Dean')
      .requirePermission('enrollment.approve')
      .requireAll()
      .build()
  );

  // Alternative requirements policy (OR)
  registry.register(
    new PolicyBuilder('CanViewReports')
      .requireRole('Admin', 'Registrar', 'Dean')
      .requireAny()
      .build()
  );

  // Department-scoped policy
  registry.register(
    new PolicyBuilder('DepartmentAdmin')
      .requireRole('DepartmentHead')
      .requireScope('department')
      .requirePermission('department.manage')
      .build()
  );
};
```


### Example 2: Protecting Pages

```typescript
// pages/StudentManagementPage.tsx
const StudentManagementPage = () => {
  return (
    <Authorized 
      policy="CanManageStudents"
      fallback={<LoadingSpinner />}
      unauthorized={<AccessDenied />}
    >
      <StudentManagementContent />
    </Authorized>
  );
};

// With scope
const DepartmentStudentsPage = () => {
  const { departmentId } = useParams();
  
  return (
    <Authorized 
      policy="CanManageDepartmentStudents"
      scope={{ type: 'department', id: departmentId }}
      unauthorized={<AccessDenied />}
    >
      <DepartmentStudentsList />
    </Authorized>
  );
};
```

### Example 3: Protecting Buttons/Actions

```typescript
// components/StudentActions.tsx
const StudentActions = ({ student }) => {
  return (
    <div className="actions">
      <Authorized permissions={['students.view']}>
        <Button onClick={() => viewStudent(student)}>View</Button>
      </Authorized>

      <Authorized permissions={['students.edit']}>
        <Button onClick={() => editStudent(student)}>Edit</Button>
      </Authorized>

      <Authorized roles={['Admin', 'Registrar']}>
        <Button onClick={() => deleteStudent(student)} variant="danger">
          Delete
        </Button>
      </Authorized>

      <Authorized 
        permissions={['enrollment.approve']}
        scope={{ type: 'department', id: student.departmentId }}
      >
        <Button onClick={() => approveEnrollment(student)}>
          Approve Enrollment
        </Button>
      </Authorized>
    </div>
  );
};
```

### Example 4: Using AuthorizeView

```typescript
const DashboardWidget = () => {
  return (
    <AuthorizeView
      policy="CanViewReports"
      authorized={
        <ReportsWidget />
      }
      unauthorized={
        <Card>
          <p>You don't have permission to view reports.</p>
        </Card>
      }
    />
  );
};
```

### Example 5: Programmatic Authorization

```typescript
const EnrollmentForm = () => {
  const { checkPolicy, hasPermission } = useAuthorization();
  const { departmentId } = useParams();

  const handleSubmit = async (data: EnrollmentData) => {
    // Check permission before submitting
    const canApprove = await checkPolicy('EnrollmentManager');
    
    if (canApprove) {
      data.status = 'Approved';
    } else {
      data.status = 'Pending';
    }

    await submitEnrollment(data);
  };

  const handleDelete = async (enrollmentId: string) => {
    const hasDeletePermission = hasPermission(
      'enrollment.delete',
      { type: 'department', id: departmentId }
    );

    if (!hasDeletePermission) {
      toast.error('You do not have permission to delete enrollments');
      return;
    }

    await deleteEnrollment(enrollmentId);
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* form fields */}
    </form>
  );
};
```

### Example 6: Multiple Requirements

```typescript
// Require ANY of the roles
<Authorized 
  roles={['Admin', 'SuperAdmin', 'Registrar']} 
  requireAll={false}
>
  <AdminPanel />
</Authorized>

// Require ALL permissions
<Authorized 
  permissions={['students.view', 'students.edit', 'students.delete']}
  requireAll={true}
>
  <AdvancedStudentManager />
</Authorized>

// Mix roles and permissions (all must pass)
<Authorized 
  roles={['Dean']}
  permissions={['curriculum.approve']}
  scope={{ type: 'college', id: collegeId }}
>
  <CurriculumApprovalButton />
</Authorized>
```

### Example 7: Route Protection

```typescript
// routes/ProtectedRoute.tsx
const ProtectedRoute = ({ 
  policy, 
  roles, 
  permissions,
  scope,
  element 
}: ProtectedRouteProps) => {
  return (
    <Authorized
      policy={policy}
      roles={roles}
      permissions={permissions}
      scope={scope}
      unauthorized={<Navigate to="/unauthorized" replace />}
    >
      {element}
    </Authorized>
  );
};

// router.tsx
const router = createBrowserRouter([
  {
    path: '/students',
    element: (
      <ProtectedRoute 
        policy="CanManageStudents" 
        element={<StudentManagementPage />} 
      />
    ),
  },
  {
    path: '/admin',
    element: (
      <ProtectedRoute 
        roles={['Admin', 'SuperAdmin']} 
        element={<AdminDashboard />} 
      />
    ),
  },
  {
    path: '/department/:deptId/enrollment',
    element: (
      <ProtectedRoute 
        permissions={['enrollment.manage']}
        // Scope will be set dynamically based on route params
        element={<DepartmentEnrollmentPage />} 
      />
    ),
  },
]);
```
