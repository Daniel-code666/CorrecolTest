USE [master];
IF DB_ID(N'CorrecolTest') IS NOT NULL
    THROW 50001, 'CorrecolTest already exists. Restore stopped to preserve its data.', 1;

RESTORE VERIFYONLY FROM DISK = N'/var/opt/mssql/backup/Backup_Prueba_desarrollador.bak';
RESTORE DATABASE [CorrecolTest]
FROM DISK = N'/var/opt/mssql/backup/Backup_Prueba_desarrollador.bak'
WITH MOVE N'Prueba' TO N'/var/opt/mssql/data/CorrecolTest.mdf',
     MOVE N'Prueba_log' TO N'/var/opt/mssql/data/CorrecolTest_log.ldf',
     RECOVERY, STATS = 10;
