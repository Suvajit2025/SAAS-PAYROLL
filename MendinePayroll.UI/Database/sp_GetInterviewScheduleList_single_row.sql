CREATE OR ALTER PROCEDURE dbo.sp_GetInterviewScheduleList
(
    @StatusCode VARCHAR(50) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CandidateName VARCHAR(200) = NULL,
    @InterviewerEmpId INT = NULL,
    @TenantID NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.id AS InterviewScheduleId,
        m.candidate_id,
        m.candidate_name,
        m.candidate_email,
        m.post,
        m.department,
        m.interview_round,
        m.interview_deadline,
        m.interview_date,
        m.meetLink AS meet_link,
        m.notes,
        scheduledBy.notes AS interviewer_notes,
        m.interview_mode,
        s.status_name AS status,
        m.assigned_on,
        m.updated_on,
        scheduledBy.confirmed_slot_from,
        scheduledBy.confirmed_slot_to,
        m.isFeedbackSubmitted,
        feedback.Recommendation,
        assigned.Interviewers,
        (
            SELECT
                ISNULL(ps.interview_date, TRY_CONVERT(DATE, LEFT(ps.slot_from, 10))) AS [Date],
                (
                    SELECT
                        ps2.id AS [Id],
                        ps2.slot_from AS [SlotFrom],
                        ps2.slot_to AS [SlotTo]
                    FROM dbo.InterviewScheduleProposedSlots ps2
                    CROSS APPLY
                    (
                        SELECT CASE
                            WHEN ps2.interview_date IS NOT NULL
                                 AND TRY_CONVERT(TIME(0), LTRIM(RTRIM(ps2.slot_to))) IS NOT NULL
                                THEN DATEADD(
                                    SECOND,
                                    DATEDIFF(
                                        SECOND,
                                        CAST('00:00:00' AS TIME),
                                        TRY_CONVERT(TIME(0), LTRIM(RTRIM(ps2.slot_to)))
                                    ),
                                    CAST(CAST(ps2.interview_date AS DATE) AS DATETIME2)
                                )
                            WHEN TRY_CONVERT(DATETIME2, ps2.slot_to) >= CAST('2000-01-01' AS DATETIME2)
                                THEN TRY_CONVERT(DATETIME2, ps2.slot_to)
                            ELSE NULL
                        END AS SlotEndAt
                    ) parsed2
                    WHERE ps2.interview_schedule_id = ps.interview_schedule_id
                      AND ps2.interviewer_emp_no = ps.interviewer_emp_no
                      AND (
                            ps2.interview_date = ps.interview_date
                            OR (ps2.interview_date IS NULL AND ps.interview_date IS NULL)
                          )
                      AND ps2.TenantId = @TenantID
                      AND ISNULL(ps2.is_overdue, 0) = 0
                      AND parsed2.SlotEndAt IS NOT NULL
                      AND parsed2.SlotEndAt >= GETDATE()
                    FOR JSON PATH, INCLUDE_NULL_VALUES
                ) AS [Slots]
            FROM dbo.InterviewScheduleProposedSlots ps
            CROSS APPLY
            (
                SELECT CASE
                    WHEN ps.interview_date IS NOT NULL
                         AND TRY_CONVERT(TIME(0), LTRIM(RTRIM(ps.slot_to))) IS NOT NULL
                        THEN DATEADD(
                            SECOND,
                            DATEDIFF(
                                SECOND,
                                CAST('00:00:00' AS TIME),
                                TRY_CONVERT(TIME(0), LTRIM(RTRIM(ps.slot_to)))
                            ),
                            CAST(CAST(ps.interview_date AS DATE) AS DATETIME2)
                        )
                    WHEN TRY_CONVERT(DATETIME2, ps.slot_to) >= CAST('2000-01-01' AS DATETIME2)
                        THEN TRY_CONVERT(DATETIME2, ps.slot_to)
                    ELSE NULL
                END AS SlotEndAt
            ) parsed
            WHERE ps.interview_schedule_id = m.id
              AND ps.TenantId = @TenantID
              AND ISNULL(ps.is_overdue, 0) = 0
              AND parsed.SlotEndAt IS NOT NULL
              AND parsed.SlotEndAt >= GETDATE()
            GROUP BY
                ps.interview_schedule_id,
                ps.interviewer_emp_no,
                ISNULL(ps.interview_date, TRY_CONVERT(DATE, LEFT(ps.slot_from, 10))),
                ps.interview_date
            FOR JSON PATH, INCLUDE_NULL_VALUES
        ) AS ProposedSlotsJSON
    FROM dbo.InterviewScheduleMaster m
    INNER JOIN dbo.InterviewStatusMaster s
        ON m.status_id = s.id

    -- Return every assigned interviewer in one comma-separated value.
    OUTER APPLY
    (
        SELECT STRING_AGG(names.InterviewerName, ', ')
               WITHIN GROUP (ORDER BY names.InterviewerName) AS Interviewers
        FROM
        (
            SELECT DISTINCT
                CONCAT_WS(
                    ' ',
                    emp.empfirstname,
                    NULLIF(emp.empmiddlename, ''),
                    emp.emplastname
                ) AS InterviewerName
            FROM dbo.InterviewScheduleInterviewerMap interviewerMap
            LEFT JOIN Essp.dbo.Empbasic emp
                ON interviewerMap.interviewer_emp_no = emp.empno
            WHERE interviewerMap.interview_schedule_id = m.id
              AND interviewerMap.TenantId = @TenantID
        ) names
    ) assigned

    -- Only one interviewer needs to confirm the shared interview slot.
    -- Prefer the map row containing confirmed times, then one containing notes.
    OUTER APPLY
    (
        SELECT TOP (1)
            interviewerMap.confirmed_slot_from,
            interviewerMap.confirmed_slot_to,
            interviewerMap.notes
        FROM dbo.InterviewScheduleInterviewerMap interviewerMap
        WHERE interviewerMap.interview_schedule_id = m.id
          AND interviewerMap.TenantId = @TenantID
        ORDER BY
            CASE
                WHEN interviewerMap.confirmed_slot_from IS NOT NULL
                  OR interviewerMap.confirmed_slot_to IS NOT NULL THEN 0
                WHEN NULLIF(LTRIM(RTRIM(interviewerMap.notes)), '') IS NOT NULL THEN 1
                ELSE 2
            END,
            interviewerMap.interviewer_emp_no
    ) scheduledBy

    -- Collapse feedback defensively so it cannot duplicate a schedule row.
    OUTER APPLY
    (
        SELECT MAX(f.Recommendation) AS Recommendation
        FROM dbo.InterviewFeedback f
        WHERE f.InterviewScheduleId = m.id
          AND f.TenantId = m.TenantId
          AND f.TenantId = @TenantID
    ) feedback

    WHERE m.TenantId = @TenantID
      AND (@StatusCode IS NULL OR s.status_code = @StatusCode)
      AND (@FromDate IS NULL OR m.interview_deadline >= @FromDate)
      AND (@ToDate IS NULL OR m.interview_deadline <= @ToDate)
      AND (@CandidateName IS NULL OR m.candidate_name LIKE '%' + @CandidateName + '%')
      AND
      (
          @InterviewerEmpId IS NULL
          OR EXISTS
          (
              SELECT 1
              FROM dbo.InterviewScheduleInterviewerMap interviewerFilter
              WHERE interviewerFilter.interview_schedule_id = m.id
                AND interviewerFilter.TenantId = @TenantID
                AND interviewerFilter.interviewer_emp_no = @InterviewerEmpId
          )
      )
    ORDER BY m.assigned_on DESC;
END;
