import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MicroondasService {
  private baseUrl = 'https://localhost:7230/api/microondas'; // URL do backend

  constructor(private http: HttpClient) { }

  ligar(tempo: number, potencia: number): Observable<any> {
    const url = `https://localhost:7230/api/microondas/ligar?tempo=${tempo}&potencia=${potencia}`;
    return this.http.post<any>(url, {}); // O retorno é um Observable do tipo 'any'
  }


  desligar(): Observable<any> {
    return this.http.post(`${this.baseUrl}/desligar`, null);
  }

  obterEstado(): Observable<any> {
    return this.http.get(`${this.baseUrl}/estado`);
  }

  pausarCancelar(): Observable<any> {
    return this.http.post(`${this.baseUrl}/pausar-cancelar`, null);
  }

  atualizarTempo(novoTempo: number): Observable<any> {
    const params = new HttpParams().set('novoTempo', novoTempo.toString()); // Cria os parâmetros da query
    return this.http.put(`${this.baseUrl}/atualizar-tempo`, {params});
  }

  // Predefinidos
  obterProgramasPreDefinidos(): Observable<any> {
    return this.http.get(`${this.baseUrl}/programas-pre-definidos`);
  }

  ligarComPrograma(nomePrograma: string): Observable<any> {
      const params = new HttpParams().set('nomePrograma', nomePrograma);
      return this.http.post(`${this.baseUrl}/ligar-programa`, {}, { params });
  }

  cadastrarProgramaCustomizado(programa: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/cadastrar-programa`, programa);
  }
}
