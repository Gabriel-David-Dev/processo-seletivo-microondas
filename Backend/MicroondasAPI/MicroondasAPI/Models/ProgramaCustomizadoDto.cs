using System.ComponentModel.DataAnnotations;

namespace MicroondasAPI.Models // Ajuste o namespace para o seu projeto
{
    public class ProgramaCustomizadoDto
    {
        [Required(ErrorMessage = "O nome do programa é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O alimento é obrigatório.")]
        public string Alimento { get; set; }

        [Required(ErrorMessage = "O tempo é obrigatório.")]
        public int TempoSegundos { get; set; }

        [Required(ErrorMessage = "A potência é obrigatória.")]
        [Range(1, 10, ErrorMessage = "A potência deve estar entre 1 e 10.")]
        public int Potencia { get; set; }

        public string? Instrucoes { get; set; } // Opcional

        [Required(ErrorMessage = "O caractere de aquecimento é obrigatório.")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "O caractere de aquecimento deve conter apenas letras.")]
        [StringLength(1, ErrorMessage = "O caractere de aquecimento deve conter apenas uma letra.")]
        public string Aquecimento { get; set; }

        
    }
}