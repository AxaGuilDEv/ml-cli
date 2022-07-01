
USE [master]

CREATE DATABASE ecotag
 ON  PRIMARY 
( NAME = N'ecotag_primary_01', FILENAME = N'/var/opt/mssql/data/ecotag_primary_01.mdf' , SIZE = 100 MB , FILEGROWTH = 10MB ),
( NAME = N'fg_dat01', FILENAME = N'/var/opt/mssql/data/ecotag_dat_01.mdf' , SIZE = 100 MB , FILEGROWTH = 10MB ),
( NAME = N'fg_idx01', FILENAME = N'/var/opt/mssql/data/ecotag_idx_01.mdf' , SIZE = 100 MB , FILEGROWTH = 10MB ),
( NAME = N'fg_lob01', FILENAME = N'/var/opt/mssql/data/ecotag_lob_01.mdf' , SIZE = 100 MB , FILEGROWTH = 10MB )
 LOG ON 
( NAME = N'Log_01_log', FILENAME = N'/var/opt/mssql/data/ecotag_log_01.ldf' ,SIZE = 100 MB, MAXSIZE = 500 MB, FILEGROWTH = 50 MB) COLLATE French_CI_AS
GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'ecotag')
BEGIN
        ALTER DATABASE [ecotag]
            SET ANSI_NULLS ON,
                ANSI_PADDING ON,
                ANSI_WARNINGS ON,
                ARITHABORT ON,
                CONCAT_NULL_YIELDS_NULL ON,
                NUMERIC_ROUNDABORT OFF,
                QUOTED_IDENTIFIER ON,
                ANSI_NULL_DEFAULT ON,
                CURSOR_DEFAULT LOCAL,
                RECOVERY FULL,
                CURSOR_CLOSE_ON_COMMIT OFF,
                AUTO_CREATE_STATISTICS ON,
                AUTO_SHRINK OFF,
                AUTO_UPDATE_STATISTICS ON,
                RECURSIVE_TRIGGERS OFF 
            WITH ROLLBACK IMMEDIATE;
        ALTER DATABASE [ecotag]
            SET AUTO_CLOSE OFF 
            WITH ROLLBACK IMMEDIATE;
END


IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'ecotag')
BEGIN
        ALTER DATABASE [ecotag]
            SET READ_COMMITTED_SNAPSHOT OFF;
END



IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'ecotag')
BEGIN
        ALTER DATABASE [ecotag]
            SET AUTO_UPDATE_STATISTICS_ASYNC OFF,
                PAGE_VERIFY NONE,
                DATE_CORRELATION_OPTIMIZATION OFF,
                DISABLE_BROKER,
                PARAMETERIZATION SIMPLE,
                SUPPLEMENTAL_LOGGING OFF 
            WITH ROLLBACK IMMEDIATE;
END

USE ecotag
GO
CREATE SCHEMA sch_ECOTAG AUTHORIZATION [dbo];
GO

