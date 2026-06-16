using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class TipoDocumentoRepository : ITipoDocumentoRepository
    {
        private readonly AppDbContext _context;

        public TipoDocumentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoDocumento>> ObtenerTodosAsync()
        {
            return await _context.TiposDocumento
                .OrderBy(t => t.Descripcion)
                .ToListAsync();
        }
    }
}
