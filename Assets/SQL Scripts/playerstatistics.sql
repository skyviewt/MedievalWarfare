CREATE DEFINER=`root`@`localhost` PROCEDURE `playerstatistics`(user_name NVARCHAR(50), games_played int, games_won int, ifOnline boolean)
BEGIN
	DECLARE _count int;
	CREATE TABLE IF NOT EXISTS Statistics (
		username NVARCHAR(50) NOT NULL,
		gamesplayed INT,
        gameswon INT,
        playerstatus boolean,
		PRIMARY KEY (username)
	);
    SELECT count(*) FROM Statistics WHERE username = user_name INTO _count;     
    IF (_count > 0) Then
		SELECT "User already exists in the statistics table.";
	Else
		INSERT IGNORE INTO Statistics(username, gamesplayed, gameswon, playerstatus)
		VALUES (user_name, games_played, games_won, ifOnline);
		SELECT "User statistic added.";
    END IF;
END