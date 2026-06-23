/*
    Table: dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY
    Purpose:
      Stores AI-generated recruitment candidate summaries after ATS scoring.

    Flow:
      1. ATS score is generated.
      2. Test result and interviewer feedback are collected when available.
      3. AI summary is generated from ATS + test + optional feedback.
      4. Generated result is saved here and later fetched by API for UI display.
*/

IF OBJECT_ID('dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY
    (
        AI_SUMMARY_ID              BIGINT IDENTITY(1,1) NOT NULL,
        TENANT_ID                  UNIQUEIDENTIFIER     NULL,
        COMPANY_ID                 BIGINT               NULL,
        REFERENCE_NO               NVARCHAR(50)         NOT NULL,
        CANDIDATE_ID               BIGINT               NULL,
        CANDIDATE_NAME             NVARCHAR(200)        NULL,
        APPLIED_POST_ID            BIGINT               NULL,
        APPLIED_POST_NAME          NVARCHAR(200)        NULL,
        DEPARTMENT_ID              BIGINT               NULL,
        DEPARTMENT_NAME            NVARCHAR(200)        NULL,

        ATS_TOTAL_SCORE            DECIMAL(6,2)         NULL,
        ATS_STATUS                 NVARCHAR(50)         NULL,
        ATS_PARAMETER_JSON         NVARCHAR(MAX)        NULL,
        TEST_RESULT_JSON           NVARCHAR(MAX)        NULL,
        INTERVIEW_FEEDBACK_JSON    NVARCHAR(MAX)        NULL,
        HAS_INTERVIEW_FEEDBACK     BIT                  NOT NULL CONSTRAINT DF_TRCAS_HAS_INTERVIEW_FEEDBACK DEFAULT (0),

        AI_RECOMMENDATION          NVARCHAR(50)         NULL,
        MATCH_PERCENTAGE           DECIMAL(6,2)         NULL,
        KEY_STRENGTHS              NVARCHAR(MAX)        NULL,
        RISK_INDICATORS            NVARCHAR(MAX)        NULL,
        FINAL_SUGGESTION           NVARCHAR(MAX)        NULL,
        AI_SUMMARY                 NVARCHAR(MAX)        NULL,
        AI_MODEL_NAME              NVARCHAR(100)        NULL,
        AI_PROMPT_VERSION          NVARCHAR(50)         NULL,

        IS_LATEST                  BIT                  NOT NULL CONSTRAINT DF_TRCAS_IS_LATEST DEFAULT (1),
        GENERATED_ON               DATETIME2(0)         NOT NULL CONSTRAINT DF_TRCAS_GENERATED_ON DEFAULT (SYSUTCDATETIME()),
        GENERATED_BY               NVARCHAR(100)        NULL,
        CREATED_ON                 DATETIME2(0)         NOT NULL CONSTRAINT DF_TRCAS_CREATED_ON DEFAULT (SYSUTCDATETIME()),
        CREATED_BY                 NVARCHAR(100)        NULL,
        UPDATED_ON                 DATETIME2(0)         NULL,
        UPDATED_BY                 NVARCHAR(100)        NULL,

        CONSTRAINT PK_TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY
            PRIMARY KEY CLUSTERED (AI_SUMMARY_ID)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TRCAS_REFERENCE_NO_IS_LATEST'
      AND object_id = OBJECT_ID('dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TRCAS_REFERENCE_NO_IS_LATEST
    ON dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY (REFERENCE_NO, IS_LATEST)
    INCLUDE (AI_RECOMMENDATION, MATCH_PERCENTAGE, ATS_TOTAL_SCORE, ATS_STATUS, GENERATED_ON);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TRCAS_CANDIDATE_ID'
      AND object_id = OBJECT_ID('dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TRCAS_CANDIDATE_ID
    ON dbo.TBL_RECRUITMENT_CANDIDATE_AI_SUMMARY (CANDIDATE_ID, IS_LATEST);
END;
GO
