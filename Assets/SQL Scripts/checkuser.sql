CREATE DEFINER=`root`@`localhost` PROCEDURE `checkUser`(username NVARCHAR(50), passwords NVARCHAR(50))
BEGIN
    DECLARE _count int;    
    SELECT count(*) FROM registration WHERE (username = username AND password = password) INTO _count; 
    IF (_count > 0) Then
		SELECT "User authenticated.";
	Else
		SELECT "User cannot be authenticated.";
    END IF;
END