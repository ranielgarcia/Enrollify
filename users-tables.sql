-- ====================================
-- USERS, ROLES, AND PERMISSIONS SCHEMA
-- ====================================

-- ************************************
-- USERS TABLE
-- ************************************
CREATE TABLE Users
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    PasswordChangedAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIME2 NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT CHK_Users_Username_NotEmpty CHECK (LEN(TRIM(Username)) > 0),
    CONSTRAINT CHK_Users_Email_NotEmpty CHECK (LEN(TRIM(Email)) > 0),
    CONSTRAINT CHK_Users_FirstName_NotEmpty CHECK (LEN(TRIM(FirstName)) > 0),
    CONSTRAINT CHK_Users_LastName_NotEmpty CHECK (LEN(TRIM(LastName)) > 0),
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Users_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO

-- ************************************
-- ROLES TABLE
-- ************************************
CREATE TABLE Roles
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIME2 NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT CHK_Roles_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0),
    CONSTRAINT FK_Roles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Roles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Roles_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO

-- ************************************
-- PERMISSIONS TABLE
-- ************************************
CREATE TABLE Permissions
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Resource VARCHAR(50) NOT NULL,
    Action VARCHAR(50) NOT NULL,
    Description VARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIME2 NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT CHK_Permissions_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0),
    CONSTRAINT CHK_Permissions_Resource_NotEmpty CHECK (LEN(TRIM(Resource)) > 0),
    CONSTRAINT CHK_Permissions_Action_NotEmpty CHECK (LEN(TRIM(Action)) > 0),
    CONSTRAINT FK_Permissions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Permissions_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Permissions_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO

-- ************************************
-- USER-ROLE MAPPING TABLE
-- ************************************
CREATE TABLE UserRoles
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2 DEFAULT SYSDATETIME(),
    ExpiresAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIME2 NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_UserRoles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO

-- Unique index to ensure one active role assignment per user
CREATE UNIQUE NONCLUSTERED INDEX UIdx_UserRoles_User_Role_IsActive
ON UserRoles(UserId, RoleId)
WHERE IsActive = 1;
GO

-- ************************************
-- ROLE-PERMISSION MAPPING TABLE
-- ************************************
CREATE TABLE RolePermissions
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIME2 NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Permissions(Id),
    CONSTRAINT FK_RolePermissions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_RolePermissions_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_RolePermissions_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO

-- Unique index to ensure one active permission assignment per role
CREATE UNIQUE NONCLUSTERED INDEX UIdx_RolePermissions_Role_Permission_IsActive
ON RolePermissions(RoleId, PermissionId)
WHERE IsActive = 1;
GO

-- ************************************
-- INDEXES ON FOREIGN KEYS FOR PERFORMANCE
-- ************************************

-- Users indexes
CREATE NONCLUSTERED INDEX IX_Users_CreatedBy 
ON Users(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_Users_UpdatedBy 
ON Users(UpdatedBy);
GO

CREATE NONCLUSTERED INDEX IX_Users_DeletedBy 
ON Users(DeletedBy);
GO

-- Roles indexes
CREATE NONCLUSTERED INDEX IX_Roles_CreatedBy 
ON Roles(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_Roles_UpdatedBy 
ON Roles(UpdatedBy);
GO

CREATE NONCLUSTERED INDEX IX_Roles_DeletedBy 
ON Roles(DeletedBy);
GO

-- Permissions indexes
CREATE NONCLUSTERED INDEX IX_Permissions_CreatedBy 
ON Permissions(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_Permissions_UpdatedBy 
ON Permissions(UpdatedBy);
GO

CREATE NONCLUSTERED INDEX IX_Permissions_DeletedBy 
ON Permissions(DeletedBy);
GO

-- UserRoles indexes
CREATE NONCLUSTERED INDEX IX_UserRoles_UserId 
ON UserRoles(UserId);
GO

CREATE NONCLUSTERED INDEX IX_UserRoles_RoleId 
ON UserRoles(RoleId);
GO

CREATE NONCLUSTERED INDEX IX_UserRoles_CreatedBy 
ON UserRoles(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_UserRoles_UpdatedBy 
ON UserRoles(UpdatedBy);
GO

CREATE NONCLUSTERED INDEX IX_UserRoles_DeletedBy 
ON UserRoles(DeletedBy);
GO

-- RolePermissions indexes
CREATE NONCLUSTERED INDEX IX_RolePermissions_RoleId 
ON RolePermissions(RoleId);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_PermissionId 
ON RolePermissions(PermissionId);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_CreatedBy 
ON RolePermissions(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_UpdatedBy 
ON RolePermissions(UpdatedBy);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_DeletedBy 
ON RolePermissions(DeletedBy);
GO

-- ************************************
-- SEED DATA FOR ROLES
-- ************************************

-- Insert default roles
INSERT INTO Roles (Name, Description) VALUES
('SuperAdmin', 'Full system access with all permissions'),
('Admin', 'Administrative access to manage system data'),
('Registrar', 'Manage student enrollments and academic records'),
('Dean', 'College-level academic oversight'),
('Department Chair', 'Department-level management'),
('Teacher', 'Manage classes, grades, and student records'),
('Student', 'View schedules, grades, and enrollment information'),
('Guest', 'Limited read-only access');
GO

-- ************************************
-- SEED DATA FOR PERMISSIONS (Examples)
-- ************************************

-- User Management Permissions
INSERT INTO Permissions (Name, Resource, Action, Description) VALUES
('users.create', 'Users', 'CREATE', 'Create new users'),
('users.read', 'Users', 'READ', 'View user information'),
('users.update', 'Users', 'UPDATE', 'Update user information'),
('users.delete', 'Users', 'DELETE', 'Delete users'),

-- Role Management Permissions
('roles.create', 'Roles', 'CREATE', 'Create new roles'),
('roles.read', 'Roles', 'READ', 'View role information'),
('roles.update', 'Roles', 'UPDATE', 'Update role information'),
('roles.delete', 'Roles', 'DELETE', 'Delete roles'),

-- Student Management Permissions
('students.create', 'Students', 'CREATE', 'Register new students'),
('students.read', 'Students', 'READ', 'View student information'),
('students.update', 'Students', 'UPDATE', 'Update student information'),
('students.delete', 'Students', 'DELETE', 'Delete students'),

-- Enrollment Management Permissions
('enrollments.create', 'Enrollments', 'CREATE', 'Create new enrollments'),
('enrollments.read', 'Enrollments', 'READ', 'View enrollment information'),
('enrollments.update', 'Enrollments', 'UPDATE', 'Update enrollment information'),
('enrollments.delete', 'Enrollments', 'DELETE', 'Delete enrollments'),

-- Grade Management Permissions
('grades.create', 'Grades', 'CREATE', 'Enter grades'),
('grades.read', 'Grades', 'READ', 'View grades'),
('grades.update', 'Grades', 'UPDATE', 'Update grades'),
('grades.delete', 'Grades', 'DELETE', 'Delete grades'),

-- Schedule Management Permissions
('schedules.create', 'Schedules', 'CREATE', 'Create class schedules'),
('schedules.read', 'Schedules', 'READ', 'View class schedules'),
('schedules.update', 'Schedules', 'UPDATE', 'Update class schedules'),
('schedules.delete', 'Schedules', 'DELETE', 'Delete class schedules');
GO
