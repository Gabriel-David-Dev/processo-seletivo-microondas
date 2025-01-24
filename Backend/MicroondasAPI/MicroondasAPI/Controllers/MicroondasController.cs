using MicroondasAPI.Services;
using Microsoft.AspNetCore.Mvc;
using MicroondasAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class MicroondasController : ControllerBase
{
    private readonly IMicroondasService _microondasService;

    public MicroondasController(IMicroondasService microondasService)
    {
        _microondasService = microondasService;
    }

    [HttpPost("ligar")]
    public async Task<IActionResult> Ligar([FromQuery] int? tempo, [FromQuery] int? potencia)
    {
        try
        {
            // Caso ambos os parâmetros sejam nulos, aplica o início rápido (30 segundos e 10 de potência)
            if (!tempo.HasValue && !potencia.HasValue)
            {
                tempo = 30;  // 30 segundos por padrão para início rápido
                potencia = 10;  // 10 de potência por padrão
            }

            // Validação de tempo
            if ((tempo.HasValue && (tempo < 1 || tempo > 120) || tempo == null))
            {
                return BadRequest("O tempo deve ser entre 1 segundo e 120 segundos.");
            }

            // Validação de potência
            if (potencia.HasValue && (potencia < 1 || potencia > 10))
            {
                return BadRequest("A potência deve ser entre 1 e 10.");
            }

            if (!potencia.HasValue)
                potencia = 10;  // Valor padrão de 10 de potência se não informado

            // Chama o serviço de ligar micro-ondas com os valores finais
            var result = await _microondasService.Ligar(tempo.Value, potencia.Value);
            return result;  // Retorna o resultado do serviço
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("atualizar-tempo")]
    public async Task<IActionResult> AtualizarTempo([FromQuery] int novoTempo)
    {
        try
        {
            var resultado = await _microondasService.AtualizarTempo(novoTempo);
            return resultado;
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("pausar-cancelar")]
    public IActionResult PausarCancelar()
    {
        var result = _microondasService.PausarCancelar();
        return result;
    }

    //predefinidos
    [HttpPost("ligar-programa")]
    public async Task<IActionResult> LigarComPrograma([FromQuery] string nomePrograma)
    {
        try
        {
            var result = await _microondasService.LigarComPrograma(nomePrograma);
            return result;
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("programas-pre-definidos")]
    public IActionResult ObterProgramasPreDefinidos()
    {
        try
        {
            var result = _microondasService.ObterProgramasPreDefinidos();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cadastrar-programa")]
    public IActionResult CadastrarPrograma(ProgramaCustomizadoDto programa)
    {
        return _microondasService.CadastrarProgramaCustomizado(programa);
    }

}
