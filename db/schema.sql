USE master;
GO

IF DB_ID('DB_Morozov_FoodSubscription') IS NOT NULL
BEGIN
    ALTER DATABASE DB_Morozov_FoodSubscription SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DB_Morozov_FoodSubscription;
END
GO

CREATE DATABASE DB_Morozov_FoodSubscription;
GO

USE DB_Morozov_FoodSubscription;
GO

CREATE TABLE [User] (
    [ID]       INT            IDENTITY(1,1) NOT NULL,
    [Login]    NVARCHAR(50)   NOT NULL,
    [Password] NVARCHAR(100)  NOT NULL,
    [Role]     NVARCHAR(20)   NOT NULL,
    [FIO]      NVARCHAR(150)  NOT NULL,
    [Photo]    NVARCHAR(255)  NULL,
    CONSTRAINT [PK_User]     PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_User_Login] UNIQUE ([Login]),
    CONSTRAINT [CK_User_Role] CHECK ([Role] IN (N'Администратор', N'Пользователь'))
);
GO

CREATE TABLE [Category] (
    [ID]   INT           IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_Category]      PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_Category_Name] UNIQUE ([Name])
);
GO

CREATE TABLE [Subscription] (
    [ID]         INT            IDENTITY(1,1) NOT NULL,
    [CategoryID] INT            NOT NULL,
    [UserID]     INT            NOT NULL,
    [Date]       DATE           NOT NULL,
    [Name]       NVARCHAR(150)  NOT NULL,
    [Num]        INT            NOT NULL,
    [Price]      DECIMAL(10, 2) NOT NULL,
    CONSTRAINT [PK_Subscription] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Subscription_Category] FOREIGN KEY ([CategoryID])
        REFERENCES [Category]([ID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Subscription_User] FOREIGN KEY ([UserID])
        REFERENCES [User]([ID])     ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [CK_Subscription_Num]   CHECK ([Num]   BETWEEN 1 AND 100),
    CONSTRAINT [CK_Subscription_Price] CHECK ([Price] BETWEEN 0 AND 100000)
);
GO

INSERT INTO [User] ([Login], [Password], [Role], [FIO], [Photo]) VALUES
(N'admin',   N'JsdJquTnSd5W1DyHLPnsDry/kECFNDGwi8E05xP+JuE=', N'Администратор', N'Морозов Александр Владимирович', N'user1.png'),
(N'morozov', N'vFhI8ifMFh619o3+mMsTEQqchDzmnpU6iBB9hlWD05c=', N'Пользователь',  N'Морозова Анна Сергеевна',        N'user2.png'),
(N'petrov',  N'vFhI8ifMFh619o3+mMsTEQqchDzmnpU6iBB9hlWD05c=', N'Пользователь',  N'Петров Илья Дмитриевич',        N'user3.png'),
(N'sidorova',N'vFhI8ifMFh619o3+mMsTEQqchDzmnpU6iBB9hlWD05c=', N'Пользователь',  N'Сидорова Мария Павловна',       N'user4.png'),
(N'manager', N'JsdJquTnSd5W1DyHLPnsDry/kECFNDGwi8E05xP+JuE=', N'Администратор', N'Козлов Никита Андреевич',       N'user5.png');
GO

INSERT INTO [Category] ([Name]) VALUES
(N'Овощи и фрукты'),
(N'Молочные продукты'),
(N'Мясо и рыба'),
(N'Бакалея и крупы'),
(N'Готовые рационы'),
(N'Выпечка и десерты');
GO

INSERT INTO [Subscription] ([CategoryID], [UserID], [Date], [Name], [Num], [Price]) VALUES
(1, 2, '2026-09-01', N'Бокс «Фермерские овощи», 5 кг',        4,  1290.00),
(2, 2, '2026-09-03', N'Бокс «Молочная ферма», неделя',        4,   980.50),
(3, 2, '2026-09-07', N'Бокс «Мясо и птица», 3 кг',            2,  2450.00),
(5, 2, '2026-09-12', N'Рацион «Сбалансированный», 7 дней',    1,  6900.00),
(1, 3, '2026-09-02', N'Бокс «Сезонные фрукты», 4 кг',         3,  1150.00),
(4, 3, '2026-09-09', N'Набор круп и бакалеи, месяц',          1,  2150.75),
(6, 3, '2026-09-15', N'Бокс «Домашняя выпечка»',              2,   740.00),
(3, 3, '2026-09-21', N'Бокс «Рыба и морепродукты», 2 кг',     2,  3180.00),
(2, 4, '2026-09-04', N'Бокс «Сыры и йогурты»',                3,  1340.00),
(5, 4, '2026-09-11', N'Рацион «Спортивный», 14 дней',         1, 11900.00),
(1, 4, '2026-09-18', N'Бокс «Зелень и салаты», 2 кг',         5,   620.00),
(4, 4, '2026-09-25', N'Набор «Макароны и соусы»',             2,   890.90),
(6, 5, '2026-09-05', N'Бокс «Десерты недели»',                2,   1020.00),
(3, 5, '2026-09-14', N'Бокс «Премиум мясо», 2 кг',            1,   4150.00),
(5, 5, '2026-09-22', N'Рацион «Вегетарианский», 7 дней',      1,  5890.00),
(2, 1, '2026-09-06', N'Бокс «Молочная ферма», неделя',        1,   980.50),
(1, 1, '2026-09-19', N'Бокс «Фермерские овощи», 5 кг',        2,  1290.00);
GO

SELECT * FROM [User];
SELECT * FROM [Category];
SELECT * FROM [Subscription];
GO
