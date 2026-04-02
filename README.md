# TrackingApi

API REST legacy para exponer tracking usando acceso directo a SQL Server, sin SP.

## Base

- Framework: `.NET 10`
- Proyecto: `C:\ERP TL\Srv_Ventas\APIs\TrackingApi\TrackingApi`
- Publish recomendado: `C:\Servicios\APIs\TrackingApi`
- Autenticación: `Bearer`
- Token: `7 días`

## Configuración

Archivo principal:

`C:\Servicios\APIs\TrackingApi\appsettings.json`

Ejemplo:

```json
{
  "ConnectionStrings": {
    "Gestion": "Server=181.78.30.214,49143;Database=Gestion;User Id=USER_SQL;Password=PASSWORD_SQL;TrustServerCertificate=True"
  },
  "Auth": {
    "Username": "api",
    "Password": "api175",
    "DisplayName": "admin"
  },
  "Jwt": {
    "Key": "CHANGE_ME",
    "Issuer": "TrackingApi",
    "Audience": "TrackingApiClients",
    "TokenDays": 7
  },
  "ApiBinding": {
    "Url": "http://localhost:5105"
  }
}
```

## Endpoints

### `GET /health`

Valida estado de API y conectividad SQL.

Ejemplo:

```bash
curl.exe http://localhost:5105/health
```

Respuesta `200`:

```json
{
  "status": "Healthy",
  "checks": {
    "sql": "Healthy"
  }
}
```

Respuesta `503`:

```json
{
  "status": "Unhealthy",
  "checks": {
    "sql": "Unhealthy"
  }
}
```

### `POST /api/v1/login`

Sin token.

Parámetros:

- `username`
- `password`

Compatibilidad:

- acepta `multipart/form-data`
- acepta `querystring`

Ejemplo querystring:

```bash
curl.exe -X POST "http://localhost:5105/api/v1/login?username=api&password=api175"
```

Ejemplo `multipart/form-data`:

```bash
curl.exe -X POST "http://localhost:5105/api/v1/login" ^
  -F "username=api" ^
  -F "password=api175"
```

Respuesta OK:

```json
{
  "status": true,
  "message": "User login successfully.",
  "data": {
    "token": "TOKEN",
    "type": "Bearer",
    "name": "admin",
    "expired_at": "2026-04-08 12:00:00"
  }
}
```

Respuesta error:

```json
{
  "status": false,
  "message": "ERROR EN LOS DATOS ENTREGADOS (username o password incorrectos)"
}
```

### `POST /api/v1/etiqueta`

Requiere header:

`Authorization: Bearer {token}`

Parámetro:

- `codigo`

Busca por:

- `Id_Delivery`
- `NombreArchivo`
- `Cod_Pedido` si el valor es GUID

Ejemplo:

```bash
curl.exe -X POST "http://localhost:5105/api/v1/etiqueta" ^
  -H "Authorization: Bearer TOKEN" ^
  -F "codigo=191534"
```

Respuesta OK:

```json
{
  "status": true,
  "message": "Transacción OK.",
  "data": {
    "id_delivery": "990089831295",
    "id_pedido": "212379",
    "codigoZebra": "^XA^XZ"
  }
}
```

### `POST /api/v1/estado_pedido`

Requiere header:

`Authorization: Bearer {token}`

Parámetro:

- `codigo`

Ejemplo:

```bash
curl.exe -X POST "http://localhost:5105/api/v1/estado_pedido" ^
  -H "Authorization: Bearer TOKEN" ^
  -F "codigo=191534"
```

Respuesta OK:

```json
{
  "status": true,
  "message": "Transacción OK.",
  "data": {
    "status": "ENTREGADO"
  }
}
```

### `POST /api/v1/orders_tracking`

Requiere header:

`Authorization: Bearer {token}`

Parámetros obligatorios:

- `id_traking`
- `cod_pedido`
- `id_delivery`
- `nombreArchivo`
- `codigoZebra`
- `nombres`
- `apellidos`
- `commune_code`

Compatibilidad:

- acepta `multipart/form-data`
- acepta `querystring`

Reglas:

- crea registro nuevo
- si falta un campo, responde error legacy
- si `cod_pedido` ya existe, responde duplicado
- guarda en `Ges_EcoOrdersTracking`

Ejemplo querystring:

```bash
curl.exe -X POST "http://localhost:5105/api/v1/orders_tracking?id_traking=1&cod_pedido=E00C2C6A-91BC-40F1-8855-C3EF685A0B38&id_delivery=990089831295&nombreArchivo=212379&codigoZebra=^XA^XZ&nombres=Pedro&apellidos=Castillo&commune_code=E90D244D-143E-4134-B0AB-E08551E0126E" ^
  -H "Authorization: Bearer TOKEN"
```

Ejemplo `multipart/form-data`:

```bash
curl.exe -X POST "http://localhost:5105/api/v1/orders_tracking" ^
  -H "Authorization: Bearer TOKEN" ^
  -F "id_traking=1" ^
  -F "cod_pedido=E00C2C6A-91BC-40F1-8855-C3EF685A0B38" ^
  -F "id_delivery=990089831295" ^
  -F "nombreArchivo=212379" ^
  -F "codigoZebra=^XA^XZ" ^
  -F "nombres=Pedro" ^
  -F "apellidos=Castillo" ^
  -F "commune_code=E90D244D-143E-4134-B0AB-E08551E0126E"
```

Respuesta OK:

```json
{
  "status": true,
  "message": "Transacción OK.",
  "data": {
    "status": ""
  }
}
```

Respuesta duplicado:

```json
{
  "status": false,
  "message": "YA EXISTE EL REGISTRO EN EL SISTEMA"
}
```

## Publicación

Comando de publish:

```powershell
dotnet publish "C:\ERP TL\Srv_Ventas\APIs\TrackingApi\TrackingApi\TrackingApi.csproj" -c Release -r win-x64 --self-contained true -o "C:\Servicios\APIs\TrackingApi"
```

## IIS

Para IIS directo:

- `Physical Path`: `C:\Servicios\APIs\TrackingApi`
- `Application Pool`: `No Managed Code`
- Hosting Bundle ASP.NET Core instalado en el servidor

## Tablas usadas

- `Ges_EcoOrdersTracking`
- `Ges_EcoOrders`
- `Ges_Parametros`
