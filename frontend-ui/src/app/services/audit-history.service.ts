import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditLogEntry {
  id: number;
  timestamp: string;
  userId: number;
  userName: string;
  action: string;
  details: string;
}

export interface AuditHistoryResponse {
  data: AuditLogEntry[];
  total: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class AuditHistoryService {
  private apiUrl = '/api/audit/history';

  constructor(private http: HttpClient) {}

  getHistory(params: {
    from?: string;
    to?: string;
    userId?: number;
    action?: string;
    page?: number;
    pageSize?: number;
  }): Observable<AuditHistoryResponse> {
    let httpParams = new HttpParams();
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.userId) httpParams = httpParams.set('userId', params.userId.toString());
    if (params.action) httpParams = httpParams.set('action', params.action);
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<AuditHistoryResponse>(this.apiUrl, { params: httpParams });
  }
}