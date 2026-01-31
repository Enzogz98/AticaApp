# AticaApp — Gestión de Usuarios (mi descripción)

Descripción breve
- Esta es mi implementación de un CRUD de `Usuario` en ASP.NET Core (.NET 8) usando `Dapper`.
- Implementé control de roles simple (sesión): `Administrador` y `Usuario`.
- Los administradores pueden ver/crear/editar/eliminar cualquier usuario. Los usuarios no-admin pueden editar solo su propio registro y pueden crear otros usuarios (se normaliza su rol a `Usuario` si corresponde).

Estructura principal
- `Controllers/UsuariosController.cs` — acciones `Index`, `Details`, `Create`, `Edit`, `Delete`.
- `Data/UsuarioRepository.cs` — acceso a datos con `Dapper` y métodos CRUD.
- `Models/Usuario.cs` — modelo `Usuario`.
- `Views/Usuarios/*` — vistas Razor para CRUD.
- `Program.cs` — configuración (registro de `UsuarioRepository`, sesión habilitada).

Requisitos previos
- .NET 8 SDK instalado.
- Base de datos SQL Server accesible y cadena de conexión en `appsettings.json` bajo `ConnectionStrings:DefaultConnection`.

Cómo ejecutar (local)
1. Configuro la cadena de conexión en `appsettings.json`.
2. En la raíz del proyecto ejecuto:
   - `dotnet restore`
   - `dotnet build`
   - `dotnet run`
3. Abro `https://localhost:{puerto}/Usuarios` en el navegador.

Expectativas de sesión
- Mi implementación asume que el flujo de login guarda en sesión:
  - `Context.Session.SetString("RolActual", "Administrador" | "Usuario")`
  - `Context.Session.SetInt32("UsuarioActualId", <id>)`
- Sin esos valores, la seguridad y las vistas pueden comportarse como en la simulación por defecto.

Notas sobre la columna de contraseña
- El `Model` tiene `PasswordHash`.
- Para evitar errores si la columna en la tabla se llama distinto, detecté dinámicamente el nombre de la columna de contraseña y lo uso en `SELECT/INSERT/UPDATE`. Recomiendo homogeneizar la tabla renombrando la columna a `PasswordHash`.

Qué hice para evitar errores comunes
- Añadí `@Html.AntiForgeryToken()` y `asp-validation-summary` en formularios para evitar `400` por antiforgery.
- Habilité sesión (`builder.Services.AddSession()` y `app.UseSession()` en `Program.cs`).
- Añadí validaciones mínimas en controlador y manejo de errores en las acciones POST.
- Convertí tokens `[cite_start]` inválidos en comentarios para evitar errores de compilación.

Depuración rápida (si algo falla)
- Si el POST de `Create` devuelve 400:
  - Verifico que el formulario incluya `@Html.AntiForgeryToken()` y que la cookie antiforgery llegue.
  - Reviso la petición en DevTools -> Network para ver el estado HTTP y la respuesta.
- Si obtengo `Invalid column name 'PasswordHash'`:
  - Reviso la tabla `Usuarios` en la DB y renombro la columna a `PasswordHash`, o añado su nombre real a la lista de nombres soportados en `GetPasswordColumnAsync()` del repositorio.
- Si la sesión no funciona:
  - Confirmo que `Program.cs` tiene `AddSession()` y `UseSession()` y que el login guarda `RolActual` y `UsuarioActualId`.

