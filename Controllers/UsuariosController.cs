using AticaApp.Models;
using AticaApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace AticaApp.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepository _repository;

        public UsuariosController(UsuarioRepository repository)
        {
            _repository = repository;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            // Simulación simple de Rol para la prueba técnica
            var rolSimulado = HttpContext.Session.GetString("RolActual") ?? "Administrador";
            ViewBag.RolActual = rolSimulado;

            var usuarios = await _repository.ObtenerTodosAsync();

            // Lógica de permisos simplificada: si es Usuario, solo muestra usuarios con rol "Usuario"
            if (rolSimulado == "Usuario")
            {
                usuarios = usuarios.Where(u => u.Rol == "Usuario");
            }

            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            var rolSimulado = HttpContext.Session.GetString("RolActual") ?? "Administrador";
            ViewBag.RolActual = rolSimulado;

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            var rolSimulado = HttpContext.Session.GetString("RolActual") ?? "Administrador";
            ViewBag.RolActual = rolSimulado;
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario, string passwordPlano)
        {
            try
            {
                var rolSimulado = HttpContext.Session.GetString("RolActual") ?? "Administrador";
                ViewBag.RolActual = rolSimulado;

                if (rolSimulado != "Administrador")
                    usuario.Rol = "Usuario";

                // 1. Validación manual de la contraseña plana
                if (string.IsNullOrWhiteSpace(passwordPlano))
                    return BadRequest("La contraseña es obligatoria.");

                // 2. Asignar el Hash antes de validar el ModelState
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);

                // 3. LIMPIAR el error específico de PasswordHash y RE-VALIDAR
                ModelState.ClearValidationState(nameof(Usuario.PasswordHash));
                ModelState.MarkFieldValid(nameof(Usuario.PasswordHash));

                // 4. Ahora chequear si el modelo es válido
                if (!ModelState.IsValid)
                {
                    // Opcional: Loguear qué campos están fallando para debug
                    var errores = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest("Datos inválidos: " + errores);
                }

                await _repository.InsertarAsync(usuario);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var rol = HttpContext.Session.GetString("RolActual");
            var usuarioActualId = HttpContext.Session.GetInt32("UsuarioActualId");

            // Seguridad: Si no es Admin y no es su propio ID, prohibido
            if (rol != "Administrador" && usuarioActualId != id)
            {
                return Forbid();
            }

            var usuario = await _repository.ObtenerPorIdAsync(id);
            ViewBag.RolActual = rol;
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario, string passwordPlano)
        {
            if (id != usuario.Id) return BadRequest("ID no coincide");

            // Lógica de actualización (Dapper)
            try
            {
                if (!string.IsNullOrWhiteSpace(passwordPlano))
                {
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);
                }
                else
                {
                    var existente = await _repository.ObtenerPorIdAsync(id);
                    usuario.PasswordHash = existente.PasswordHash;
                }

                await _repository.ActualizarAsync(usuario);

                // Devolvemos un OK para que el success del AJAX se dispare
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: Usuarios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Solo el Administrador puede eliminar
            var rol = HttpContext.Session.GetString("RolActual") ?? "Administrador";
            if (rol != "Administrador") return Forbid();

            await _repository.EliminarAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
