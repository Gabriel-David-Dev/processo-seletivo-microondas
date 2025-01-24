import { MicroondasService } from './../../services/microondas.service';
import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';

interface ProgramaPreDefinido {
  nome: string;
  alimento: string;
  tempoSegundos: number;
  potencia: number;
  instrucoes: string;
  aquecimento: string;
  isCustomizado: boolean;
}

@Component({
    selector: 'app-microondas',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './microondas.component.html',
    styleUrls: ['./microondas.component.css']
})
export class MicroondasComponent implements OnInit, OnDestroy {
    tempo: number = 0;
    tempoExibido: number = 0;
    potencia: number = 0;
    estado: string = 'Desligado';
    stringProcessamento: string = "";
    microondasEmExecucao: boolean = false;
    tempoRestante: number = 0;
    tempoInicial: number = 0;
    estaPausado: boolean = false;
    private intervalo: any;
    programasPreDefinidos: ProgramaPreDefinido[] = [];
    programaForm!: FormGroup;
    modalAberto: boolean = false;

    constructor(private microondasService: MicroondasService, private fb: FormBuilder) { }

    ngOnInit() {
      this.microondasService.obterProgramasPreDefinidos().subscribe(programas => {
          this.programasPreDefinidos = programas;
      });

      this.programaForm = this.fb.group({ // Inicializa o formulário
        nome: ['', Validators.required],
        alimento: ['', Validators.required],
        tempoSegundos: ['', [Validators.required, Validators.min(1)]],
        potencia: ['', [Validators.required, Validators.min(1), Validators.max(10)]],
        instrucoes: [''],
        aquecimento: ['', [Validators.required, Validators.pattern(/^[A-Za-z]$/)]] // Validação de caractere único
    });
    }

    ngOnDestroy() {
        clearInterval(this.intervalo);
    }

    ligarComAtalho(atalho: string) {
      const programa = this.programasPreDefinidos.find(p => p.aquecimento.toUpperCase() === atalho.toUpperCase());
      if (programa) {
        this.tempo = programa.tempoSegundos;
        this.potencia = programa.potencia;
        this.ligarComProgramaBackend(programa.nome, programa.potencia);
      } else {
        alert("Atalho inválido");
      }
    }

    ligarComProgramaBackend(nomePrograma: string, potenciaDoPrograma: number) {
        this.potencia = potenciaDoPrograma;
          this.microondasService.ligarComPrograma(nomePrograma).subscribe({
              next: (response) => {
                  const data = response;
                  if (data && data.tempo !== undefined && data.potencia !== undefined && data.progresso !== undefined) {
                      this.estado = `Tempo restante: ${data.tempo}`;
                      this.contagemRegressiva(data.tempo);
                      this.microondasEmExecucao = true;
                      this.stringProcessamento = data.progresso;
                      this.potencia = data.potencia;
                  } else {
                      alert('Resposta inesperada do servidor');
                  }
              },
              error: (error) => {
                  console.error('Erro ao ligar o micro-ondas', error);
                  alert('Erro ao ligar o micro-ondas');
              }
        });
    }

    pausarCancelar() {
        if (!this.microondasEmExecucao) {
            this.tempo = 0;
            this.potencia =0;
            return;
        }

        clearInterval(this.intervalo);
        this.estaPausado = !this.estaPausado;

        if (this.estaPausado) {
            this.estado = 'Pausado';
            this.microondasService.pausarCancelar().subscribe();
        } else {
            this.estado = 'Cancelado';
            this.microondasEmExecucao = false;
            this.stringProcessamento = "";
            this.tempoRestante = 0;
            this.tempoInicial = 0;
            this.tempo = 0;
            this.potencia = 0;
            this.microondasService.pausarCancelar().subscribe();
        }
    }

    ligar() {
        this.iniciarRapido();

        if (this.potencia == 0 || this.potencia == null) {
          this.potencia = 10;
        }

        let tempoParaUsar = this.tempo;
        const potencia = this.potencia;

        clearInterval(this.intervalo);

        if (this.microondasEmExecucao && !this.estaPausado) {
            tempoParaUsar = this.tempoRestante + 30;
            if (tempoParaUsar > 120) {
                tempoParaUsar = 120;
            }
            this.tempoRestante = tempoParaUsar;
            this.tempoInicial = tempoParaUsar;
            this.microondasService.atualizarTempo(tempoParaUsar).subscribe(response => {
                const data = response;
                if (data && data.tempoAtual !== undefined) {
                    this.contagemRegressiva(data.tempoAtual);
                } else {
                    alert('Resposta inesperada do servidor ao atualizar o tempo');
                }
            }, error => {
                console.error('Erro ao atualizar o tempo', error);
                alert('Erro ao atualizar o tempo.');
            });
            return;
        }

        if (this.microondasEmExecucao && this.estaPausado) {
            this.estaPausado = false;
            this.contagemRegressiva(this.tempoRestante); // Usando tempoRestante!
            this.microondasService.pausarCancelar().subscribe();
            return;
        }

        if (tempoParaUsar < 1 || tempoParaUsar > 120) {
            alert("O tempo deve ser entre 1 segundo e 120 segundos.");
            return;
        }

        if (potencia < 1 || potencia > 10) {
            alert("A potência deve ser entre 1 e 10.");
            return;
        }

        this.tempoInicial = tempoParaUsar;
        this.microondasService.ligar(tempoParaUsar, potencia).subscribe({
            next: (response) => {
                const data = response;
                if (data && data.tempo !== undefined) {
                    this.estado = `Tempo restante: ${data.tempo}`;
                    this.contagemRegressiva(data.tempo);
                    this.microondasEmExecucao = true;
                } else {
                    alert('Resposta inesperada do servidor');
                }
            },
            error: (error) => {
                console.error('Erro ao ligar o micro-ondas', error);
                alert('Erro ao ligar o micro-ondas');
            }
        });
    }

