namespace TiendaDeportiva.DTOs
{
    public class ProductoDTO
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }
        public CategoriaDTO Categoria { get; set; } = null!;
    }
}
