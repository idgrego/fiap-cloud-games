CREATE TABLE EnumGamesCategories (
    id TINYINT NOT NULL PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO EnumGamesCategories (name) VALUES
    ('Unknown'),
    ('Action'),
    ('Adventure'),
    ('Role-Playing'),
    ('Simulation'),
    ('Strategy'),
    ('Sports'),
    ('Puzzle'),
    ('Idle');

CREATE TABLE Games (
    id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    name VARCHAR(200) NOT NULL,
    manufacturer VARCHAR(200) NOT NULL,
    description VARCHAR(MAX) NULL,
    online BIT NOT NULL DEFAULT 0,
    multiplyer BIT NOT NULL DEFAULT 0,
    category_id TINYINT NOT NULL REFERENCES EnumGamesCategories(id) DEFAULT 0,
    url_game VARCHAR(255) NULL,
    url_video VARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE GamesPhotos (
    id INT NOT NULL PRIMARY KEY REFERENCES Games(id) ON DELETE CASCADE,
    content_type VARCHAR(50) NOT NULL,
    image VARBINARY(MAX) NOT NULL,
    thumbnail VARBINARY(MAX) NULL
);




