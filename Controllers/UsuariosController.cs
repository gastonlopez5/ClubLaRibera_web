using CLubLaRibera_Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CLubLaRibera_Web.Controllers
{
    [Authorize(Policy = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly DataContext _context;
        private readonly IConfiguration config;
        private readonly IHostingEnvironment environment;
        private readonly Utilidades utilidades = new Utilidades();

        public UsuariosController(DataContext context, IConfiguration config, IHostingEnvironment environment)
        {
            _context = context;
            this.config = config;
            this.environment = environment;
        }

        // GET: UsuarioController
        public ActionResult Index()
        {
            return View();
        }

        // GET: UsuarioController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuarioController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsuarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuarioController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UsuarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuarioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [AllowAnonymous]
        public ActionResult LoginModal()
        {
            return PartialView("_LoginModal", new LoginView());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginView loginView)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: loginView.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes("Salt"),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                    var p = await _context.Usuarios
                        .Include(a => a.TipoUsuario)
                        .FirstOrDefaultAsync(a => a.Email == loginView.Email);

                    if (p == null || p.Clave != hashed)
                    {
                        ViewBag.Error = "Email o Contraseña incorrectos";
                        return PartialView("_LoginModal", loginView);
                    }
                    else
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, p.Email),
                            new Claim(ClaimTypes.Role, p.TipoUsuario.Rol),
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            //AllowRefresh = <bool>,
                            // Refreshing the authentication session should be allowed.
                            AllowRefresh = true,
                            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                            // The time at which the authentication ticket expires. A 
                            // value set here overrides the ExpireTimeSpan option of 
                            // CookieAuthenticationOptions set with AddCookie.

                            //IsPersistent = true,
                            // Whether the authentication session is persisted across 
                            // multiple requests. When used with cookies, controls
                            // whether the cookie's lifetime is absolute (matching the
                            // lifetime of the authentication ticket) or session-based.

                            //IssuedUtc = <DateTimeOffset>,
                            // The time at which the authentication ticket was issued.

                            //RedirectUri = <string>
                            // The full path or absolute URI to be used as an http 
                            // redirect response value.
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        ViewBag.Success = "Bienvenido " + p.Nombre + "!!";

                        return PartialView("_LoginModal", loginView);
                    }
                }
                else
                {
                    return PartialView("_LoginModal", loginView);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.StackTrate = ex.StackTrace;
                return PartialView("_LoginModal", loginView);
            }
        }

        [AllowAnonymous]
        public ActionResult RegistroModal()
        {
            ViewBag.Roles = _context.Roles.ToList();
            //return View();
            return PartialView("_RegistroModal", new Usuario());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RegistrarUsuario(Usuario usuario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<String> permitidos = new List<string>();
                    permitidos.AddRange(config["Permitidos"].Split());
                    long limite_kb = 600;
                    usuario.Clave = "4321";

                    if (_context.Usuarios.Any(x => x.Email == usuario.Email))
                    {
                        ViewBag.Error = "Ya existe un propietario con ese email o dni";
                        return PartialView("_RegistroModal", usuario);
                    }
                    else
                    {
                        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: usuario.Clave,
                        salt: System.Text.Encoding.ASCII.GetBytes("Salt"),
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 1000,
                        numBytesRequested: 256 / 8));

                        usuario.Clave = hashed;

                        if (usuario.Archivo != null && (!permitidos.Contains(usuario.Archivo.ContentType) || usuario.Archivo.Length >= limite_kb * 1024))
                        {
                            ViewBag.Error = "El archivo seleccionado no es una imagen o excede el tamaño de 600 kb";
                            return PartialView("_RegistroModal", usuario);
                        }

                        _context.Usuarios.Add(usuario);
                        await _context.SaveChangesAsync();

                        if (usuario.Archivo != null)
                        {
                            var id = _context.Usuarios.FirstOrDefault(u => u.Email == usuario.Email).Id;
                            string wwwPath = environment.WebRootPath;
                            string path = Path.Combine(wwwPath, "FotoPerfil\\" + id);

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            string fileName = Path.GetFileName(usuario.Archivo.FileName);
                            string pathCompleto = Path.Combine(path, fileName);
                            usuario.FotoPerfil = Path.Combine("\\FotoPerfil\\" + id, fileName);
                            usuario.Id = id;

                            using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                            {
                                usuario.Archivo.CopyTo(stream);
                            }

                            _context.Usuarios.Update(usuario);
                            await _context.SaveChangesAsync();
                        }

                        if (usuario.RolId == 4)
                        {
                            ViewBag.Success = "Usuario registrado exitosamente! Recibirá en su correo la contraseña para ingresar";

                            utilidades.EnciarCorreo(usuario.Email,
                                "Club La Ribera - Alta de Usuario",
                                "<h2>Gracias por registrarte " + usuario.Apellido + " " + usuario.Nombre + "!!</h2>" +
                                "<p>Recuerda modificar la contraseña cuando ingreses.</p>" +
                                "<br />" +
                                "<p>Tu contraseña es: 4321");
                        }
                        else
                        {
                            ViewBag.Success = "Usuario registrado exitosamente! Una vez que te aprueben, reciviras un mail con tu contraseña";
                        }

                        return PartialView("_RegistroModal", usuario);
                    }
                }
                else
                {
                    return PartialView("_RegistroModal", usuario);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.StackTrate = ex.StackTrace;
                return PartialView("_RegistroModal", usuario);
            }
        }

        [AllowAnonymous]
        public ActionResult RecuperarPassModal()
        {
            return PartialView("_RecuperarClaveModal", new RecuperarClaveView());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RecuperarPass(RecuperarClaveView recuperar)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuario = _context.Usuarios.FirstOrDefault(x => x.Email == recuperar.Email);

                    if (usuario != null)
                    {
                        ViewBag.Success = "Recibirá en su correo la contraseña para ingresar.";

                        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: "4321",
                        salt: System.Text.Encoding.ASCII.GetBytes("Salt"),
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 1000,
                        numBytesRequested: 256 / 8));

                        usuario.Clave = hashed;

                        _context.Usuarios.Update(usuario);
                        await _context.SaveChangesAsync();

                        utilidades.EnciarCorreo(usuario.Email,
                            "Club La Ribera - Blanqueo de clave",
                            "<h2>Recuperación de clave para " + usuario.Apellido + " " + usuario.Nombre + "</h2>" +
                            "<p>Recuerda modificar la contraseña cuando ingreses.</p>" +
                            "<br />" +
                            "<p>Tu contraseña es: 4321");
                    }
                    else
                    {
                        ViewBag.Error = "No existe un usuario con este email";
                    }

                    return PartialView("_RecuperarClaveModal", recuperar);
                }
                else
                {
                    return PartialView("_RecuperarClaveModal", new RecuperarClaveView());
                }
                
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                TempData["StackTrace"] = ex.StackTrace;
                return RedirectToAction("Login");
            }
        }
    }
}
