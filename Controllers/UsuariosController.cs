using CLubLaRibera_Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginView loginView)
        {
            try
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: loginView.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes("Salt"),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                var p = _context.Usuarios.FirstOrDefault(x => x.Email == loginView.Email);
                if (p == null || p.Clave != hashed)
                {
                    ViewBag.Mensaje = "Email o Contraseña incorrectos";
                    return View();
                }
                else
                {
                    var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, p.Email),
                        new Claim(ClaimTypes.Role, p.TipoUsuario.Rol),
                    };

                    var token = new JwtSecurityToken(
                        issuer: config["TokenAuthentication:Issuer"],
                        audience: config["TokenAuthentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(60),
                        signingCredentials: credenciales
                    );
                    return RedirectToAction(nameof(Index), "Home", new JwtSecurityTokenHandler().WriteToken(token));
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.StackTrate = ex.StackTrace;
                return View();
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

                        /*
                        _context.Usuarios.Add(usuario);
                        await _context.SaveChangesAsync();
                        */

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
    }
}
