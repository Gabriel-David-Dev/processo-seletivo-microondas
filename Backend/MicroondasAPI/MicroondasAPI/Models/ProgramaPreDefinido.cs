namespace MicroondasAPI.Models
{
    public class ProgramaPreDefinido
    {
        public string Nome { get; set; }
        public string Alimento { get; set; }
        public int TempoSegundos { get; set; }
        public int Potencia { get; set; }
        public string Instrucoes { get; set; }
        public string Aquecimento { get; set; }
        public bool IsCustomizado { get; set; }
    }
}