namespace ContractManagement.Models
{
    public class Contrato
    {
        public int Id { get; set; }
        public string Empresa { get; set; } = "";
        public string ParteEnvolvida { get; set; } = "";
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public decimal Valor { get; set; }
        public string Periodicidade { get; set; } = "";
        public string Status { get; set; } = "ativo";
        public string Documento { get; set; } = "";
        public bool RenovacaoAutomatica { get; set; }
    }
}
