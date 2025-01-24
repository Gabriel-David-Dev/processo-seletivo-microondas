using MicroondasAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace MicroondasAPI.Services
{
    public class MicroondasService : IMicroondasService
    {
        private Microondas _microondas;
        private System.Timers.Timer _timer;
        private ReadOnlyCollection<ProgramaPreDefinido> _programasPreDefinidos;
        private List<ProgramaPreDefinido> _programas = new List<ProgramaPreDefinido>();
        private HashSet<string> _atalhosUsados = new HashSet<string>(); // Campo para atalhos usados
        private readonly string _jsonFilePath;

        private void ValidarAtalhos()
        {
            foreach (var programa in _programas)
            {
                if (string.IsNullOrEmpty(programa.Aquecimento))
                {
                    throw new ArgumentException($"O programa '{programa.Nome}' deve ter um atalho (Aquecimento) definido.");
                }

                if (programa.Aquecimento == ".")
                {
                    throw new ArgumentException($"O atalho (Aquecimento) do programa '{programa.Nome}' não pode ser '.' (ponto).");
                }

                if (_atalhosUsados.Contains(programa.Aquecimento.ToUpper()))
                {
                    throw new ArgumentException($"O atalho (Aquecimento) '{programa.Aquecimento}' do programa '{programa.Nome}' já está em uso.");
                }

                _atalhosUsados.Add(programa.Aquecimento.ToUpper());
            }
        }

        public MicroondasService()
        {
            _microondas = new Microondas();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (sender, args) => PassarTempo();
            _atalhosUsados = new HashSet<string>();

            // Obtém o diretório da aplicação CORRETAMENTE
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _jsonFilePath = Path.Combine(appDirectory, "programas_customizados.json");

            // Carrega os programas DO JSON (se existir)
            CarregarProgramasDoJson();

            //Inicializa os programas predefinidos APÓS carregar do JSON
            if (_programas == null || _programas.Count == 0) // Verifica se o JSON está vazio ou não existe
            {
                _programas = new List<ProgramaPreDefinido>()
                {
                    new ProgramaPreDefinido { Nome = "Pipoca", Alimento = "Pipoca (de micro-ondas)", TempoSegundos = 180, Potencia = 7, Instrucoes = "Observar o barulho...", Aquecimento = "P", IsCustomizado = false },
                    new ProgramaPreDefinido { Nome = "Leite", Alimento = "Leite", TempoSegundos = 300, Potencia = 5, Instrucoes = "Cuidado com aquecimento...", Aquecimento = "L", IsCustomizado = false },
                    new ProgramaPreDefinido { Nome = "Carnes de boi", Alimento = "Carne em pedaço...", TempoSegundos = 840, Potencia = 4, Instrucoes = "Interrompa o processo...", Aquecimento = "C", IsCustomizado = false },
                    new ProgramaPreDefinido { Nome = "Frango", Alimento = "Frango (qualquer corte)", TempoSegundos = 480, Potencia = 7, Instrucoes = "Interrompa o processo...", Aquecimento = "F", IsCustomizado = false },
                    new ProgramaPreDefinido { Nome = "Feijão", Alimento = "Feijão congelado", TempoSegundos = 480, Potencia = 9, Instrucoes = "Deixe o recipiente destampado...", Aquecimento = "J" , IsCustomizado = false}
                };
            }
            ValidarAtalhos();
            _programasPreDefinidos = new ReadOnlyCollection<ProgramaPreDefinido>(_programas);
        }

        private void CarregarProgramasDoJson()
        {
            if (File.Exists(_jsonFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_jsonFilePath);
                    _programas = JsonSerializer.Deserialize<List<ProgramaPreDefinido>>(json);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Erro ao carregar JSON: {ex.Message}");
                    _programas = new List<ProgramaPreDefinido>();
                }
            }
            else
            {
                _programas = new List<ProgramaPreDefinido>(); // Inicializa uma lista vazia se o arquivo não existir
            }
        }

        private void SalvarProgramasEmJson()
        {
            var json = JsonSerializer.Serialize(_programas, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_jsonFilePath, json);
        }

        public IActionResult CadastrarProgramaCustomizado(ProgramaCustomizadoDto programaDto)
        {
            if (!ValidarModelo(programaDto))
            {
                return new BadRequestObjectResult(new { message = "Dados inválidos para o programa customizado." });
            }

            // NOVA VALIDAÇÃO: Tempo mínimo de 1 segundo
            if (programaDto.TempoSegundos < 1)
            {
                return new BadRequestObjectResult(new { message = "O tempo mínimo para um programa customizado é de 1 segundo." });
            }

            string atalhoMaiusculo = programaDto.Aquecimento.ToUpper(); // Converte para maiúsculo UMA VEZ

            if (!ValidarAtalho(atalhoMaiusculo)) // Usa a versão em maiúsculo na validação
            {
                return new BadRequestObjectResult(new { message = $"O atalho (Aquecimento) '{programaDto.Aquecimento}' já está em uso ou é inválido." });
            }

            var programa = new ProgramaPreDefinido
            {
                Nome = programaDto.Nome,
                Alimento = programaDto.Alimento,
                TempoSegundos = programaDto.TempoSegundos,
                Potencia = programaDto.Potencia,
                Instrucoes = programaDto.Instrucoes,
                Aquecimento = atalhoMaiusculo, // Armazena a versão em maiúsculo
                IsCustomizado = true
            };

            _programas.Add(programa);
            _atalhosUsados.Add(atalhoMaiusculo); // Adiciona o atalho À LISTA DE ATALHOS USADOS AQUI
            SalvarProgramasEmJson();

            _programasPreDefinidos = new ReadOnlyCollection<ProgramaPreDefinido>(_programas);

            return new OkObjectResult(new { message = "Programa customizado cadastrado com sucesso." });
        }

        private bool ValidarModelo(ProgramaCustomizadoDto programaDto)
        {
            var context = new ValidationContext(programaDto, null, null);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(programaDto, context, results, true);
        }

        private bool ValidarAtalho(string atalho)
        {
            if (string.IsNullOrEmpty(atalho) || atalho == ".")
            {
                return false; // Atalho inválido
            }

            if (_atalhosUsados.Contains(atalho)) // Verifica SEMPRE na lista de atalhos usados
            {
                return false; // Atalho já em uso
            }

            return true; // Atalho válido
        }
        public async Task<IActionResult> Ligar(int tempo, int potencia, bool programaPreDefinido = false)
        {
            try
            {
                _microondas.ExecutandoProgramaPreDefinido = programaPreDefinido;

                if (potencia == 0)
                {
                    potencia = 10;
                }

                if (!programaPreDefinido && (tempo < 1 || tempo > 120))
                {
                    return new BadRequestObjectResult(new { error = "O tempo deve ser entre 1 segundo e 120 segundos." });
                }

                if (potencia < 1 || potencia > 10)
                {
                    return new BadRequestObjectResult(new { error = "A potência deve ser entre 1 e 10." });
                }

                string tempoFormatado = tempo.ToString();
                if (tempo >= 60 && tempo <= 100)
                {
                    int minutos = tempo / 60;
                    int segundos = tempo % 60;
                    tempoFormatado = $"{minutos}:{segundos:D2}";
                }

                if (_microondas.EstaLigado)
                {
                    _microondas.TempoSegundos = tempo;
                    _microondas.Potencia = potencia;
                    _microondas.Progresso = GerarProgressoAquecimento(tempo, potencia);
                    return new OkObjectResult(new
                    {
                        message = "Micro-ondas reiniciado com novo tempo e potência.",
                        tempo = tempoFormatado,
                        potencia = _microondas.Potencia,
                        progresso = _microondas.Progresso
                    });
                }

                _microondas.TempoSegundos = tempo;
                _microondas.Potencia = potencia;
                _microondas.EstaLigado = true;
                _microondas.Progresso = GerarProgressoAquecimento(tempo, potencia);
                _timer.Start();

                return new OkObjectResult(new
                {
                    message = $"Micro-ondas ligado",
                    tempo = tempoFormatado,
                    potencia = _microondas.Potencia,
                    progresso = _microondas.Progresso
                });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> AtualizarTempo(int novoTempo)
        {
            try
            {
                if (_microondas.ExecutandoProgramaPreDefinido)
                {
                    return new BadRequestObjectResult(new { error = "Não é possível atualizar o tempo durante a execução de um programa pré-definido." });
                }

                if (!_microondas.EstaLigado)
                {
                    return new BadRequestObjectResult(new { error = "O micro-ondas está desligado. Ligue-o antes de atualizar o tempo." });
                }

                if (_microondas.TempoSegundos > 120) // Verifica se veio de um programa predefinido
                {
                    return new BadRequestObjectResult(new { error = "Não é possível atualizar o tempo durante a execução de um programa pré-definido." });
                }

                int novoTempoComAdicao = _microondas.TempoSegundos + 30;

                if (novoTempoComAdicao > 120)
                {
                    return new BadRequestObjectResult(new { error = "O tempo total não pode ultrapassar 120 segundos." });
                }

                _microondas.TempoSegundos = novoTempoComAdicao;
                _microondas.Progresso = GerarProgressoAquecimento(_microondas.TempoSegundos, _microondas.Potencia);

                return new OkObjectResult(new
                {
                    message = "Tempo atualizado com sucesso.",
                    tempoAtual = _microondas.TempoSegundos,
                    progresso = _microondas.Progresso
                });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { error = ex.Message });
            }
        }

        public IActionResult PausarCancelar()
        {
            bool primeiraPausa = !_microondas.EstaPausado && _microondas.EstaLigado;

            if (!_microondas.EstaLigado)
            {
                return new BadRequestObjectResult(new { message = "O micro-ondas não está em execução." });
            }

            if (_microondas.EstaPausado)
            {
                Desligar();
                return new OkObjectResult(new { message = "Micro-ondas cancelado." });
            }
            else if (primeiraPausa)
            {
                _microondas.EstaPausado = true;
                _timer.Stop();
                return new OkObjectResult(new { message = "Micro-ondas pausado." });
            }

            return new BadRequestObjectResult(new { message = "Não é possível pausar neste momento." });
        }

        public async Task<IActionResult> LigarComPrograma(string nomePrograma)
        {
            var programa = _programasPreDefinidos.FirstOrDefault(p => p.Nome == nomePrograma);
            if (programa == null)
            {
                return new BadRequestObjectResult(new { error = "Programa não encontrado." });
            }

            return await Ligar(programa.TempoSegundos, programa.Potencia, true);
        }

        public List<ProgramaPreDefinido> ObterProgramasPreDefinidos()
        {
            return _programasPreDefinidos.ToList();
        }

        public Microondas ObterEstado()
        {
            return _microondas;
        }

        public void PassarTempo()
        {
            if (_microondas.EstaLigado && !_microondas.EstaPausado && _microondas.TempoSegundos > 0)
            {
                _microondas.TempoSegundos--;
                if (_microondas.TempoSegundos == 0)
                {
                    Desligar();
                }
            }
        }

        public void Desligar()
        {
            _microondas.EstaLigado = false;
            _microondas.EstaPausado = false;
            _timer.Stop();
        }

        public string GerarProgressoAquecimento(int tempo, int potencia)
        {
            int pontosPorSegundo = potencia;
            int totalDePontos = tempo * pontosPorSegundo;

            string progresso = string.Join(" ", Enumerable.Range(0, tempo).Select(_ => new string('.', pontosPorSegundo)));

            return progresso;
        }
    }
}