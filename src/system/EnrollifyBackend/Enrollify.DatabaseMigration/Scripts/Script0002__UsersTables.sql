
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
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
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
CREATE TABLE UserRolesAssignments
(
    Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
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
    
    CONSTRAINT FK_UserRolesAssignments_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRolesAssignments_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_UserRolesAssignments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_UserRolesAssignments_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_UserRolesAssignments_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
);
GO

-- Unique index to ensure one active role assignment per user
CREATE UNIQUE NONCLUSTERED INDEX UIdx_UserRolesAssignments_User_Role_IsActive
ON UserRolesAssignments(UserId, RoleId)
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
    CreatedAt DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy INT NULL,
    DeletedAt DATETIMEOFFSET NULL,
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
