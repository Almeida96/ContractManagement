using System;
using System.Linq;
using System.Threading.Tasks;
using ContractManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Services
{
    public class ContratoRenovacaoService
    {
        private readonly ApplicationDbContext _context;

        public ContratoRenovacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método que renova contratos com renovação automática
        public async Task RenovarContratosAutomaticamenteAsync()
        {
            var today = DateTime.Now;

            // Procurar contratos que expiram nos próximos 7 dias e têm renovação automática habilitada
            var contratosParaRenovar = await _context.Contratos
                .Where(c => c.RenovacaoAutomatica && c.DataFim <= today.AddDays(7))
                .ToListAsync();

            foreach (var contrato in contratosParaRenovar)
            {
                // Aqui, você pode definir as regras de renovação, como estender a data de término por 1 ano
                contrato.DataInicio = contrato.DataFim;  // A data de início agora é a data de fim anterior
                contrato.DataFim = contrato.DataFim.AddYears(1);  // Extensão de 1 ano, por exemplo

                // Atualizar o status para ativo novamente, se necessário
                contrato.Status = "ativo";
            }

            // Salvar as alterações no banco de dados
            await _context.SaveChangesAsync();
        }
    }
}
