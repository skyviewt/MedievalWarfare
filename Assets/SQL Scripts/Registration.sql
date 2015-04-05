CREATE DEFINER=`root`@`localhost` PROCEDURE `registration`(username NVARCHAR(50), password NVARCHAR(50))
BEGIN
    DECLARE _count int;
	CREATE TABLE IF NOT EXISTS Registration (
		username NVARCHAR(50) NOT NULL,
		password NVARCHAR(50) NOT NULL,
		PRIMARY KEY (username)
	);
    SELECT count(*) FROM registration WHERE username = username INTO _count;     
    IF (_count > 0) Then
		SELECT "User already exists.";
	Else
		INSERT IGNORE INTO Registration(username, password)
		VALUES (username, password);
		SELECT "User registered.";
    END IF;
END