#!/bin/sh
export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"
exec /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -w 65535 "$@"
