CREATE OR ALTER PROCEDURE dbo.SP_Saas_MailConfiguration_GetByPurpose
(
    @TenantId UNIQUEIDENTIFIER,
    @MailPurpose VARCHAR(100),
    @ProfileName VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        MD.Id,
        MD.ConfigId,
        MD.TenantId,
        MD.MailPurpose,
        MD.MailSubject,
        MD.MailBody,
        MC.ProfileName,
        MC.Recipients,
        MC.URL,
        MC.FromEmail,
        MC.UserName,
        MC.AppPassword,
        MC.Password,
        MC.Smtp_Port,
        MC.Smtp_Host,
        MC.EnableSsl
    FROM dbo.tbl_Saas_Mail_Details MD
    INNER JOIN dbo.tbl_Saas_Mail_Configuration MC
        ON MC.TenantId = MD.TenantId
       AND MC.ConfigId = MD.ConfigId
    WHERE MD.MailPurpose = @MailPurpose
      AND MC.ProfileName = @ProfileName
      AND MD.IsActive = 1
      AND MC.IsActive = 1
      AND MC.TenantId = @TenantId
    ORDER BY MD.Id DESC;
END;
GO
