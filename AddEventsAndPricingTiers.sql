-- Migration to add Events, Venues, and PricingTiers tables
-- This resolves the 'Invalid column name TotalCapacity' error

-- Create Venues table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Venues' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Venues] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Name] varchar(200) NOT NULL,
        [Address] varchar(500) NOT NULL,
        [Capacity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0
    );
END

-- Create Events table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Events' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Events] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Title] varchar(200) NOT NULL,
        [Description] text NOT NULL,
        [OrganizerId] uniqueidentifier NOT NULL,
        [VenueId] uniqueidentifier NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Visibility] nvarchar(20) NOT NULL,
        [TotalCapacity] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Events_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id])
    );
    
    CREATE INDEX [IX_Events_VenueId] ON [Events] ([VenueId]);
END

-- Create PricingTiers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PricingTiers' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PricingTiers] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [EventId] uniqueidentifier NOT NULL,
        [Name] varchar(100) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Currency] varchar(3) NOT NULL,
        [Capacity] int NOT NULL,
        [AvailableCapacity] int NOT NULL,
        [SaleStartDate] datetime2 NOT NULL,
        [SaleEndDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] int NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [FK_PricingTiers_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_PricingTiers_EventId] ON [PricingTiers] ([EventId]);
END

-- Insert the migration record
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20241225120000_AddEventsAndPricingTiers')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
    VALUES ('20241225120000_AddEventsAndPricingTiers', '8.0.5');
END

PRINT 'Migration completed successfully: Added Events, Venues, and PricingTiers tables';