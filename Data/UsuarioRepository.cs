using AticaApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AticaApp.Data
{
    public class UsuarioRepository
    {
        private readonly string _connectionString;
        private string? _passwordColumnCache;

        public UsuarioRepository(IConfiguration config)
        {
            var cs = config?.GetConnectionString("DefaultConnection");
            _connectionString = cs ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        // Detecta (y cachea) el nombre de la columna que guarda la contraseña en la tabla Usuarios
        private async Task<string> GetPasswordColumnAsync()
        {
            if (!string.IsNullOrEmpty(_passwordColumnCache)) return _passwordColumnCache!;

            using var db = new SqlConnection(_connectionString);
            // Buscar entre nombres comunes. Si no encuentra, asumimos "PasswordHash".
            var col = await db.QueryFirstOrDefaultAsync<string>(@"
                SELECT TOP 1 COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Usuarios'
                  AND COLUMN_NAME IN ('PasswordHash','Password','Password_Hash','PasswordHashBase')
            ");
            _passwordColumnCache = col ?? "PasswordHash";
            return _passwordColumnCache;
        }

        public async Task<IEnumerable<Usuario>> ListarTodos()
        {
            using var db = new SqlConnection(_connectionString);
            var pwdCol = await GetPasswordColumnAsync();
            var sql = $@"
                SELECT
                    Id,
                    Nombre,
                    Apellido,
                    Documento,
                    Email,
                    Rol,
                    Username,
                    {pwdCol} AS PasswordHash
                FROM Usuarios
            ";
            return await db.QueryAsync<Usuario>(sql);
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync() => await ListarTodos();

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            using var db = new SqlConnection(_connectionString);
            var pwdCol = await GetPasswordColumnAsync();
            var sql = $@"
                SELECT
                    Id,
                    Nombre,
                    Apellido,
                    Documento,
                    Email,
                    Rol,
                    Username,
                    {pwdCol} AS PasswordHash
                FROM Usuarios
                WHERE Id = @Id
            ";
            return await db.QuerySingleOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        public async Task InsertarAsync(Usuario usuario)
        {
            using var db = new SqlConnection(_connectionString);
            var pwdCol = await GetPasswordColumnAsync();

            var sql = $@"
                INSERT INTO Usuarios (Nombre, Apellido, Documento, Email, Rol, Username, {pwdCol})
                VALUES (@Nombre, @Apellido, @Documento, @Email, @Rol, @Username, @PasswordHash);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            var id = await db.ExecuteScalarAsync<int>(sql, new
            {
                usuario.Nombre,
                usuario.Apellido,
                usuario.Documento,
                usuario.Email,
                usuario.Rol,
                usuario.Username,
                usuario.PasswordHash
            });

            usuario.Id = id;
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            using var db = new SqlConnection(_connectionString);
            var pwdCol = await GetPasswordColumnAsync();

            var sql = $@"
                UPDATE Usuarios
                SET Nombre = @Nombre,
                    Apellido = @Apellido,
                    Documento = @Documento,
                    Email = @Email,
                    Rol = @Rol,
                    Username = @Username,
                    {pwdCol} = @PasswordHash
                WHERE Id = @Id
            ";
            await db.ExecuteAsync(sql, new
            {
                usuario.Nombre,
                usuario.Apellido,
                usuario.Documento,
                usuario.Email,
                usuario.Rol,
                usuario.Username,
                usuario.PasswordHash,
                usuario.Id
            });
        }

        public async Task EliminarAsync(int id)
        {
            using var db = new SqlConnection(_connectionString);
            await db.ExecuteAsync("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }
    }
}
