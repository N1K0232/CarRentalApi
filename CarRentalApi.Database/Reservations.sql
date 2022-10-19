﻿CREATE TABLE [dbo].[Reservations]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
	[PersonId] UNIQUEIDENTIFIER NOT NULL,
	[VehicleId] UNIQUEIDENTIFIER NOT NULL,
	[ReservationStart] DATETIME NOT NULL,
	[ReservationEnd] DATETIME NOT NULL,
	[CreationDate] DATETIME NOT NULL,
	[UpdatedDate] DATETIME NULL,
	[IsDeleted] BIT NOT NULL,
	[DeletedDate] DATETIME NULL,

	PRIMARY KEY(Id)
)