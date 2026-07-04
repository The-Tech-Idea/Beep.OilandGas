-- ============================================================================
-- Phase 1 — RBAC Hardening: Extension Tables
-- Created: 2026-07-02
-- Module: SecurityModule (Order 40)
-- ============================================================================

-- 1. PERSONA_ROLE — bridges Persona (UI/UX) to Role (API authorization)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PERSONA_ROLE')
BEGIN
    CREATE TABLE PERSONA_ROLE (
        PERSONA_ROLE_ID     NVARCHAR(128)    NOT NULL PRIMARY KEY,
        PERSONA_ID          NVARCHAR(128)    NOT NULL,
        PERSONA_CODE        NVARCHAR(64)     NOT NULL,
        ROLE_ID             NVARCHAR(128)    NOT NULL,
        ROLE_NAME           NVARCHAR(128)    NOT NULL,
        IS_PRIMARY          NVARCHAR(1)      NOT NULL DEFAULT 'N',
        PRIORITY            INT              NOT NULL DEFAULT 1,
        EFFECTIVE_SCOPE     NVARCHAR(32)     NULL,           -- GLOBAL, FIELD, ASSET

        -- ModelEntityBase audit columns
        ACTIVE_IND          NVARCHAR(1)      NOT NULL DEFAULT 'Y',
        ROW_CREATED_BY      NVARCHAR(128)    NOT NULL DEFAULT 'SYSTEM',
        ROW_CREATED_DATE    DATETIME         NULL,
        ROW_CHANGED_BY      NVARCHAR(128)    NOT NULL DEFAULT 'SYSTEM',
        ROW_CHANGED_DATE    DATETIME         NULL,
        ROW_EFFECTIVE_DATE  DATETIME         NULL,
        ROW_EXPIRY_DATE     DATETIME         NULL,
        ROW_QUALITY         NVARCHAR(MAX)    NULL,
        PPDM_GUID           NVARCHAR(128)    NULL,
        EXPIRY_DATE         DATETIME         NULL,
        EFFECTIVE_DATE      DATETIME         NULL,
        REMARK              NVARCHAR(MAX)    NULL,
        SOURCE              NVARCHAR(256)    NULL
    );

    CREATE INDEX IX_PERSONA_ROLE_CODE ON PERSONA_ROLE(PERSONA_CODE);
    CREATE INDEX IX_PERSONA_ROLE_ROLE_NAME ON PERSONA_ROLE(ROLE_NAME);
    CREATE UNIQUE INDEX IX_PERSONA_ROLE_UK ON PERSONA_ROLE(PERSONA_CODE, ROLE_NAME);
END
GO

-- 2. ROLE_HIERARCHY — parent-child role inheritance
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ROLE_HIERARCHY')
BEGIN
    CREATE TABLE ROLE_HIERARCHY (
        ROLE_HIERARCHY_ID   NVARCHAR(128)    NOT NULL PRIMARY KEY,
        PARENT_ROLE_ID      NVARCHAR(128)    NOT NULL,
        PARENT_ROLE_NAME    NVARCHAR(128)    NOT NULL,
        CHILD_ROLE_ID       NVARCHAR(128)    NOT NULL,
        CHILD_ROLE_NAME     NVARCHAR(128)    NOT NULL,
        INHERITANCE_TYPE    NVARCHAR(32)     NOT NULL DEFAULT 'FULL',  -- FULL, SELECTIVE, DENY
        DOMAIN_FILTER       NVARCHAR(MAX)    NULL,                     -- comma-separated domain prefixes for SELECTIVE
        PRIORITY            INT              NOT NULL DEFAULT 1,

        -- ModelEntityBase audit columns
        ACTIVE_IND          NVARCHAR(1)      NOT NULL DEFAULT 'Y',
        ROW_CREATED_BY      NVARCHAR(128)    NOT NULL DEFAULT 'SYSTEM',
        ROW_CREATED_DATE    DATETIME         NULL,
        ROW_CHANGED_BY      NVARCHAR(128)    NOT NULL DEFAULT 'SYSTEM',
        ROW_CHANGED_DATE    DATETIME         NULL,
        ROW_EFFECTIVE_DATE  DATETIME         NULL,
        ROW_EXPIRY_DATE     DATETIME         NULL,
        ROW_QUALITY         NVARCHAR(MAX)    NULL,
        PPDM_GUID           NVARCHAR(128)    NULL,
        EXPIRY_DATE         DATETIME         NULL,
        EFFECTIVE_DATE      DATETIME         NULL,
        REMARK              NVARCHAR(MAX)    NULL,
        SOURCE              NVARCHAR(256)    NULL
    );

    CREATE UNIQUE INDEX IX_ROLE_HIERARCHY_UK ON ROLE_HIERARCHY(PARENT_ROLE_NAME, CHILD_ROLE_NAME);
