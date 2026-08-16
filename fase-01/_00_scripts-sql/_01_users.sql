/* This script creates the Users, Accounts, and UsersPhotos tables in the database. */

CREATE TABLE Users (
    id INT NOT NULL PRIMARY KEY IDENTITY(1,1), -- começa em 1 e incrementa de 1 em 1
    fullname VARCHAR(255) NOT NULL,
    nickname VARCHAR(50) NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    admin BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP,
    validated_at DATETIME2 NULL,
    
    -- Adiciona a restrição (Check Constraint)
    CONSTRAINT CK_Users_ValidatedAt_GreaterThan_CreatedAt 
    CHECK (validated_at IS NULL OR validated_at >= created_at)
);

CREATE TABLE Accounts (
    id INT NOT NULL PRIMARY KEY REFERENCES Users(id) ON DELETE CASCADE,
    password_hash VARCHAR(255) NOT NULL,
    approved BIT NOT NULL DEFAULT 0,
    failed_counter INT NOT NULL DEFAULT 0
);

CREATE TABLE UsersPhotos (
    id INT NOT NULL PRIMARY KEY REFERENCES Users(id) ON DELETE CASCADE,
    content_type VARCHAR(50) NOT NULL,
    image VARBINARY(MAX) NOT NULL,
    thumbnail VARBINARY(MAX) NULL
);
