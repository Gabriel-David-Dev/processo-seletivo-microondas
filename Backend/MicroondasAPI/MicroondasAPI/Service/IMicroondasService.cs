using MicroondasAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroondasAPI.Services
{
    public interface IMicroondasService
    {
        Task<IActionResult> Ligar(int tempo, int potencia = 10, bool programaPreDefinido = false); // Liga o micro-ondas
        void Desligar(); // Desliga o micro-ondas
        Microondas ObterEstado(); // Retorna o estado atual do micro-ondas
        void PassarTempo(); // Simula a passagem de 1 segundo
        Task<IActionResult> AtualizarTempo(int novoTempo);
        IActionResult PausarCancelar(); // Pausa e cancela micro-ondas

        // Programas predefinidos
        Task<IActionResult> LigarComPrograma(string nomePrograma);
        List<ProgramaPreDefinido> ObterProgramasPreDefinidos();
        // Programas customizados
        IActionResult CadastrarProgramaCustomizado(ProgramaCustomizadoDto programaDto); // Adicione esta linha
    }
}
