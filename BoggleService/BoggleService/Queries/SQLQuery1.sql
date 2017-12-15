--Clears table and resets identity to 0
--DELETE FROM Games
--DBCC CHECKIDENT(Games, RESEED, 0)

--Adds users to game
--select * from Games
--insert into Games(Player1, Player2, Board, TimeLimit, StartTime, GameState) values('b75719a0-3f8f-45f6-9edf-1dfad455fd73', null, 'abcdefghijklmnop', 30, null, 'pending');
--INSERT INTO Games(Player1, Player2, Board, TimeLimit, StartTime, GameState) VALUES('b75719a0-3f8f-45f6-9edf-1dfad455fd73', null, 'EHYTOGAELZESJHNT', 120, null, 'pending')
--select * from Games
--update Games set Player2 = 'ebe93cdd-345c-49e4-849b-42e21d035044', TimeLimit = '60', StartTime = current_timestamp, GameStatus = 'active' where GameID = 10
--select * from Games

--SELECT MAX(GameID) AS GamesCount FROM Games

--update users set gameid = null

--INSERT INTO words(Word, GameID, Player, Score) VALUES('Test', 2, 'd78480f2-afec-4f23-8041-76ce52b6e68b', 1)

--DELETE FROM Games
--DBCC CHECKIDENT(Games, RESEED, 0)
----DELETE FROM Users
--DELETE FROM Words


SELECT * FROM Games
SELECT * FROM users
SELECT * FROM words
