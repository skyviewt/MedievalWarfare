CREATE DEFINER=`root`@`localhost` PROCEDURE `updateLoser`(user_name nvarchar(50))
BEGIN
	UPDATE Statistics
	SET gamesplayed = gamesplayed + 1
	WHERE username = user_name;
END