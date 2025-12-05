-- ====================================
-- USERS, ROLES, AND PERMISSIONS SCHEMA
-- ====================================

-- ************************************
-- USERS TABLE
-- ************************************
CREATE TABLE Users
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    LastLoginAt DATETIMEOFFSET NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT CHK_Users_Email_NotEmpty CHECK (LEN(TRIM(Email)) > 0),
    CONSTRAINT CHK_Users_FirstName_NotEmpty CHECK (LEN(TRIM(FirstName)) > 0),
    CONSTRAINT CHK_Users_LastName_NotEmpty CHECK (LEN(TRIM(LastName)) > 0),
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Users_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- ************************************
-- ROLES TABLE
-- ************************************
CREATE TABLE Roles
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description TEXT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT CHK_Roles_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0),
    CONSTRAINT FK_Roles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Roles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Roles_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);

-- ************************************
-- USER-ROLE MAPPING TABLE
-- ************************************
CREATE TABLE UserRolesAssignments
(
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    ExpiresAt DATETIMEOFFSET NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,
    
    CONSTRAINT PK_UserRoleAssignments PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRolesAssignments_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRolesAssignments_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_UserRolesAssignments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_UserRolesAssignments_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_UserRolesAssignments_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);


-- Unique index to ensure one active role assignment per user
CREATE UNIQUE NONCLUSTERED INDEX UIdx_UserRolesAssignments_User_Role_IsActive
ON UserRolesAssignments(UserId, RoleId)
WHERE IsActive = 1;
GO


CREATE TABLE PermissionScopes
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE, -- e.g. Users, Courses, Enrollments

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,

    CONSTRAINT CHK_PermissionScopes_Name_NotEmpty CHECK (LEN(TRIM(Name)) > 0),
    CONSTRAINT FK_PermissionScopes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_PermissionScopes_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_PermissionScopes_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
)


-- ************************************
-- ROLE-PERMISSION MAPPING TABLE
-- ************************************
CREATE TABLE RolePermissions
(
    RoleId INT NOT NULL,
    PermissionScopeId INT NOT NULL,
    BitmaskPermission INT NOT NULL, -- e.g 1, 2, 4, 8 (bits)

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
    DeletedBy INT NULL,

    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionScopeId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_RolePermissions_PermissionScope FOREIGN KEY (PermissionScopeId) REFERENCES PermissionScopes(Id),
    CONSTRAINT FK_RolePermissions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_RolePermissions_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_RolePermissions_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);


-- Unique index to ensure one active permission assignment per role
CREATE UNIQUE NONCLUSTERED INDEX UIdx_RolePermissions_Role_Permission_IsActive
ON RolePermissions(RoleId, PermissionScopeId)
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

-- UserRoles indexes
CREATE NONCLUSTERED INDEX IX_UserRolesAssignments_UserId 
ON UserRolesAssignments(UserId);
GO

CREATE NONCLUSTERED INDEX IX_UserRolesAssignments_RoleId 
ON UserRolesAssignments(RoleId);
GO

CREATE NONCLUSTERED INDEX IX_UserRolesAssignments_CreatedBy 
ON UserRolesAssignments(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_UserRolesAssignments_UpdatedBy 
ON UserRolesAssignments(UpdatedBy);
GO

CREATE NONCLUSTERED INDEX IX_UserRolesAssignments_DeletedBy 
ON UserRolesAssignments(DeletedBy);
GO

-- RolePermissions indexes
CREATE NONCLUSTERED INDEX IX_RolePermissions_RoleId 
ON RolePermissions(RoleId);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_PermissionId 
ON RolePermissions(BitmaskPermission);
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


-- ====================================
-- SEED Initial System's User and Role
-- ====================================
-- 1) Temporarily disable the self-referencing FK
ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_CreatedBy;
GO

-- 2) Seed a System user with Id=1 referencing itself
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, Email, FirstName, LastName, LastLoginAt, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy)
VALUES (1, 'system@enrollify.local', 'System', 'Account', NULL, 1, SYSDATETIME(), 1, NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT Users OFF;
GO

-- 3) Re-enable and validate the FK
ALTER TABLE Users WITH CHECK CHECK CONSTRAINT FK_Users_CreatedBy;
GO

INSERT INTO Roles (Name, Description, CreatedBy) VALUES
('SuperAdmin', 'Full system access with all permissions', 1);
GO