END
GO

-- 3. TEMP_ROLE_ELEVATION — time-bound temporary role elevations
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TEMP_ROLE_ELEVATION')
BEGIN
    CREATE TABLE TEMP_ROLE_ELEVATION (
        ELEVATION_ID                NVARCHAR(128)    NOT NULL PRIMARY KEY,
        USER_ID                     NVARCHAR(128)    NOT NULL,
        USER_NAME                   NVARCHAR(256)    NULL,
        ELEVATED_ROLE_ID            NVARCHAR(128)    NOT NULL,
        ELEVATED_ROLE_NAME          NVARCHAR(128)    NOT NULL,
        BASE_ROLE_ID                NVARCHAR(128)    NULL,
        EFFECTIVE_FROM              DATETIME         NOT NULL,
        EFFECTIVE_TO                DATETIME         NOT NULL,
        REASON                      NVARCHAR(MAX)    NOT NULL,
        REQUESTED_BY                NVARCHAR(128)    NULL,
        STATUS                      NVARCHAR(32)     NOT NULL DEFAULT 'PENDING',  -- PENDING, ACTIVE, EXPIRED, REVOKED, REJECTED
        SCOPE_LIMITATION            NVARCHAR(MAX)    NULL,
        REVOKED_AT                  DATETIME         NULL,
        REVOKED_BY                  NVARCHAR(128)    NULL,
        REVOKED_REASON              NVARCHAR(MAX)    NULL,
        APPROVAL_PROCESS_INSTANCE_ID NVARCHAR(128)   NULL,

        -- ModelEntityBase audit columns
        ACTIVE_IND          NVARCHAR(1)      NOT NULL DEFAULT 'Y',
        ROW_CREATED_BY      NVARCHAR(128)    NOT NULL DEFAULT 'SYSTEM',
        ROW_CREATED_DATE    DATETIME         NULL,
        ROW_CHANGED_BY      NVARCHAR(128)    NOT NULL DEFAULT 'SYSTEM',
        ROW_CHANGED_DATE    DATETIME         NULL,
        ROW_EFFECTIVE_DATE  DATETIME         NULL,
        ROW_EXPIRY_DATE     DATETIME         NULL,
        ROW_QUALITY         NVARCHAR(MAX)    NULL,
        PPDM_GUID           NVARCHAR(128)    NULL,
        EXPIRY_DATE         DATETIME         NULL,
        EFFECTIVE_DATE      DATETIME         NULL,
        REMARK              NVARCHAR(MAX)    NULL,
        SOURCE              NVARCHAR(256)    NULL
    );

    CREATE INDEX IX_TEMP_ROLE_ELEVATION_USER ON TEMP_ROLE_ELEVATION(USER_ID, STATUS);
    CREATE INDEX IX_TEMP_ROLE_ELEVATION_EXPIRY ON TEMP_ROLE_ELEVATION(EFFECTIVE_TO, STATUS);
END
GO
