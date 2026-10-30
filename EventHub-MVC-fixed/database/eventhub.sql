-- EVENTHUB DATABASE - SQL SERVER
-- Chay file nay 1 lan. Tai khoan mau + du lieu mau duoc app tu tao khi chay lan dau.
IF DB_ID('EventHub') IS NULL CREATE DATABASE EventHub;
GO
USE EventHub;
GO
IF OBJECT_ID('dbo.bookings','U') IS NOT NULL DROP TABLE dbo.bookings;
IF OBJECT_ID('dbo.venues','U')   IS NOT NULL DROP TABLE dbo.venues;
IF OBJECT_ID('dbo.users','U')    IS NOT NULL DROP TABLE dbo.users;
GO
CREATE TABLE dbo.users (
    id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    name          NVARCHAR(100) NOT NULL,
    email         NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    role          NVARCHAR(30)  NOT NULL DEFAULT 'CLIENT',   -- ADMIN | COORDINATOR | CLIENT
    created_at    DATETIME2     NOT NULL DEFAULT SYSDATETIME()
);
CREATE TABLE dbo.venues (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    name        NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    address     NVARCHAR(500) NOT NULL,
    capacity    INT           NOT NULL,
    price       DECIMAL(12,2) NOT NULL DEFAULT 0,
    status      NVARCHAR(30)  NOT NULL DEFAULT 'AVAILABLE',  -- AVAILABLE | MAINTENANCE
    image_url   NVARCHAR(500) NULL,
    created_at  DATETIME2     NOT NULL DEFAULT SYSDATETIME()
);
CREATE TABLE dbo.bookings (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id     BIGINT NOT NULL REFERENCES dbo.users(id),
    venue_id    BIGINT NOT NULL REFERENCES dbo.venues(id),
    event_date  DATE   NOT NULL,
    start_time  TIME   NOT NULL,
    end_time    TIME   NOT NULL,
    guest_count INT    NOT NULL,
    status      NVARCHAR(30) NOT NULL DEFAULT 'PENDING',     -- PENDING | CONFIRMED | COMPLETED | CANCELLED
    notes       NVARCHAR(MAX) NULL,
    created_at  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO
