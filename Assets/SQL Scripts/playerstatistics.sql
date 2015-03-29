CREATE DEFINER=`root`@`localhost` PROCEDURE `playerstatistics`(username NVARCHAR(50), games_played int, games_won int, ifOnline boolean)
BEGIN
	DECLARE _count int;
	CREATE TABLE IF NOT EXISTS Statistics (
		username NVARCHAR(50) NOT NULL,
		gamesplayed INT,
        gameswon INT,
        playerstatus boolean,
		PRIMARY KEY (username)
	);
    SELECT count(*) FROM registration WHERE username = username INTO _count;     
    IF (_count > 0) Then
		SELECT "User already exists in the statistics table.";
	Else
		INSERT IGNORE INTO Registration(username, password)
		VALUES (username, games_played, games_won, ifOnline);
		SELECT "User statistic added.";
    END IF;
END