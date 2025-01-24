namespace MicroondasAPI.Models
{
    public class Microondas
    {
        public int TempoSegundos { get; set; } // Tempo configurado em segundos
        public bool EstaLigado { get; set; }  // Estado do micro-ondas
        public int Potencia { get; set; }
        public string Progresso { get; set; } = string.Empty;
        public bool EstaPausado { get; set; }
        public bool ExecutandoProgramaPreDefinido { get; set; }
        public Microondas()
        {
            TempoSegundos = 0;
            EstaLigado = false;
        }
    }
}
