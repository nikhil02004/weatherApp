-- =============================================================
-- Auth.Service  –  Database bootstrap & Stored Procedures
-- Run once against your SQL Server database.
-- =============================================================

-- ---------------------------------------------------------------
-- 1. Table: dbo.Users
-- ---------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'dbo.Users') AND type = N'U'
)
BEGIN
    CREATE TABLE dbo.Users
    (
        Id               NVARCHAR(450)  NOT NULL,
        Username         NVARCHAR(256)  NOT NULL,
        PasswordHash     NVARCHAR(MAX)  NOT NULL,
        Email            NVARCHAR(256)  NULL,
        ExternalProvider NVARCHAR(50)   NULL,
        ExternalId       NVARCHAR(256)  NULL,
        CreatedAt        DATETIME2(7)   NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Users          PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Users_Username UNIQUE (Username)
    );

    SET QUOTED_IDENTIFIER ON;
    EXEC('CREATE UNIQUE INDEX UQ_Users_Email ON dbo.Users (Email) WHERE Email IS NOT NULL');
END;
ELSE
BEGIN
    -- Add new columns to existing table if they don't exist
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'Email')
        ALTER TABLE dbo.Users ADD Email NVARCHAR(256) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'ExternalProvider')
        ALTER TABLE dbo.Users ADD ExternalProvider NVARCHAR(50) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'ExternalId')
        ALTER TABLE dbo.Users ADD ExternalId NVARCHAR(256) NULL;

    -- Add unique index on Email if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'UQ_Users_Email')
    BEGIN
        SET QUOTED_IDENTIFIER ON;
        EXEC('CREATE UNIQUE INDEX UQ_Users_Email ON dbo.Users (Email) WHERE Email IS NOT NULL');
    END;
END;
GO

-- ---------------------------------------------------------------
-- 2. sp_GetUserByUsername
--    Returns the single row whose Username matches (case-insensitive
--    collation is handled by the column / DB default).
-- ---------------------------------------------------------------
IF OBJECT_ID(N'dbo.sp_GetUserByUsername', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByUsername;
GO

CREATE PROCEDURE dbo.sp_GetUserByUsername
    @Username NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id,
           Username,
           PasswordHash,
           Email,
           ExternalProvider,
           ExternalId
    FROM   dbo.Users
    WHERE  Username = @Username;
END;
GO

-- ---------------------------------------------------------------
-- sp_GetUserByEmail
--    Returns the single user row whose Email matches.
-- ---------------------------------------------------------------
IF OBJECT_ID(N'dbo.sp_GetUserByEmail', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByEmail;
GO

CREATE PROCEDURE dbo.sp_GetUserByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id,
           Username,
           PasswordHash,
           Email,
           ExternalProvider,
           ExternalId
    FROM   dbo.Users
    WHERE  Email = @Email;
END;
GO

-- ---------------------------------------------------------------
-- sp_GetUserByExternalId
--    Returns user matched by provider + external subject ID.
-- ---------------------------------------------------------------
IF OBJECT_ID(N'dbo.sp_GetUserByExternalId', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserByExternalId;
GO

CREATE PROCEDURE dbo.sp_GetUserByExternalId
    @Provider   NVARCHAR(50),
    @ExternalId NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id,
           Username,
           PasswordHash,
           Email,
           ExternalProvider,
           ExternalId
    FROM   dbo.Users
    WHERE  ExternalProvider = @Provider
      AND  ExternalId       = @ExternalId;
END;
GO

-- ---------------------------------------------------------------
-- 3. sp_CreateUser
--    Inserts a new user.  Returns 1 on success, 0 if the username
--    already exists (duplicate key), -1 on any other error.
-- ---------------------------------------------------------------
IF OBJECT_ID(N'dbo.sp_CreateUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateUser;
GO

CREATE PROCEDURE dbo.sp_CreateUser
    @Id              NVARCHAR(450),
    @Username        NVARCHAR(256),
    @PasswordHash    NVARCHAR(MAX),
    @Email           NVARCHAR(256) = NULL,
    @ExternalProvider NVARCHAR(50) = NULL,
    @ExternalId      NVARCHAR(256) = NULL,
    @RowsAffected    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO dbo.Users (Id, Username, PasswordHash, Email, ExternalProvider, ExternalId)
        VALUES (@Id, @Username, @PasswordHash, @Email, @ExternalProvider, @ExternalId);

        SET @RowsAffected = @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        -- Duplicate username (unique constraint violation)
        IF ERROR_NUMBER() IN (2601, 2627)
            SET @RowsAffected = 0;
        ELSE
        BEGIN
            SET @RowsAffected = -1;
            THROW;   -- re-raise unexpected errors to the caller
        END
    END CATCH
END;
GO
