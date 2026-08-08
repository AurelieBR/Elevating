IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720183425_InitialCreate'
)
BEGIN
    CREATE TABLE [Goals] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [TargetDate] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Goals] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720183425_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Goals_Category] ON [Goals] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720183425_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Goals_Status] ON [Goals] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720183425_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720183425_InitialCreate', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731211358_AddGoalActions'
)
BEGIN
    CREATE TABLE [GoalActions] (
        [Id] int NOT NULL IDENTITY,
        [GoalId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Status] int NOT NULL,
        [Position] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_GoalActions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoalActions_Goals_GoalId] FOREIGN KEY ([GoalId]) REFERENCES [Goals] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731211358_AddGoalActions'
)
BEGIN
    CREATE INDEX [IX_GoalActions_GoalId] ON [GoalActions] ([GoalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731211358_AddGoalActions'
)
BEGIN
    CREATE INDEX [IX_GoalActions_GoalId_Position] ON [GoalActions] ([GoalId], [Position]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731211358_AddGoalActions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731211358_AddGoalActions', N'10.0.10');
END;

COMMIT;
GO

