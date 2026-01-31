using AticaApp.Data;
using AticaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace AticaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioRepository _repository;

        public AccountController(UsuarioRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var usuarios = await _repository.ObtenerTodosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Username == model.Username);

            if (usuario != null && BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                // Guardamos los datos en la sesión
                HttpContext.Session.SetString("RolActual", usuario.Rol);
                HttpContext.Session.SetInt32("UsuarioActualId", usuario.Id);

                // Redirección por Rol
               
                    return RedirectToAction("Index", "Usuarios");
                
            }

            TempData["Error"] = "Usuario o contraseña incorrectos";
            return View(model);
        }

        // Método para cerrar sesión (Debe ser HttpGet para funcionar con window.location.href)
        [HttpGet]
        public IActionResult Logout()
        {
            // Limpia todas las variables de sesión (RolActual, UsuarioActualId, etc.)
            HttpContext.Session.Clear();

            // Redirige a la pantalla de login
            return RedirectToAction("Login", "Account");
        }
    }
}