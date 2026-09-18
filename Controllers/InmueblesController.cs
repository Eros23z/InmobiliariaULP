using InmobiliariaULP.Models;
using InmobiliariaULP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InmobiliariaULP.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly IRepositorioInmueble _repoInmueble;
        private readonly IRepositorioPropietario _repoPropietario;
        private readonly IRepositorioTipoInmueble _repoTipoInmueble;
        private readonly IRepositorioImagenInmueble _repoImagen;
        private readonly IWebHostEnvironment _environment;

        public InmueblesController(
            IRepositorioInmueble repoInmueble,
            IRepositorioPropietario repoPropietario,
            IRepositorioTipoInmueble repoTipoInmueble,
            IRepositorioImagenInmueble repoImagen,
            IWebHostEnvironment environment)
        {
            _repoInmueble = repoInmueble;
            _repoPropietario = repoPropietario;
            _repoTipoInmueble = repoTipoInmueble;
            _repoImagen = repoImagen;
            _environment = environment;
        }

        private void CargarListasDesplegables(int? idPropietario = null, int? idTipo = null)
        {
            var propietarios = _repoPropietario.ObtenerTodos(null, 1, 500)
                .Where(p => p.Estado)
                .OrderBy(p => p.Apellido);

            var tipos = _repoTipoInmueble.ObtenerTodos()
                .OrderBy(t => t.Descripcion);

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "NombreCompleto", idPropietario);
            ViewBag.IdTipoInmueble = new SelectList(tipos, "IdTipoInmueble", "Descripcion", idTipo);
        }

        // GET: Inmuebles
        public IActionResult Index(string search, int? tipoId, bool? soloDisponibles, int page = 1, int pageSize = 10)
        {
            var items = _repoInmueble.ObtenerTodos(search, tipoId, soloDisponibles);

            int totalItems = items.Count;
            var paginados = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.TipoId = tipoId;
            ViewBag.SoloDisponibles = soloDisponibles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TiposFiltro = new SelectList(_repoTipoInmueble.ObtenerTodos(), "IdTipoInmueble", "Descripcion", tipoId);

            return View(paginados);
        }

        // GET: Inmuebles/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = _repoInmueble.ObtenerPorId(id.Value);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View();
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible,ImagenPortada,IdPropietario,IdTipoInmueble")] Inmueble inmueble, IFormFile? imagenPortadaFile)
        {
            if (ModelState.IsValid)
            {
                if (imagenPortadaFile != null)
                {
                    string? rutaRelativa = GuardarArchivo(imagenPortadaFile, "inmuebles");
                    if (rutaRelativa != null)
                    {
                        inmueble.ImagenPortada = rutaRelativa;
            }
                }

                _repoInmueble.Alta(inmueble);
                TempData["Success"] = "Inmueble registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = _repoInmueble.ObtenerPorId(id.Value);
            if (inmueble == null) return NotFound();

            CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("IdInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible,ImagenPortada,IdPropietario,IdTipoInmueble")] Inmueble inmueble, IFormFile? imagenPortadaFile)
        {
            if (id != inmueble.IdInmueble) return NotFound(); 

            if (ModelState.IsValid)
            {
                if (imagenPortadaFile != null)
                {
                    string? nuevaRuta = GuardarArchivo(imagenPortadaFile, "inmuebles");
                    if (nuevaRuta != null)
                    {
                        if (!string.IsNullOrEmpty(inmueble.ImagenPortada) && inmueble.ImagenPortada.StartsWith("/uploads/")) 
                {
                            string rutaVieja = Path.Combine(_environment.WebRootPath, inmueble.ImagenPortada.TrimStart('/')); 
                    if (System.IO.File.Exists(rutaVieja)) System.IO.File.Delete(rutaVieja);
                        }
                        inmueble.ImagenPortada = nuevaRuta; 
            }
                }

                _repoInmueble.Modificacion(inmueble);
                TempData["Success"] = "Inmueble actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            CargarListasDesplegables(inmueble.IdPropietario, inmueble.IdTipoInmueble); 
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = _repoInmueble.ObtenerPorId(id.Value);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repoInmueble.Baja(id);
                TempData["Success"] = "Inmueble eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se puede eliminar el inmueble porque registra reservas asociadas. Se recomienda suspender su disponibilidad.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Endpoint AJAX
        [HttpGet]
        public IActionResult ObtenerPrecio(int id)
        {
            var inmueble = _repoInmueble.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return Json(new { precio = inmueble.PrecioPorDia, porcentaje = inmueble.PorcentajeReserva });
        }

        private string? GuardarArchivo(IFormFile? archivo, string subCarpeta)
        {
            if (archivo == null || archivo.Length == 0) return null;

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!extensionesPermitidas.Contains(extension)) return null;

            string carpetaDestino = Path.Combine(_environment.WebRootPath, "uploads", subCarpeta);
            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }

            string nombreUnico = $"{Guid.NewGuid()}{extension}";
            string rutaFisica = Path.Combine(carpetaDestino, nombreUnico);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            return $"/uploads/{subCarpeta}/{nombreUnico}";
        }

        // GET: Inmuebles/Galeria/5
        public IActionResult Galeria(int id)
        {
            var inmueble = _repoInmueble.ObtenerPorId(id);
            if (inmueble == null) return NotFound();

            ViewBag.Inmueble = inmueble;
            var imagenes = _repoImagen.ObtenerPorInmueble(id);
            return View(imagenes);
        }

        // POST: Inmuebles/SubirImagenes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubirImagenes(int idInmueble, List<IFormFile> fotos)
        {
            if (fotos != null && fotos.Count > 0)
            {
                int subidas = 0;
                foreach (var foto in fotos)
                {
                    string? ruta = GuardarArchivo(foto, "inmuebles");
                    if (ruta != null)
                    {
                        _repoImagen.Alta(new ImagenInmueble
                        {
                            IdInmueble = idInmueble,
                            Url = ruta
                        });
                        subidas++;
                    }
                }
                TempData["Success"] = $"Se agregaron {subidas} imágenes a la galería.";
            }
            return RedirectToAction(nameof(Galeria), new { id = idInmueble });
        }

        // POST: Inmuebles/EliminarImagen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarImagen(int idImagen, int idInmueble)
        {
            var img = _repoImagen.ObtenerPorId(idImagen);
            if (img != null)
            {
                if (img.Url.StartsWith("/uploads/"))
                {
                    string rutaFisica = Path.Combine(_environment.WebRootPath, img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(rutaFisica)) System.IO.File.Delete(rutaFisica);
                }
                _repoImagen.Borrar(idImagen);
                TempData["Success"] = "Imagen eliminada de la galería.";
            }
            return RedirectToAction(nameof(Galeria), new { id = idInmueble });
        }
    }
}