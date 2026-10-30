using ContractManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Services;

public class ContratoRenovacaoService(ApplicationDbContext context, TimeProvider clock)
{
    public async Task RenovarContratosAutomaticamenteAsync()
    {
        var today = clock.GetUtcNow().UtcDateTime.Date;
        var horizon = today.AddDays(7);
        var contracts = await context.Contratos
            .Where(c => c.RenovacaoAutomatica && c.Status == "ativo" && c.DataFim >= today && c.DataFim <= horizon)
            .ToListAsync();
        foreach (var contract in contracts)
        {
            contract.DataInicio = contract.DataFim;
            contract.DataFim = contract.DataFim.AddYears(1);
        }
        await context.SaveChangesAsync();
    }
}
