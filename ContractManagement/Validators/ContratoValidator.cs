using FluentValidation;
using ContractManagement.Models;

namespace ContractManagement.Validators;

public class ContratoValidator : AbstractValidator<Contrato>
{
    public ContratoValidator()
    {
        RuleFor(x => x.Empresa).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ParteEnvolvida).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DataInicio).GreaterThanOrEqualTo(new DateTime(1900, 1, 1)).LessThan(x => x.DataFim);
        RuleFor(x => x.DataFim).LessThan(new DateTime(9998, 1, 1));
        RuleFor(x => x.Valor).GreaterThan(0).LessThanOrEqualTo(9999999999.99m).PrecisionScale(12, 2, false);
        RuleFor(x => x.Status).Must(value => new[] { "ativo", "cancelado", "encerrado" }.Contains(value))
            .WithMessage("Status must be ativo, cancelado or encerrado.");
        RuleFor(x => x.Periodicidade).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Documento).MaximumLength(2048);
    }
}
