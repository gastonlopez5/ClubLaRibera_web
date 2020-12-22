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
                    string foto = null;

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

                        usuario.Estado = true;
                        usuario.RolId = 1;
                        usuario.Clave = hashed;

                        if (usuario.FotoPerfil != null)
                        {
                            foto = usuario.FotoPerfil;
                            usuario.FotoPerfil = "a";
                        }

                        _context.Usuarios.Add(usuario);
                        await _context.SaveChangesAsync();

                        if (usuario.FotoPerfil != null)
                        {
                            var user = _context.Usuarios.FirstOrDefault(x => x.Email == usuario.Email);
                            var fileName = "fotoperfil.png";
                            string wwwPath = environment.WebRootPath;
                            string path = wwwPath + "/fotoperfil/" + user.Id;
                            string filePath = "/fotoperfil/" + user.Id + "/" + fileName;
                            string pathFull = Path.Combine(path, fileName);

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            using (var fileStream = new FileStream(pathFull, FileMode.Create))
                            {
                                var bytes = Convert.FromBase64String(foto);
                                fileStream.Write(bytes, 0, bytes.Length);
                                fileStream.Flush();
                                user.FotoPerfil = filePath;
                            }

                            _context.Usuarios.Update(user);
                            _context.SaveChanges();
                        }

                        ViewBag.Id = "Usuario registrado exitosamente! Ingrese por favor";

                        return RedirectToAction(nameof(Index), "Home");
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
                return RedirectToAction(nameof(Index), "Home");
            }
        }
    }
}