/****** Object:  Table [sch_ECOTAG].[T_User] ******/
if not exists (select * from sysobjects where name='T_User' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_User](
    [USR_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [USR_Email] [varchar](254) NOT NULL,
    [USR_NameIdentifier] [varchar](64) NOT NULL,
    [USR_Role] [varchar](128) NULL,
    CONSTRAINT [PK_T_User] PRIMARY KEY NONCLUSTERED ([USR_Id]),
    CONSTRAINT [UK_T_User_Email] UNIQUE([USR_Email]),
    CONSTRAINT [UK_T_User_NameIdentifier] UNIQUE([USR_NameIdentifier])
    ) ON [PRIMARY]
END

GO 

/****** Object:  Table [sch_ECOTAG].[T_Group] ******/
if not exists (select * from sysobjects where name='T_Group' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_Group](
    [GRP_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [GRP_Name] [varchar](48) NOT NULL,
    [GRP_CreatorNameIdentifier] [varchar](64) NOT NULL,
    [GRP_CreateDate] BIGINT NOT NULL,
    [GRP_UpdateDate] BIGINT NOT NULL,
    CONSTRAINT [PK_T_Group] PRIMARY KEY NONCLUSTERED ([GRP_Id]),
    CONSTRAINT [UK_T_Group_Name] UNIQUE ([GRP_Name])
    ) ON [PRIMARY]
END

GO

/****** Object:  Table [sch_ECOTAG].[T_GroupUsers] ******/
if not exists (select * from sysobjects where name='T_GroupUsers' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_GroupUsers](
    [GPU_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [GRP_Id] uniqueidentifier NOT NULL,
    [USR_Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_T_GroupUsers] PRIMARY KEY NONCLUSTERED ([GPU_Id]),
    CONSTRAINT [FK_T_GroupUsers_GRP_Id] FOREIGN KEY (GRP_Id) REFERENCES [sch_ECOTAG].[T_Group] (GRP_Id),
    CONSTRAINT [FK_T_GroupUsers_USR_Id] FOREIGN KEY (USR_Id) REFERENCES [sch_ECOTAG].[T_User] (USR_Id)
    ) ON [PRIMARY]
END

GO

/****** Object:  Table [sch_ECOTAG].[T_Dataset] ******/
if not exists (select * from sysobjects where name='T_Dataset' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_Dataset](
    [DTS_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [DTS_Name] [varchar](48) NOT NULL,
    [DTS_Type] int NOT NULL,
    [DTS_Classification] int NOT NULL,
    [GRP_Id] uniqueidentifier NOT NULL,
    [DTS_CreatorNameIdentifier] [varchar](64) NOT NULL,
    [DTS_CreateDate] BIGINT NOT NULL,
    [DTS_Locked] int NULL,
    [DTS_BlobUri] [varchar](512) NOT NULL,
    CONSTRAINT [PK_T_Dataset] PRIMARY KEY NONCLUSTERED ([DTS_Id]),
    CONSTRAINT [UK_T_Dataset_Name] UNIQUE ([DTS_Name]),
    CONSTRAINT [FK_T_Dataset_GRP_Id] FOREIGN KEY (GRP_Id) REFERENCES [sch_ECOTAG].[T_Group] (GRP_Id)
    ) ON [PRIMARY]
END

GO

/****** Object:  Table [sch_ECOTAG].[T_File] ******/
if not exists (select * from sysobjects where name='T_File' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_File](
    [FLE_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [FLE_Name] [varchar](1024) NOT NULL,
    [FLE_Size] BIGINT NOT NULL,
    [FLE_ContentType] [varchar](256) NOT NULL,
    [FLE_CreatorNameIdentifier] [varchar](64) NOT NULL,
    [FLE_CreateDate] BIGINT NOT NULL,
    [DTS_Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_T_File] PRIMARY KEY NONCLUSTERED ([FLE_Id]),
    CONSTRAINT [FK_T_File_DTS_Id] FOREIGN KEY (DTS_Id) REFERENCES [sch_ECOTAG].[T_Dataset] (DTS_Id)
    ) ON [PRIMARY]
END

GO


/****** Object:  Table [sch_ECOTAG].[T_Project] ******/
if not exists (select * from sysobjects where name='T_Project' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_Project](
    [PRJ_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [DTS_Id] uniqueidentifier NOT NULL,
    [GRP_Id] uniqueidentifier NOT NULL,
    [PRJ_Name] [varchar](48) NOT NULL,
    [PRJ_NumberCrossAnnotation] [int] NOT NULL CHECK (PRJ_NumberCrossAnnotation between 1 and 10),
    [PRJ_CreateDate] BIGINT NOT NULL,
    [PRJ_AnnotationType] int NOT NULL,
    [PRJ_LabelsJson] [varchar](2048) NOT NULL,
    [PRJ_CreatorNameIdentifier] [varchar](64) NOT NULL,
    CONSTRAINT [PK_T_Project] PRIMARY KEY NONCLUSTERED ([PRJ_Id]),
    CONSTRAINT [FK_T_Project_GRP_Id] FOREIGN KEY (GRP_Id) REFERENCES [sch_ECOTAG].[T_Group] (GRP_Id),
    CONSTRAINT [FK_T_Project_DTS_Id] FOREIGN KEY (DTS_Id) REFERENCES [sch_ECOTAG].[T_Dataset] (DTS_Id)
    )
END

GO

/****** Object:  Table [sch_ECOTAG].[T_Reservation] ******/
if not exists (select * from sysobjects where name='T_Reservation' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_Reservation](
    [RSV_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [FLE_Id] uniqueidentifier NOT NULL,
    [PRJ_Id] uniqueidentifier NOT NULL,
    [RSV_TimeStamp] BIGINT NOT NULL,
    CONSTRAINT [PK_T_Reservation] PRIMARY KEY NONCLUSTERED ([RSV_Id]),
    CONSTRAINT [FK_T_Reservation_FLE_Id] FOREIGN KEY (FLE_Id) REFERENCES [sch_ECOTAG].[T_File] (FLE_Id),
    CONSTRAINT [FK_T_Reservation_PRJ_Id] FOREIGN KEY (PRJ_Id) REFERENCES [sch_ECOTAG].[T_Project] (PRJ_Id)
    )
END

GO

/****** Object:  Table [sch_ECOTAG].[T_Annotation] ******/
if not exists (select * from sysobjects where name='T_Annotation' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_Annotation](
    [ANO_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [FLE_Id] uniqueidentifier NOT NULL,
    [PRJ_Id] uniqueidentifier NOT NULL,
    [ANO_CreatorNameIdentifier] [varchar](64) NOT NULL,
    [ANO_TimeStamp] BIGINT NOT NULL,
    [ANO_ExpectedOutput] [varchar](4048) NOT NULL,
    CONSTRAINT [PK_T_Annotation] PRIMARY KEY NONCLUSTERED ([ANO_Id]),
    CONSTRAINT [FK-T_Annotation_FLE_Id] FOREIGN KEY (FLE_Id) REFERENCES [sch_ECOTAG].[T_File] (FLE_Id),
    CONSTRAINT [FK_T_Annotation_PRJ_Id] FOREIGN KEY (PRJ_Id) REFERENCES [sch_ECOTAG].[T_Project] (PRJ_Id)
    )
END

GO

/****** Object:  Table [sch_ECOTAG].[T_Audit] ******/
if not exists (select * from sysobjects where name='T_Audit' and xtype='U')
BEGIN
CREATE TABLE [sch_ECOTAG].[T_Audit](
    [AUD_Id] uniqueidentifier NOT NULL DEFAULT newid(),
    [AUD_ElementId] uniqueidentifier NOT NULL,
    [AUD_Type] [varchar](32) NOT NULL,
    [AUD_NameIdentifier] [varchar](64) NOT NULL,
    [AUD_CreateDate] BIGINT NOT NULL,
    [AUD_Diff] [varchar](4048) NOT NULL,
    CONSTRAINT [PK_T_Audit] PRIMARY KEY NONCLUSTERED ([AUD_Id])
    )
END

GO

CREATE CLUSTERED INDEX [IND_AuditElementIdTypeCreateDate] ON [sch_ECOTAG].[T_Audit]
(
    [AUD_ElementId] ASC,
    [AUD_Type] ASC,
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE INDEX [IND_AnnotationCreatorNameIdentifier] ON [sch_ECOTAG].[T_Annotation]
(
    [ANO_CreatorNameIdentifier] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE CLUSTERED INDEX [IND_AnnotationProjectId] ON [sch_ECOTAG].[T_Annotation]
(
    [PRJ_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE CLUSTERED INDEX [IND_ReservationProjectId] ON [sch_ECOTAG].[T_Reservation]
(
    [PRJ_Id] ASC,
    [RSV_TimeStamp] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE UNIQUE INDEX [IND_FileName_DatasetId] ON [sch_ECOTAG].[T_File]
(
    [FLE_Name], 
    [DTS_Id]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE INDEX [IND_DatasetLocked] ON [sch_ECOTAG].[T_Dataset]
(
    [DTS_Locked] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE CLUSTERED INDEX [IND_UserNameIdentifier] ON [sch_ECOTAG].[T_User]
(
    [USR_NameIdentifier] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE CLUSTERED INDEX [IND_GroupName] ON [sch_ECOTAG].[T_Group]
(
    [GRP_Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

USE [ecotag]
GO
