using Microsoft.AspNetCore.Authorization; // Importa para proteger com [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractManagement.Data;
using ContractManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContractManagement.Controllers
{
    [Authorize] // Protege todo o controlador com autenticação JWT
    [Route("api/[controller]")]
    [ApiController]
    public class ContratosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContratosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/contratos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contrato>>> GetContratos()
        {
            // Retorna a lista de contratos do banco de dados
            return await _context.Contratos.ToListAsync();
        }

        // GET: api/contratos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contrato>> GetContrato(int id)
        {
            // Busca o contrato pelo ID
            var contrato = await _context.Contratos.FindAsync(id);

            // Se o contrato não for encontrado, retorna 404
            if (contrato == null)
            {
                return NotFound();
            }

            // Retorna o contrato encontrado
            return contrato;
        }

        // POST: api/contratos
        [HttpPost]
        public async Task<ActionResult<Contrato>> PostContrato(Contrato contrato)
        {
            // Adiciona o novo contrato ao contexto do banco de dados
            _context.Contratos.Add(contrato);
            await _context.SaveChangesAsync();

            // Retorna o contrato criado com o ID gerado
            return CreatedAtAction("GetContrato", new { id = contrato.Id }, contrato);
        }

        // PUT: api/contratos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContrato(int id, Contrato contrato)
        {
            // Verifica se o ID do contrato na URL corresponde ao contrato enviado
            if (id != contrato.Id)
            {
                return BadRequest();
            }

            // Marca o contrato como modificado no contexto
            _context.Entry(contrato).State = EntityState.Modified;

            try
            {
                // Tenta salvar as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se o contrato existe; se não, retorna 404
                if (!ContratoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Retorna 204 No Content ao atualizar com sucesso
            return NoContent();
        }

        // DELETE: api/contratos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContrato(int id)
        {
            // Busca o contrato pelo ID
            var contrato = await _context.Contratos.FindAsync(id);

            // Se o contrato não for encontrado, retorna 404
            if (contrato == null)
            {
                return NotFound();
            }

            // Remove o contrato do contexto do banco de dados
            _context.Contratos.Remove(contrato);
            await _context.SaveChangesAsync();

            // Retorna 204 No Content após a exclusão bem-sucedida
            return NoContent();
        }

        // Método auxiliar para verificar se o contrato existe
        private bool ContratoExists(int id)
        {
            return _context.Contratos.Any(e => e.Id == id);
        }
    }
}
