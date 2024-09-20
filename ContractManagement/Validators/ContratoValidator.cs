using FluentValidation;
using ContractManagement.Models; // Certifique-se de ajustar o namespace conforme o seu projeto

namespace ContractManagement.Validators // Certifique-se de usar o namespace correto do seu projeto
{
    public class ContratoValidator : AbstractValidator<Contrato>
    {
        public ContratoValidator()
        {
            RuleFor(x => x.Empresa)
                .NotEmpty().WithMessage("A empresa é obrigatória.");

            RuleFor(x => x.ParteEnvolvida)
                .NotEmpty().WithMessage("A parte envolvida é obrigatória.");

            RuleFor(x => x.DataInicio)
                .LessThan(x => x.DataFim)
                .WithMessage("A data de início deve ser anterior à data de término.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor do contrato deve ser maior que zero.");
        }
    }
}
