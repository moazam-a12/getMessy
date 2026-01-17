-- ============================================
-- GetMessy - MSSQL Database Setup Script
-- For deployment on smarterasp.net
-- ============================================

-- Create Users Table
CREATE TABLE [Users] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [FullName] NVARCHAR(MAX) NOT NULL,
    [Email] NVARCHAR(MAX) NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Role] NVARCHAR(MAX) NOT NULL,
    [Drink] BIT NOT NULL,
    [Lunch] BIT NOT NULL
);

-- Create Menus Table
CREATE TABLE [Menus] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(MAX) NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [Date] DATETIME NOT NULL,
    [IsFood] BIT NOT NULL
);

-- Create Bills Table
CREATE TABLE [Bills] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [Date] DATETIME NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Paid] BIT NOT NULL,
    CONSTRAINT [FK_Bills_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

-- Create Attendances Table
CREATE TABLE [Attendances] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [MenuId] INT NOT NULL,
    [Attended] BIT NOT NULL,
    CONSTRAINT [FK_Attendances_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Attendances_Menus_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menus]([Id]) ON DELETE CASCADE
);

-- Create Indexes for Foreign Keys
CREATE INDEX [IX_Bills_UserId] ON [Bills] ([UserId]);
CREATE INDEX [IX_Attendances_UserId] ON [Attendances] ([UserId]);
CREATE INDEX [IX_Attendances_MenuId] ON [Attendances] ([MenuId]);

-- ============================================
-- Optional: Sample data for testing
-- ============================================

-- Uncomment these to insert sample data

-- INSERT INTO [Users] (FullName, Email, PasswordHash, Role, Drink, Lunch)
-- VALUES ('Admin User', 'admin@example.com', 'hashed_password', 'Admin', 1, 1);

-- INSERT INTO [Menus] (Name, Price, Date, IsFood)
-- VALUES ('Chicken Biryani', 250.00, GETDATE(), 1);

-- INSERT INTO [Bills] (UserId, Date, Amount, Paid)
-- VALUES (1, GETDATE(), 1500.00, 0);

-- INSERT INTO [Attendances] (UserId, MenuId, Attended)
-- VALUES (1, 1, 1);
