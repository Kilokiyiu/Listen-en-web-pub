-- 修复 ListenService：试卷/答案 PDF 字段（若 EF 迁移未自动执行可手动运行）
USE Listen_En_Web_listen;
GO

IF COL_LENGTH('T_Album', 'PaperFileUrl') IS NULL
BEGIN
    ALTER TABLE T_Album ADD PaperFileUrl NVARCHAR(1000) NULL;
END
GO

IF COL_LENGTH('T_Album', 'AnswerFileUrl') IS NULL
BEGIN
    ALTER TABLE T_Album ADD AnswerFileUrl NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120000_AddAlbumDocumentUrls'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517120000_AddAlbumDocumentUrls', N'10.0.7');
END
GO
