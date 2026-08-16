-- =========================================================================
-- Script para popular as tabelas [Games] e [GamePhotos]
-- Contém jogos reais dos consoles Xbox, PlayStation 5 e Nintendo Switch
-- Mapeamento das Categorias (CategoryId):
-- 1 = Action, 2 = Adventure, 3 = RolePlaying, 4 = Simulation, 5 = Strategy
-- 6 = Sports, 7 = Puzzle, 8 = Racing, 9 = Fighting, 10 = Horror
-- =========================================================================

-- Clean/Reset opcional (Descomente se desejar limpar as tabelas antes de inserir)
-- DELETE FROM [GamePhotos];
-- DELETE FROM [Games];
-- DBCC CHECKIDENT ('Games', RESEED, 0);

-- -------------------------------------------------------------------------
-- 1. JOGOS DE AÇÃO (CategoryId = 1)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Halo Infinite', 'Xbox Game Studios', 1, 'O Lendário Master Chief retorna na maior aventura já criada da franquia Halo para salvar a humanidade.', 1, 1, '2021-12-08', 'https://www.xbox.com/pt-BR/games/halo-infinite', 'https://www.youtube.com/watch?v=PyMlV5_HRWk', GETUTCDATE()),
('God of War Ragnarök', 'Sony Interactive Entertainment', 1, 'Kratos e Atreus embarcam em uma jornada épica em busca de respostas nos Nove Reinos antes do Ragnarök.', 0, 0, '2022-11-09', 'https://www.playstation.com/pt-br/games/god-of-war-ragnarok/', 'https://www.youtube.com/watch?v=hfJ4Km46A-0', GETUTCDATE()),
('Super Mario Odyssey', 'Nintendo', 1, 'Junte-se a Mario em uma enorme aventura 3D pelo mundo todo e use suas novas habilidades para salvar a Princesa Peach.', 0, 1, '2017-10-27', 'https://www.nintendo.com/pt-br/store/products/super-mario-odyssey-switch/', 'https://www.youtube.com/watch?v=wGQHQc_3yYo', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 2. JOGOS DE AVENTURA (CategoryId = 2)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Indiana Jones e o Grande Círculo', 'Bethesda Softworks', 2, 'Desvende um dos maiores mistérios da história em uma aventura em primeira pessoa no papel de Indiana Jones.', 0, 0, '2024-12-09', 'https://www.xbox.com/pt-BR/games/indiana-jones-and-the-great-circle', 'https://www.youtube.com/watch?v=cM2IskD36K0', GETUTCDATE()),
('Uncharted: Coleção Legado dos Ladrões', 'Sony Interactive Entertainment', 2, 'Busque sua sorte e deixe sua marca no mapa com Nathan Drake e Chloe Frazer.', 0, 0, '2022-01-28', 'https://www.playstation.com/pt-br/games/uncharted-legacy-of-thieves-collection/', 'https://www.youtube.com/watch?v=v=vO_cO05s0O0', GETUTCDATE()),
('The Legend of Zelda: Tears of the Kingdom', 'Nintendo', 2, 'Uma aventura épica pelas terras e céus de Hyrule aguarda nesta sequência de Breath of the Wild.', 0, 0, '2023-05-12', 'https://www.nintendo.com/pt-br/store/products/the-legend-of-zelda-tears-of-the-kingdom-switch/', 'https://www.youtube.com/watch?v=uHGShqcAHlQ', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 3. JOGOS DE RPG / ROLEPLAYING (CategoryId = 3)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Starfield', 'Bethesda Game Studios', 3, 'O primeiro novo universo em mais de 25 anos da Bethesda. Crie qualquer personagem e explore com liberdade sem precedentes.', 0, 0, '2023-09-06', 'https://www.xbox.com/pt-BR/games/starfield', 'https://www.youtube.com/watch?v=kfYEiTdsyas', GETUTCDATE()),
('Final Fantasy VII Rebirth', 'Square Enix', 3, 'A nova jornada no projeto de remake de Final Fantasy VII onde Cloud e seus amigos exploram o vasto planeta.', 0, 0, '2024-02-29', 'https://www.playstation.com/pt-br/games/final-fantasy-vii-rebirth/', 'https://www.youtube.com/watch?v=H_UnJ721S_g', GETUTCDATE()),
('Xenoblade Chronicles 3', 'Nintendo', 3, 'Junte-se a Noah e Mio em meio à turbulência entre as nações hostis de Keves e Agnus.', 0, 0, '2022-07-29', 'https://www.nintendo.com/pt-br/store/products/xenoblade-chronicles-3-switch/', 'https://www.youtube.com/watch?v=tI4A_cO2pYI', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 4. JOGOS DE SIMULAÇÃO (CategoryId = 4)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Microsoft Flight Simulator 2024', 'Xbox Game Studios', 4, 'Explore o mundo com a maior frota de aeronaves em uma simulação de aviação incrivelmente detalhada.', 1, 1, '2024-11-19', 'https://www.xbox.com/pt-BR/games/microsoft-flight-simulator-2024', 'https://www.youtube.com/watch?v=p3xg-sJ-7E8', GETUTCDATE()),
('Gran Turismo 7', 'Sony Interactive Entertainment', 4, 'O simulador de corrida real definitivo que reúne os melhores recursos da história da franquia.', 1, 1, '2022-03-04', 'https://www.playstation.com/pt-br/games/gran-turismo-7/', 'https://www.youtube.com/watch?v=1tBUsXj20pM', GETUTCDATE()),
('Animal Crossing: New Horizons', 'Nintendo', 4, 'Crie seu próprio paraíso em uma ilha deserta e customize tudo ao seu gosto.', 1, 1, '2020-03-20', 'https://www.nintendo.com/pt-br/store/products/animal-crossing-new-horizons-switch/', 'https://www.youtube.com/watch?v=_3YNL0OW1VI', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 5. JOGOS DE ESTRATÉGIA (CategoryId = 5)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Age of Empires IV: Anniversary Edition', 'Xbox Game Studios', 5, 'Um dos mais aclamados jogos de estratégia em tempo real retorna com batalhas históricas épicas.', 1, 1, '2023-08-22', 'https://www.xbox.com/pt-BR/games/age-of-empires-iv', 'https://www.youtube.com/watch?v=QT41804fX8E', GETUTCDATE()),
('Helldivers 2', 'Sony Interactive Entertainment', 5, 'Junte-se aos Helldivers e lute pela liberdade em uma galáxia hostil neste jogo de tiro tático coop.', 1, 1, '2024-02-08', 'https://www.playstation.com/pt-br/games/helldivers-2/', 'https://www.youtube.com/watch?v=UC_3S8_g0_M', GETUTCDATE()),
('Fire Emblem Engage', 'Nintendo', 5, 'Invoque os lendários Emblemas para defender o continente de Elyos neste RPG de estratégia em turnos.', 0, 0, '2023-01-20', 'https://www.nintendo.com/pt-br/store/products/fire-emblem-engage-switch/', 'https://www.youtube.com/watch?v=3ex_5S_L3S0', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 6. JOGOS DE ESPORTES (CategoryId = 6)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('EA SPORTS FC 24', 'Electronic Arts', 6, 'O início do futuro do futebol no mundo dos videogames com licenciamentos oficiais e jogabilidade hypermotion.', 1, 1, '2023-09-29', 'https://www.ea.com/pt-br/games/ea-sports-fc/fc-24', 'https://www.youtube.com/watch?v=XhP3Xh4L2oE', GETUTCDATE()),
('MLB The Show 24', 'Sony Interactive Entertainment', 6, 'Viva seus momentos de glória no beisebol e torne-se uma lenda na Major League Baseball.', 1, 1, '2024-03-19', 'https://www.playstation.com/pt-br/games/mlb-the-show-24/', 'https://www.youtube.com/watch?v=d_kS9Y4u_gM', GETUTCDATE()),
('Mario Strikers: Battle League', 'Nintendo', 6, 'Um esporte de 5 contra 5 parecido com futebol, sem regras, onde o objetivo é vencer a qualquer custo.', 1, 1, '2022-06-10', 'https://www.nintendo.com/pt-br/store/products/mario-strikers-battle-league-switch/', 'https://www.youtube.com/watch?v=v=_3Z3zM00M0M', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 7. JOGOS DE PUZZLE / QUEBRA-CABEÇA (CategoryId = 7)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Tetris Effect: Connected', 'Enhance Games', 7, 'Tetris como você nunca viu, ouviu ou sentiu antes, com modos cooperativos e competitivos online.', 1, 1, '2020-11-10', 'https://www.xbox.com/pt-BR/games/tetris-effect-connected', 'https://www.youtube.com/watch?v=v=k8vI8p60O30', GETUTCDATE()),
('Astro Bot', 'Sony Interactive Entertainment', 7, 'Uma aventura gigante de plataforma e quebra-cabeças cheia de nostalgia e uso do controle DualSense.', 0, 0, '2024-09-06', 'https://www.playstation.com/pt-br/games/astro-bot/', 'https://www.youtube.com/watch?v=3S3Y4u00M0M', GETUTCDATE()),
('Captain Toad: Treasure Tracker', 'Nintendo', 7, 'Guie o Captain Toad através de mapas cheios de armadilhas e segredos para encontrar os tesouros.', 0, 1, '2018-07-13', 'https://www.nintendo.com/pt-br/store/products/captain-toad-treasure-tracker-switch/', 'https://www.youtube.com/watch?v=tI4M3N00M0M', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 8. JOGOS DE CORRIDA (CategoryId = 8)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Forza Horizon 5', 'Xbox Game Studios', 8, 'Aventure-se pelas paisagens vibrantes e em constante evolução do México com ação ilimitada ao volante.', 1, 1, '2021-11-09', 'https://www.xbox.com/pt-BR/games/forza-horizon-5', 'https://www.youtube.com/watch?v=FYH9n37B7Yw', GETUTCDATE()),
('Need for Speed Unbound', 'Electronic Arts', 8, 'Corra contra o tempo, despiste a polícia e enfrente as qualificatórias semanais do circuito de rua.', 1, 1, '2022-12-02', 'https://www.ea.com/pt-br/games/need-for-speed/need-for-speed-unbound', 'https://www.youtube.com/watch?v=H2Y8S_2o_3s', GETUTCDATE()),
('Mario Kart 8 Deluxe', 'Nintendo', 8, 'Acelere pelas pistas com personagens do universo Mario e use itens para derrotar seus oponentes.', 1, 1, '2017-04-28', 'https://www.nintendo.com/pt-br/store/products/mario-kart-8-deluxe-switch/', 'https://www.youtube.com/watch?v=tKlRN2Y84hU', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 9. JOGOS DE LUTA (CategoryId = 9)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Mortal Kombat 1', 'Warner Bros. Games', 9, 'Descubra um Universo de Mortal Kombat renascido, criado pelo Deus do Fogo Liu Kang.', 1, 1, '2023-09-19', 'https://www.xbox.com/pt-BR/games/mortal-kombat-1', 'https://www.youtube.com/watch?v=MYb34_p34_M', GETUTCDATE()),
('Tekken 8', 'Bandai Namco Entertainment', 9, 'Sinta o poder de cada golpe nesta lendária franquia de luta 3D com gráficos de nova geração.', 1, 1, '2024-01-26', 'https://www.playstation.com/pt-br/games/tekken-8/', 'https://www.youtube.com/watch?v=_23Y4u00M0M', GETUTCDATE()),
('Super Smash Bros. Ultimate', 'Nintendo', 9, 'Lutadores lendários e mundos de jogos se reúnem no confronto definitivo.', 1, 1, '2018-12-07', 'https://www.nintendo.com/pt-br/store/products/super-smash-bros-ultimate-switch/', 'https://www.youtube.com/watch?v=WShCN-AYHqA', GETUTCDATE());

-- -------------------------------------------------------------------------
-- 10. JOGOS DE TERROR / HORROR (CategoryId = 10)
-- -------------------------------------------------------------------------
INSERT INTO [Games] ([Name], [Manufacturer], [CategoryId], [Description], [Online], [Multiplayer], [ReleasedAt], [UrlGame], [UrlVideo], [CreatedAt])
VALUES 
('Alan Wake 2', 'Epic Games Publishing', 10, 'Uma série de assassinatos rituais ameaça Bright Falls. Alan Wake luta para escapar do seu pesadelo.', 0, 0, '2023-10-27', 'https://www.xbox.com/pt-BR/games/alan-wake-2', 'https://www.youtube.com/watch?v=dlQ3fe-pA4s', GETUTCDATE()),
('Resident Evil 4 Remake', 'Capcom', 10, 'Leon S. Kennedy é enviado em uma missão para resgatar a filha do presidente dos EUA em um vilarejo europeu.', 0, 0, '2023-03-24', 'https://www.playstation.com/pt-br/games/resident-evil-4-remake/', 'https://www.youtube.com/watch?v=j5Xv23_p34_M', GETUTCDATE()),
('Luigi''s Mansion 3', 'Nintendo', 10, 'Luigi deve salvar Mario e seus amigos em um hotel assombrado repleto de fantasmas divertidos e assustadores.', 0, 1, '2019-10-31', 'https://www.nintendo.com/pt-br/store/products/luigis-mansion-3-switch/', 'https://www.youtube.com/watch?v=RSGgC6B8D30', GETUTCDATE());

