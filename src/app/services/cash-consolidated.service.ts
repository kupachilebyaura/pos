import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CashConsolidated {
  group: string;
  cantidadArqueos: number;
  efectivoInicial: number;
  ventasEfectivo: number;
  ingresos: number;
  retiros: number;
  montoFinal: number;
  diferencia: number;
}

@Injectable({ providedIn: 'root' })
export class CashConsolidatedService {
  private apiUrl = '/api/cash';

  constructor(private http: HttpClient) {}

  getConsolidated(params: {
    from: string;
    to: string;
    groupBy?: string;
  }): Observable<CashConsolidated[]> {
    let httpParams = new HttpParams()
      .set('from', params.from)
      .set('to', params.to);
    if (params.groupBy) httpParams = httpParams.set('groupBy', params.groupBy);

    return this.http.get<CashConsolidated[]>(`${this.apiUrl}/consolidated`, { params: httpParams });
  }

  descargarExcel(from: string, to: string, groupBy: string = 'day') {
    return this.http.get(`${this.apiUrl}/consolidated-excel`, {
      params: { from, to, groupBy },
      responseType: 'blob'
    });
  }

  descargarPdf(from: string, to: string, groupBy: string = 'day') {
    return this.http.get(`${this.apiUrl}/consolidated-pdf`, {
      params: { from, to, groupBy },
      responseType: 'blob'
    });
  }
}