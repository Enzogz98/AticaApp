namespace AticaApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; } // Administrador o Usuario [cite: 9, 18]
        public string Username { get; set; }
        public string? PasswordHash { get; set; } // Para manejar el hash [cite: 12]
    }
}
