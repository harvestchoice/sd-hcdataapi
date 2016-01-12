-- On local machine, set all database instances to SIMPLE recovery models
-- Shrink log files
USE HC_DB_WEB_DEV
GO
DBCC SHRINKFILE('HC_DB_WEB_DEV_log', TRUNCATEONLY)
GO

USE HC_DB_WEB_2
GO
DBCC SHRINKFILE('HC_WEB_DB_2.0_Log', TRUNCATEONLY)
GO

USE HC_WEB_DB
GO
DBCC SHRINKFILE('HC_WEB_DB_2.0_Log', TRUNCATEONLY)
GO

USE HC_WEB_DB_old
GO
DBCC SHRINKFILE('HC_WEB_DB_2.0_Log', TRUNCATEONLY)
GO

SELECT state_desc DatabaseStatus_sysDatabase,*
FROM sys.databases where name='HC_DB_WEB_2'

ALTER DATABASE HC_DB_WEB_2 SET TRUSTWORTHY ON;

SELECT owner_sid FROM sys.databases WHERE database_id=DB_ID()

SELECT sid FROM sys.database_principals WHERE name=N'dbo'

USE HC_DB_WEB_2
GO
SP_DROPUSER [CANDACE\sparadee]
GO
SP_CHANGEDBOWNER [CANDACE\sparadee]

select compatibility_level
from   sys.databases 
where  name = 'HC_DB_WEB_2'

ALTER DATABASE HC_DB_WEB_2
SET COMPATIBILITY_LEVEL = 110 
