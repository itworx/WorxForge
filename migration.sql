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
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [ASSET_TYPE] (
        [asset_type_id] uniqueidentifier NOT NULL,
        [name] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_ASSET_TYPE] PRIMARY KEY ([asset_type_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [DOMAIN] (
        [domain_id] uniqueidentifier NOT NULL,
        [name] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_DOMAIN] PRIMARY KEY ([domain_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [TAG] (
        [tag_id] uniqueidentifier NOT NULL,
        [name] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_TAG] PRIMARY KEY ([tag_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [TOOL] (
        [tool_id] uniqueidentifier NOT NULL,
        [name] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_TOOL] PRIMARY KEY ([tool_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [USE_CASE] (
        [use_case_id] uniqueidentifier NOT NULL,
        [name] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_USE_CASE] PRIMARY KEY ([use_case_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [USER] (
        [user_id] uniqueidentifier NOT NULL,
        [username] nvarchar(max) NOT NULL,
        [display_name] nvarchar(max) NOT NULL,
        [email] nvarchar(max) NOT NULL,
        [email_verified] bit NOT NULL,
        [role] nvarchar(max) NOT NULL,
        [google_id] uniqueidentifier NOT NULL,
        [avatar_url] nvarchar(max) NULL,
        [created_at] datetime2 NOT NULL,
        [updated_at] datetime2 NOT NULL,
        [deleted_at] datetime2 NULL,
        CONSTRAINT [PK_USER] PRIMARY KEY ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [ASSET] (
        [asset_id] uniqueidentifier NOT NULL,
        [author_id] uniqueidentifier NOT NULL,
        [title] nvarchar(max) NOT NULL,
        [description] nvarchar(max) NULL,
        [markdown_content] nvarchar(max) NOT NULL,
        [asset_type_id] uniqueidentifier NOT NULL,
        [domain_id] uniqueidentifier NOT NULL,
        [use_case_id] uniqueidentifier NOT NULL,
        [created_at] datetime2 NOT NULL,
        [updated_at] datetime2 NOT NULL,
        CONSTRAINT [PK_ASSET] PRIMARY KEY ([asset_id]),
        CONSTRAINT [FK_ASSET_ASSET_TYPE_asset_type_id] FOREIGN KEY ([asset_type_id]) REFERENCES [ASSET_TYPE] ([asset_type_id]),
        CONSTRAINT [FK_ASSET_DOMAIN_domain_id] FOREIGN KEY ([domain_id]) REFERENCES [DOMAIN] ([domain_id]),
        CONSTRAINT [FK_ASSET_USER_author_id] FOREIGN KEY ([author_id]) REFERENCES [USER] ([user_id]),
        CONSTRAINT [FK_ASSET_USE_CASE_use_case_id] FOREIGN KEY ([use_case_id]) REFERENCES [USE_CASE] ([use_case_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [AUTH_SESSION] (
        [session_id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [token_hash] nvarchar(max) NOT NULL,
        [issued_at] datetime2 NOT NULL,
        [expires_at] datetime2 NOT NULL,
        [revoked_at] datetime2 NULL,
        CONSTRAINT [PK_AUTH_SESSION] PRIMARY KEY ([session_id]),
        CONSTRAINT [FK_AUTH_SESSION_USER_user_id] FOREIGN KEY ([user_id]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [REPORT] (
        [report_id] uniqueidentifier NOT NULL,
        [reporter_user_id] uniqueidentifier NOT NULL,
        [resolved_by] uniqueidentifier NULL,
        [reason] nvarchar(max) NOT NULL,
        [status] nvarchar(max) NOT NULL,
        [created_at] datetime2 NOT NULL,
        [resolved_at] datetime2 NULL,
        CONSTRAINT [PK_REPORT] PRIMARY KEY ([report_id]),
        CONSTRAINT [FK_REPORT_USER_reporter_user_id] FOREIGN KEY ([reporter_user_id]) REFERENCES [USER] ([user_id]),
        CONSTRAINT [FK_REPORT_USER_resolved_by] FOREIGN KEY ([resolved_by]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [ASSET_TAG] (
        [asset_id] uniqueidentifier NOT NULL,
        [tag_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ASSET_TAG] PRIMARY KEY ([asset_id], [tag_id]),
        CONSTRAINT [FK_ASSET_TAG_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_ASSET_TAG_TAG_tag_id] FOREIGN KEY ([tag_id]) REFERENCES [TAG] ([tag_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [ASSET_TOOL] (
        [asset_id] uniqueidentifier NOT NULL,
        [tool_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ASSET_TOOL] PRIMARY KEY ([asset_id], [tool_id]),
        CONSTRAINT [FK_ASSET_TOOL_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_ASSET_TOOL_TOOL_tool_id] FOREIGN KEY ([tool_id]) REFERENCES [TOOL] ([tool_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [ASSET_VIEW] (
        [user_id] uniqueidentifier NOT NULL,
        [asset_id] uniqueidentifier NOT NULL,
        [last_viewed_at] datetime2 NOT NULL,
        [view_count] int NOT NULL,
        CONSTRAINT [PK_ASSET_VIEW] PRIMARY KEY ([user_id], [asset_id]),
        CONSTRAINT [FK_ASSET_VIEW_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_ASSET_VIEW_USER_user_id] FOREIGN KEY ([user_id]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [BOOKMARK] (
        [user_id] uniqueidentifier NOT NULL,
        [asset_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_BOOKMARK] PRIMARY KEY ([user_id], [asset_id]),
        CONSTRAINT [FK_BOOKMARK_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_BOOKMARK_USER_user_id] FOREIGN KEY ([user_id]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [COMMENT] (
        [comment_id] uniqueidentifier NOT NULL,
        [asset_id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [parent_comment_id] uniqueidentifier NULL,
        [content] nvarchar(max) NOT NULL,
        [created_at] datetime2 NOT NULL,
        [deleted_at] datetime2 NULL,
        CONSTRAINT [PK_COMMENT] PRIMARY KEY ([comment_id]),
        CONSTRAINT [FK_COMMENT_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_COMMENT_COMMENT_parent_comment_id] FOREIGN KEY ([parent_comment_id]) REFERENCES [COMMENT] ([comment_id]),
        CONSTRAINT [FK_COMMENT_USER_user_id] FOREIGN KEY ([user_id]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [REACTION] (
        [asset_id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [reaction_type] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_REACTION] PRIMARY KEY ([asset_id], [user_id]),
        CONSTRAINT [FK_REACTION_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_REACTION_USER_user_id] FOREIGN KEY ([user_id]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [REPORT_ASSET] (
        [report_id] uniqueidentifier NOT NULL,
        [asset_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_REPORT_ASSET] PRIMARY KEY ([report_id]),
        CONSTRAINT [FK_REPORT_ASSET_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_REPORT_ASSET_REPORT_report_id] FOREIGN KEY ([report_id]) REFERENCES [REPORT] ([report_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [NOTIFICATION] (
        [notification_id] uniqueidentifier NOT NULL,
        [recipient_user_id] uniqueidentifier NOT NULL,
        [actor_user_id] uniqueidentifier NULL,
        [type] nvarchar(max) NOT NULL,
        [message] nvarchar(max) NOT NULL,
        [asset_id] uniqueidentifier NULL,
        [comment_id] uniqueidentifier NULL,
        [report_id] uniqueidentifier NULL,
        [is_read] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [read_at] datetime2 NULL,
        CONSTRAINT [PK_NOTIFICATION] PRIMARY KEY ([notification_id]),
        CONSTRAINT [FK_NOTIFICATION_ASSET_asset_id] FOREIGN KEY ([asset_id]) REFERENCES [ASSET] ([asset_id]),
        CONSTRAINT [FK_NOTIFICATION_COMMENT_comment_id] FOREIGN KEY ([comment_id]) REFERENCES [COMMENT] ([comment_id]),
        CONSTRAINT [FK_NOTIFICATION_REPORT_report_id] FOREIGN KEY ([report_id]) REFERENCES [REPORT] ([report_id]),
        CONSTRAINT [FK_NOTIFICATION_USER_actor_user_id] FOREIGN KEY ([actor_user_id]) REFERENCES [USER] ([user_id]),
        CONSTRAINT [FK_NOTIFICATION_USER_recipient_user_id] FOREIGN KEY ([recipient_user_id]) REFERENCES [USER] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE TABLE [REPORT_COMMENT] (
        [report_id] uniqueidentifier NOT NULL,
        [comment_id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_REPORT_COMMENT] PRIMARY KEY ([report_id]),
        CONSTRAINT [FK_REPORT_COMMENT_COMMENT_comment_id] FOREIGN KEY ([comment_id]) REFERENCES [COMMENT] ([comment_id]),
        CONSTRAINT [FK_REPORT_COMMENT_REPORT_report_id] FOREIGN KEY ([report_id]) REFERENCES [REPORT] ([report_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'asset_type_id', N'name') AND [object_id] = OBJECT_ID(N'[ASSET_TYPE]'))
        SET IDENTITY_INSERT [ASSET_TYPE] ON;
    EXEC(N'INSERT INTO [ASSET_TYPE] ([asset_type_id], [name])
    VALUES (''11111111-1111-1111-1111-111111111101'', N''Prompt''),
    (''11111111-1111-1111-1111-111111111102'', N''Skill''),
    (''11111111-1111-1111-1111-111111111103'', N''Agent'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'asset_type_id', N'name') AND [object_id] = OBJECT_ID(N'[ASSET_TYPE]'))
        SET IDENTITY_INSERT [ASSET_TYPE] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'domain_id', N'name') AND [object_id] = OBJECT_ID(N'[DOMAIN]'))
        SET IDENTITY_INSERT [DOMAIN] ON;
    EXEC(N'INSERT INTO [DOMAIN] ([domain_id], [name])
    VALUES (''21111111-1111-1111-1111-111111111101'', N''Development''),
    (''21111111-1111-1111-1111-111111111102'', N''Quality Assurance''),
    (''21111111-1111-1111-1111-111111111103'', N''DevOps'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'domain_id', N'name') AND [object_id] = OBJECT_ID(N'[DOMAIN]'))
        SET IDENTITY_INSERT [DOMAIN] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'tool_id', N'name') AND [object_id] = OBJECT_ID(N'[TOOL]'))
        SET IDENTITY_INSERT [TOOL] ON;
    EXEC(N'INSERT INTO [TOOL] ([tool_id], [name])
    VALUES (''41111111-1111-1111-1111-111111111101'', N''WorxGPT''),
    (''41111111-1111-1111-1111-111111111102'', N''Claude Code''),
    (''41111111-1111-1111-1111-111111111103'', N''GitHub Copilot'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'tool_id', N'name') AND [object_id] = OBJECT_ID(N'[TOOL]'))
        SET IDENTITY_INSERT [TOOL] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'use_case_id', N'name') AND [object_id] = OBJECT_ID(N'[USE_CASE]'))
        SET IDENTITY_INSERT [USE_CASE] ON;
    EXEC(N'INSERT INTO [USE_CASE] ([use_case_id], [name])
    VALUES (''31111111-1111-1111-1111-111111111101'', N''Code generation''),
    (''31111111-1111-1111-1111-111111111102'', N''Code review''),
    (''31111111-1111-1111-1111-111111111103'', N''Testing'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'use_case_id', N'name') AND [object_id] = OBJECT_ID(N'[USE_CASE]'))
        SET IDENTITY_INSERT [USE_CASE] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_asset_type_id] ON [ASSET] ([asset_type_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_author_id] ON [ASSET] ([author_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_domain_id] ON [ASSET] ([domain_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_use_case_id] ON [ASSET] ([use_case_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_TAG_tag_id] ON [ASSET_TAG] ([tag_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_TOOL_tool_id] ON [ASSET_TOOL] ([tool_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ASSET_TYPE_name] ON [ASSET_TYPE] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ASSET_VIEW_asset_id] ON [ASSET_VIEW] ([asset_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AUTH_SESSION_user_id] ON [AUTH_SESSION] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BOOKMARK_asset_id] ON [BOOKMARK] ([asset_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_COMMENT_asset_id] ON [COMMENT] ([asset_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_COMMENT_parent_comment_id] ON [COMMENT] ([parent_comment_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_COMMENT_user_id] ON [COMMENT] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DOMAIN_name] ON [DOMAIN] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NOTIFICATION_actor_user_id] ON [NOTIFICATION] ([actor_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NOTIFICATION_asset_id] ON [NOTIFICATION] ([asset_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NOTIFICATION_comment_id] ON [NOTIFICATION] ([comment_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NOTIFICATION_recipient_user_id] ON [NOTIFICATION] ([recipient_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NOTIFICATION_report_id] ON [NOTIFICATION] ([report_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_REACTION_user_id] ON [REACTION] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_REPORT_reporter_user_id] ON [REPORT] ([reporter_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_REPORT_resolved_by] ON [REPORT] ([resolved_by]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_REPORT_ASSET_asset_id] ON [REPORT_ASSET] ([asset_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_REPORT_COMMENT_comment_id] ON [REPORT_COMMENT] ([comment_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TAG_name] ON [TAG] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TOOL_name] ON [TOOL] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_USE_CASE_name] ON [USE_CASE] ([name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802222825_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802222825_InitialCreate', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804130931_ClearLookupSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260804130931_ClearLookupSeedData', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804224229_ChangeUserGoogleIdToString'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[USER]') AND [c].[name] = N'google_id');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [USER] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [USER] ALTER COLUMN [google_id] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804224229_ChangeUserGoogleIdToString'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260804224229_ChangeUserGoogleIdToString', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806111416_AddAssetTitleIndex'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ASSET]') AND [c].[name] = N'title');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ASSET] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [ASSET] ALTER COLUMN [title] nvarchar(300) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806111416_AddAssetTitleIndex'
)
BEGIN
    CREATE INDEX [IX_ASSET_title] ON [ASSET] ([title]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806111416_AddAssetTitleIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806111416_AddAssetTitleIndex', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807181823_AddAssetSoftDelete'
)
BEGIN
    ALTER TABLE [ASSET] ADD [deleted_at] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807181823_AddAssetSoftDelete'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807181823_AddAssetSoftDelete', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807220900_SyncModelChanges'
)
BEGIN
    IF COL_LENGTH('COMMENT', 'updated_at') IS NULL
    BEGIN
        ALTER TABLE [COMMENT] ADD [updated_at] datetime2 NULL;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807220900_SyncModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807220900_SyncModelChanges', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809125803_RemoveCommentReporting'
)
BEGIN
    DROP TABLE [REPORT_COMMENT];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809125803_RemoveCommentReporting'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260809125803_RemoveCommentReporting', N'10.0.10');
END;

COMMIT;
GO

