-- =============================================================================
-- Creates the Notifications and NotificationRecipients tables with full audit
-- columns, performance indexes, and foreign key constraints.
-- =============================================================================

-- ---------------------------------------------------------------------------
-- 1. Notifications (main table)
-- ---------------------------------------------------------------------------
CREATE TABLE [Notifications]
(
  [Id]            INT             NOT NULL IDENTITY(1,1),
  [Type]          NVARCHAR(100)   NOT NULL,           -- e.g. 'CurriculumCreated', 'BackgroundJobCompleted'
  [Title]         NVARCHAR(500)   NOT NULL,
  [Message]       NVARCHAR(MAX)   NOT NULL,
  [Severity]      NVARCHAR(20)    NOT NULL DEFAULT 'Info',   -- Info | Success | Warning | Error
  [Category]      NVARCHAR(50)    NOT NULL DEFAULT 'System', -- Academic | Enrollment | System | Admin | Audit
  [ReferenceType] NVARCHAR(100)   NULL,               -- Polymorphic: 'Curriculum', 'Subject', 'Schedule', etc.
  [ReferenceId]   INT             NULL,               -- FK value (not enforced — polymorphic)
  [TargetScope]   NVARCHAR(20)    NOT NULL DEFAULT 'Broadcast', -- User | Role | Broadcast
  [TargetUserId]  INT             NULL,               -- Populated when TargetScope = 'User'
  [TargetRoleId]  INT             NULL,               -- Populated when TargetScope = 'Role'
  [RetentionDays] INT             NOT NULL DEFAULT 5,
  [ExpiresAt]     DATETIMEOFFSET    NULL,               -- Computed at insert: CreatedAt + RetentionDays

  -- Standard Enrollify audit columns
  [CreatedAt]     DATETIMEOFFSET    NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy]     INT             NOT NULL,

  CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC),

  CONSTRAINT [FK_Notifications_CreatedBy_Users]
    FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]),

  CONSTRAINT [FK_Notifications_TargetUserId_Users]
    FOREIGN KEY ([TargetUserId]) REFERENCES [Users] ([Id]),

  CONSTRAINT [FK_Notifications_TargetRoleId_Roles]
    FOREIGN KEY ([TargetRoleId]) REFERENCES [Roles] ([Id]),

  CONSTRAINT [CK_Notifications_Severity]
    CHECK ([Severity] IN ('Info', 'Success', 'Warning', 'Error')),

  CONSTRAINT [CK_Notifications_TargetScope]
    CHECK ([TargetScope] IN ('User', 'Role', 'Broadcast')),

  CONSTRAINT [CK_Notifications_TargetScope_User]
    CHECK ([TargetScope] <> 'User' OR [TargetUserId] IS NOT NULL),

  CONSTRAINT [CK_Notifications_TargetScope_Role]
    CHECK ([TargetScope] <> 'Role' OR [TargetRoleId] IS NOT NULL)
);
GO

-- ---------------------------------------------------------------------------
-- 2. NotificationRecipients (junction / read-state table)
-- ---------------------------------------------------------------------------
CREATE TABLE [NotificationRecipients]
(
  [Id]             INT          NOT NULL IDENTITY(1,1),
  [NotificationId] INT          NOT NULL,
  [UserId]         INT          NOT NULL,
  [IsRead]         BIT          NOT NULL DEFAULT 0,
  [ReadAt]         DATETIMEOFFSET NULL,
  [IsDismissed]    BIT          NOT NULL DEFAULT 0,
  [DismissedAt]    DATETIMEOFFSET NULL,

  CONSTRAINT [PK_NotificationRecipients] PRIMARY KEY CLUSTERED ([Id] ASC),

  CONSTRAINT [FK_NotificationRecipients_NotificationId_Notifications]
    FOREIGN KEY ([NotificationId]) REFERENCES [Notifications] ([Id]),

  CONSTRAINT [FK_NotificationRecipients_UserId_Users]
    FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

-- ---------------------------------------------------------------------------
-- 3. Performance indexes
-- ---------------------------------------------------------------------------

-- Most common query: unread notifications for a user
CREATE NONCLUSTERED INDEX [IX_NotificationRecipients_UserId_IsRead]
  ON [NotificationRecipients] ([UserId] ASC, [IsRead] ASC)
  INCLUDE ([NotificationId], [IsDismissed])
GO

-- Cleanup job: find expired active notifications
CREATE NONCLUSTERED INDEX [IX_Notifications_ExpiresAt]
  ON [Notifications] ([ExpiresAt] ASC)
  INCLUDE ([Id], [RetentionDays]);
GO

-- Polymorphic lookups: e.g. "all notifications about Curriculum #5"
CREATE NONCLUSTERED INDEX [IX_Notifications_ReferenceType_ReferenceId]
  ON [Notifications] ([ReferenceType] ASC, [ReferenceId] ASC)
GO

-- Role-targeted notifications
CREATE NONCLUSTERED INDEX [IX_Notifications_TargetRoleId]
  ON [Notifications] ([TargetRoleId] ASC)
  WHERE [TargetRoleId] IS NOT NULL;
GO
