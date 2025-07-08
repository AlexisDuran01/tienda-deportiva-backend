using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaDeportiva.Models;
using TiendaDeportiva.DTOs;

namespace TiendaDeportiva.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly TiendaDeportivaContext _context;

        public ProductosController(TiendaDeportivaContext context)
        {
            _context = context;
        }

        // GET: api/Productos 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductos(
            [FromQuery] string? buscar = null,
            [FromQuery] string[]? categorias = null
        )
        {
            // Comenzar con la consulta base
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            // FILTRO DE BÚSQUEDA POR TEXTO
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var searchTerm = buscar.ToLower().Trim();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(searchTerm) ||
                    p.Descripcion.ToLower().Contains(searchTerm) ||
                    p.Categoria.Nombre.ToLower().Contains(searchTerm)
                );
            }

            // FILTRO POR CATEGORÍAS
            if (categorias != null && categorias.Length > 0)
            {
                // Filtrar categorías vacías o nulas
                var categoriasValidas = categorias
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToArray();

                if (categoriasValidas.Length > 0)
                {
                    query = query.Where(p => categoriasValidas.Contains(p.Categoria.Nombre));
                }
            }

            // EJECUTAR LA CONSULTA Y MAPEAR A DTO
            var productos = await query
                .OrderBy(p => p.ProductoId) // Ordenar alfabéticamente
                .Select(p => new ProductoDTO
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl,
                    Categoria = new CategoriaDTO
                    {
                        CategoriaId = p.Categoria.CategoriaId,
                        Nombre = p.Categoria.Nombre
                    }
                })
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/Productos/aleatorios
        [HttpGet("aleatorios")]
        public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductosAleatorios()
        {
            // Incluye la información de la categoría relacionada y genera un orden aleatorio usando Guid.NewGuid()
            // Esto hace que cada vez que se consulte, los productos se ordenen de forma diferente
            var productosAleatorios = await _context.Productos
                .Include(p => p.Categoria) // Incluye la entidad relacionada 'Categoria'
                .OrderBy(p => Guid.NewGuid()) // Ordena aleatoriamente los productos
                .Take(6) // Toma solo los primeros 6 productos del resultado aleatorio
                .Select(p => new ProductoDTO // Proyecta cada producto a un DTO
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl,
                    Categoria = new CategoriaDTO
                    {
                        CategoriaId = p.Categoria.CategoriaId,
                        Nombre = p.Categoria.Nombre
                    }
                })
                .ToListAsync(); // Ejecuta la consulta de forma asíncrona y obtiene la lista

            // Devuelve la lista de productos aleatorios con un código HTTP 200 OK
            return Ok(productosAleatorios);
        }




        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDTO>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.ProductoId == id)
                .Select(p => new ProductoDTO
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl,
                    Categoria = new CategoriaDTO
                    {
                        CategoriaId = p.Categoria.CategoriaId,
                        Nombre = p.Categoria.Nombre
                    }
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound();
            }

            return producto;
        }

        // PUT: api/Productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.ProductoId)
            {
                return BadRequest();
            }

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProducto", new { id = producto.ProductoId }, producto);
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoId == id);
        }
    }
}
