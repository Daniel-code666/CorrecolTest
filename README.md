# CorrecolTest

## Del backup al seed Code First

Se restauró `Backup_Prueba_desarrollador.bak` para inspeccionar y exportar sus catálogos: 246 países, 33 departamentos y 1.118 municipios. Los datos están en `CorrecolTest.Infrastructure/Persistence/SeedData`, como JSON UTF-8 incluidos en el ensamblado publicado.

La migración `InitialCreate` crea desde cero las tablas de países, departamentos, ciudades y clientes, con sus relaciones y campos de auditoría. La aplicación no necesita el archivo `.bak`. Los códigos geográficos originales se conservan como claves; los identificadores auxiliares y el nombre de departamento duplicado en municipios no se trasladan al modelo nuevo.

`CatalogSeed` inserta únicamente códigos faltantes, también cuando un catálogo está parcialmente cargado. No sobrescribe registros existentes ni reactiva países, departamentos o ciudades desactivados mediante `Active`. Los callbacks `UseSeeding` y `UseAsyncSeeding` se ejecutan bajo el bloqueo de migración de EF Core y la carga usa una transacción. La fecha de creación de los catálogos representa su importación, pues el backup no contiene fechas históricas de auditoría. La migración `AddCatalogActive` incorpora la eliminación lógica de los catálogos conservando los registros existentes.

La base restaurada `CorrecolTest` se conserva para consulta. La aplicación usa una base independiente, `CorrecolTestApp`, creada por migraciones.

## Docker

El `compose.yaml` reúne el frontend Angular, la API .NET y SQL Server en el mismo proyecto Docker. Requiere Docker con contenedores Linux y una copia del proyecto frontend accesible desde este equipo.

### Configuración y arranque

Desde la carpeta del backend, crear `.env` a partir de `.env.example` solo si todavía no existe:

```powershell
Copy-Item .env.example .env
```

Editar `.env`: definir `MSSQL_SA_PASSWORD` con una contraseña robusta y `FRONTEND_PATH` con la ruta a la carpeta del frontend que contiene su `Dockerfile` y `package.json`. Si `.env` ya existe, conservar las credenciales y ajustar únicamente las variables necesarias.

```dotenv
# Ejemplo con ambos proyectos en carpetas hermanas.
FRONTEND_PATH=../Front
FRONTEND_PORT=4200
API_PORT=5080
SQLSERVER_PORT=14333
APP_DATABASE=CorrecolTestApp
```

`FRONTEND_PATH` admite una ruta relativa a `compose.yaml` o una ruta absoluta. Para rutas de Windows con espacios, usar barras `/` y comillas simples, por ejemplo `FRONTEND_PATH='C:/Proyectos de trabajo/Front'`. Configurar la ruta real de cada equipo; `../Front` es solo un ejemplo.

```powershell
docker compose up --build -d
docker compose ps -a
```

| Servicio | Función | Dirección desde el equipo | Dirección interna |
| --- | --- | --- | --- |
| `front` | Nginx sirve la aplicación Angular y redirige las solicitudes a la API | http://localhost:4200 | `front:80` |
| `api` | API .NET | http://localhost:5080/swagger | `api:8080` |
| `db` | SQL Server con datos persistentes | `localhost:14333` | `db:1433` |

Las direcciones locales de la tabla usan los puertos predeterminados. Compose publica los puertos únicamente en `127.0.0.1`; `FRONTEND_PORT`, `API_PORT` y `SQLSERVER_PORT` permiten cambiarlos en `.env`.

### Comunicación entre frontend y backend

Los tres servicios comparten la red que Compose crea automáticamente. Docker resuelve los nombres de servicio `api` y `db` dentro de esa red.

Angular utiliza rutas relativas como `/api/clientes`. El navegador envía la solicitud al mismo origen que sirve la página y Nginx la reenvía al backend mediante esta configuración del archivo `nginx.conf` del frontend:

```nginx
location /api/ {
    proxy_pass http://api:8080;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

```text
Navegador: http://localhost:4200/api/clientes
    → front (Nginx, puerto 80)
    → api:8080/api/clientes
    → db:1433
```

`api:8080` es la dirección interna del backend; `localhost:5080` es su acceso desde el equipo. Cambiar `API_PORT` no cambia el destino interno del proxy. Con este esquema, el navegador utiliza un mismo origen y no necesita CORS para comunicarse con la API.

Para desarrollar Angular fuera de Docker con `npm start`, el archivo `proxy.conf.json` del frontend redirige `/api/**` a `http://localhost:5080`. Si se modifica `API_PORT`, también debe ajustarse ese proxy de desarrollo. Si el frontend de Docker ya ocupa el puerto 4200, usar `npm start -- --port 4201`.

### Disponibilidad y persistencia

Compose inicia SQL Server y espera su comprobación de disponibilidad antes de arrancar la API. La API aplica las migraciones y carga los catálogos faltantes antes de atender solicitudes. Este comportamiento se controla con `Database:InitializeOnStartup` en `appsettings.json`, habilitado por defecto. Cada reinicio comprueba las migraciones y el seed sin duplicar datos ni sobrescribir registros existentes. Si la inicialización falla, la API no comienza a atender solicitudes. El servicio `front` espera a que la API esté saludable antes de arrancar.

La API expone su estado en http://localhost:5080/health. Nginx tiene su propia comprobación en http://localhost:4200/health, que indica la disponibilidad del frontend, sin comprobar la base de datos.

- SQL Server para DBeaver: `localhost:14333`, usuario `sa`, contraseña definida en `.env`.
- Base de la aplicación: `CorrecolTestApp`. Base del backup conservado: `CorrecolTest`.
- Para la conexión local, usar `encrypt=true` y `trustServerCertificate=true`.

`APP_DATABASE` es configurable en `.env`. La cadena de conexión tiene su estructura en `appsettings.json` y Compose la sustituye mediante `ConnectionStrings__DefaultConnection`, utilizando `db` como servidor. `AUTOMAPPER_LICENSE_KEY` permite configurar la licencia de AutoMapper. Las credenciales no se incluyen en el repositorio ni en la imagen.

Para reconstruir solo el frontend después de cambiar su código, con la API ya en ejecución:

```powershell
docker compose up -d --build --no-deps front
```

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
| Duplicados, registros inactivos, relaciones que impiden desactivar o conflictos de concurrencia | 409 |
| Error inesperado | 500 |

Los errores inesperados se registran con su identificador de seguimiento y devuelven un mensaje genérico, sin detalles internos. Las cancelaciones del consumidor no intentan escribir una respuesta sobre una conexión cerrada. La validación de entrada de ASP.NET Core también devuelve `ValidationProblemDetails`, manteniendo el formato de errores.
