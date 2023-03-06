﻿----
---- ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
----

-- ===========================================================================================
--   Description		:	Création de la table UTILISATEUR.
-- ===========================================================================================

create table [dbo].[UTILISATEUR] (
	[UTI_ID] int8 identity,
	[UTI_AGE] numeric,
	[PRO_ID] int8,
	[UTI_EMAIL] varchar,
	[UTI_NOM] varchar,
	[UTI_ACTIF] boolean,
	[TUT_CODE] varchar,
	[UTI_ID_PARENT] int8,
	[UTI_DATE_CREATION] date,
	[UTI_DATE_MODIFICATION] date,
	constraint [PK_UTILISATEUR] primary key clustered ([UTI_ID] ASC),
	constraint [FK_UTILISATEUR_PROFIL_PRO_ID] foreign key ([PRO_ID]) references [dbo].[PROFIL] ([PRO_ID]),
	constraint [FK_UTILISATEUR_TYPE_UTILISATEUR_TUT_CODE] foreign key ([TUT_CODE]) references [dbo].[TYPE_UTILISATEUR] ([TUT_CODE]),
	constraint [FK_UTILISATEUR_UTILISATEUR_UTI_ID_PARENT] foreign key ([UTI_ID_PARENT]) references [dbo].[UTILISATEUR] ([UTI_ID]),
	constraint [UK_UTILISATEUR_UTI_EMAIL_UTI_ID_PARENT] unique nonclustered ([UTI_EMAIL] ASC, [UTI_ID_PARENT] ASC),
	constraint [UK_UTILISATEUR_UTI_ID_PARENT] unique nonclustered ([UTI_ID_PARENT] ASC))
go

/* Index on foreign key column for UTILISATEUR.PRO_ID */
create nonclustered index [IDX_UTILISATEUR_PRO_ID_FK]
	on [dbo].[UTILISATEUR] ([PRO_ID] ASC)
go

/* Index on foreign key column for UTILISATEUR.TUT_CODE */
create nonclustered index [IDX_UTILISATEUR_TUT_CODE_FK]
	on [dbo].[UTILISATEUR] ([TUT_CODE] ASC)
go

/* Index on foreign key column for UTILISATEUR.UTI_ID_PARENT */
create nonclustered index [IDX_UTILISATEUR_UTI_ID_PARENT_FK]
	on [dbo].[UTILISATEUR] ([UTI_ID_PARENT] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'Utilisateur', 'SCHEMA', 'dbo', 'TABLE', 'UTILISATEUR';
