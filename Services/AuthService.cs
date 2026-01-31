namespace AticaApp.Services
{// Services/AuthService.cs
    using AticaApp.Data;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public class AuthService
    {
        private readonly IConfiguration _config;
        private readonly UsuarioRepository _repository;

        public AuthService(IConfiguration config, UsuarioRepository repository)
        {
            _config = config;
            _repository = repository;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var usuarios = await _repository.ObtenerTodosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Username == username);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Id", usuario.Id.ToString())
            }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