    contagemRegressiva(tempoInicialRecebido: any) {
        clearInterval(this.intervalo);

        let tempoRestanteLocal = tempoInicialRecebido;

        if (typeof tempoInicialRecebido === 'string' && tempoInicialRecebido.includes(':')) {
            const [minutos, segundos] = tempoInicialRecebido.split(':').map(Number);
            tempoRestanteLocal = minutos * 60 + segundos;
        }

        this.tempoRestante = tempoRestanteLocal;

        let stringCompleta = "";
        for (let i = 0; i < tempoRestanteLocal; i++) {
            for (let j = 0; j < this.potencia; j++) {
                stringCompleta += ".";
            }
            if (i < tempoRestanteLocal - 1) {
                stringCompleta += " ";
            }
        }

        this.stringProcessamento = stringCompleta;
        let tempoRestanteContagem = tempoRestanteLocal;

        this.intervalo = setInterval(() => {
            if (tempoRestanteContagem > 0) {
                tempoRestanteContagem--;
                this.tempoRestante = tempoRestanteContagem;

                let novaString = "";
                for (let i = 0; i < tempoRestanteContagem; i++){
                    for (let j = 0; j < this.potencia; j++){
                        novaString += ".";
                    }
                    if (i < tempoRestanteContagem - 1){
                        novaString += " ";
                    }
                }
                this.stringProcessamento = novaString;

                if (tempoRestanteContagem >= 60 && tempoRestanteContagem <= 100) {
                    const minutos = Math.floor(tempoRestanteContagem / 60);
                    const segundos = tempoRestanteContagem % 60;
                    this.estado = `Tempo restante: ${minutos}:${segundos < 10 ? '0' : ''}${segundos} minutos. \n Potência: ${this.potencia} watts.`;
                } else {
                    this.estado = `Tempo restante: ${tempoRestanteContagem} segundos. \n Potência: ${this.potencia} watts.`;
                }
            } else {
                clearInterval(this.intervalo);
                this.estado = 'Aquecimento concluído!';
                this.microondasEmExecucao = false;
                this.estaPausado = false;
                this.stringProcessamento = "";
                this.tempoRestante = 0;
                this.tempoInicial = 0;
            }
        }, 1000);
    }

    @HostListener('window:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        // Verifica se o foco está em um campo de input ou textarea
        const activeElement = document.activeElement;
        if (activeElement && (activeElement.tagName === 'INPUT' || activeElement.tagName === 'TEXTAREA')) {
            return;
        }

        const teclaPressionada = event.key.toUpperCase();
        const programaCorrespondente = this.programasPreDefinidos.find(p => p.aquecimento.toUpperCase() === teclaPressionada);

        if (programaCorrespondente) {
            this.ligarComAtalho(teclaPressionada);
        }
    }

    obterEstado() {
        this.microondasService.obterEstado().subscribe(response => {
            console.log('Estado do micro-ondas', response);
            this.estado = `Tempo restante: ${response.tempoSegundos} segundos | Potência: ${response.potencia}%`;
        }, error => {
            console.error('Erro ao obter o estado', error);
            this.estado = "Erro ao obter o estado";
        });
    }

    cadastrarPrograma() {
      if (this.programaForm.valid) {
          this.microondasService.cadastrarProgramaCustomizado(this.programaForm.value).subscribe({
              next: (response) => {
                  alert(response.message);
                  this.programaForm.reset();
                  this.ngOnInit(); // Recarrega os programas após o cadastro
                  this.fecharModalCadastro();
              },
              error: (error) => {
                  alert(error.error.message);
                  console.error('Erro ao cadastrar programa:', error);
              }
          });
      } else {
          alert("Por favor, preencha todos os campos corretamente.");
      }
    }

    // Metodos mais simples

    abrirModalCadastro() {
        this.modalAberto = true;
      }

    fecharModalCadastro() {
        this.modalAberto = false;
        this.programaForm.reset();
    }

    adicionarTempo(value: number): void {
        this.tempoExibido = this.tempoExibido * 10 + value;
        this.tempo = this.tempoExibido;
    }

    limparTempo(): void {
        this.tempo = 0;
        this.tempoExibido = 0;
    }

    adicionarPotencia(value: number): void {
        this.potencia = this.potencia * 10 + value;
    }

    limparPotencia(): void {
        this.potencia = 0;
    }

    private iniciarRapido(): void {
        if (!this.tempo && !this.potencia) {
            this.tempo = 30;
            this.potencia = 10;
        }
    }

}
