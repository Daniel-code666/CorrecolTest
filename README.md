# CorrecolTest

## Del backup al seed Code First

Se restauró `Backup_Prueba_desarrollador.bak` para inspeccionar y exportar sus catálogos: 246 países, 33 departamentos y 1.118 municipios. Los datos están en `CorrecolTest.Infrastructure/Persistence/SeedData`, como JSON UTF-8 incluidos en el ensamblado publicado.

La migración `InitialCreate` crea desde cero las tablas de países, departamentos, ciudades y clientes, con sus relaciones y campos de auditoría. La aplicación no necesita el archivo `.bak`. Los códigos geográficos originales se conservan como claves; los identificadores auxiliares y el nombre de departamento duplicado en municipios no se trasladan al modelo nuevo.

`CatalogSeed` inserta únicamente códigos faltantes, también cuando un catálogo está parcialmente cargado. No sobrescribe registros existentes. Los callbacks `UseSeeding` y `UseAsyncSeeding` se ejecutan bajo el bloqueo de migración de EF Core y la carga usa una transacción. La fecha de creación de los catálogos representa su importación, pues el backup no contiene fechas históricas de auditoría.

La base restaurada `CorrecolTest` se conserva para consulta. La aplicación usa una base independiente, `CorrecolTestApp`, creada por migraciones.

## Docker

Requiere Docker con contenedores Linux. Crear `.env` a partir de `.env.example` y definir `MSSQL_SA_PASSWORD` con una contraseña robusta. Si `.env` ya existe, conservar sus credenciales.

```powershell
Copy-Item .env.example .env  # Solo la primera vez, si .env no existe.
docker compose up --build -d
docker compose ps -a
```

Compose inicia SQL Server y espera su comprobación de disponibilidad antes de arrancar la API. La API aplica las migraciones y carga los catálogos faltantes antes de atender solicitudes. Este comportamiento se controla con `Database:InitializeOnStartup` en `appsettings.json`, habilitado por defecto. Cada reinicio comprueba las migraciones y el seed sin duplicar datos ni sobrescribir registros existentes. Si la inicialización falla, la API no comienza a atender solicitudes. Solo se necesitan los servicios `db` y `api`; la API expone su estado en `/health`.

- Swagger: http://localhost:5080/swagger
- SQL Server para DBeaver: `localhost:14333`, usuario `sa`, contraseña definida en `.env`.
- Base de la aplicación: `CorrecolTestApp`. Base del backup conservado: `CorrecolTest`.
- Para la conexión local, usar `encrypt=true` y `trustServerCertificate=true`.

`API_PORT`, `SQLSERVER_PORT` y `APP_DATABASE` son configurables en `.env`. La cadena de conexión tiene su estructura en `appsettings.json` y Compose la sustituye mediante `ConnectionStrings__DefaultConnection`. `AUTOMAPPER_LICENSE_KEY` permite configurar la licencia de AutoMapper. Las credenciales no se incluyen en el repositorio ni en la imagen.

`docker compose stop` detiene los servicios y conserva los datos en el volumen `correcoltest_sqlserver-data`. `docker compose up -d` permite iniciarlos de nuevo. No usar `docker compose down -v` si se desea conservar las bases.

## SOLID

- **Responsabilidad única:** controladores para HTTP, servicios para reglas de negocio, repositorios para EF Core, exportador para Excel y contexto para persistencia y auditoría.
- **Abierto/cerrado:** los contratos de persistencia y exportación permiten sustituir sus implementaciones sin cambiar los casos de uso.
- **Sustitución de Liskov:** las implementaciones respetan los contratos de sus interfaces; las entidades comparten las propiedades de `AuditTable`.
- **Segregación de interfaces:** clientes, catálogos y exportación tienen contratos separados, sin un repositorio genérico con operaciones innecesarias.
- **Inversión de dependencias:** Application depende de Domain y define interfaces; Infrastructure las implementa. La API compone estas dependencias mediante inyección.

`AuditTable` aporta `CreationDate` y `UpdatedDate` a cada entidad; no genera una tabla adicional. El contexto asigna las fechas UTC automáticamente y conserva la fecha de creación durante las actualizaciones. AutoMapper transforma DTOs y proyecta consultas; EF Core ejecuta las operaciones mediante LINQ, sin procedimientos almacenados nuevos.

## Middleware de excepciones

`ExceptionMiddleware` centraliza las excepciones de la API y genera respuestas `ProblemDetails`:

| Situación | HTTP |
| --- | --- |
| Validación de negocio | 400 |
| Recurso inexistente | 404 |
| Identificación duplicada, cliente inactivo o conflicto de concurrencia | 409 |
| Error inesperado | 500 |

Los errores inesperados se registran con su identificador de seguimiento y devuelven un mensaje genérico, sin detalles internos. Las cancelaciones del consumidor no intentan escribir una respuesta sobre una conexión cerrada. La validación de entrada de ASP.NET Core también devuelve `ValidationProblemDetails`, manteniendo el formato de errores.
