import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CashSessionHistory {
  id: number;
  openedAt: string;
  closedAt: string;
  initialAmount: number;
  closingAmount: number;
  difference: number;
  user: string;
}

export interface CashHistoryResponse {
  data: CashSessionHistory[];
  total: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class CashHistoryService {
  private apiUrl = '/api/cash/history';

  constructor(private http: HttpClient) {}

  getHistory(params: {
    from?: string;
    to?: string;
    userId?: number;
    page?: number;
    pageSize?: number;
  }): Observable<CashHistoryResponse> {
    let httpParams = new HttpParams();
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.userId) httpParams = httpParams.set('userId', params.userId.toString());
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<CashHistoryResponse>(this.apiUrl, { params: httpParams });
  }
}