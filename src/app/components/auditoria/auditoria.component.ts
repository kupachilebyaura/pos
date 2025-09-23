import { Component, OnInit } from '@angular/core';
import { AuditHistoryService, AuditLogEntry } from '../../services/audit-history.service';

@Component({
  selector: 'app-auditoria',
  templateUrl: './auditoria.component.html'
})
export class AuditoriaComponent implements OnInit {
  from: string = '';
  to: string = '';
  userId: number | null = null;
  action: string = '';
  page: number = 1;
  pageSize: number = 20;
  total: number = 0;
  logs: AuditLogEntry[] = [];
  loading = false;
  errorMsg: string | null = null;

  constructor(private auditService: AuditHistoryService) {}

  ngOnInit() {
    this.buscar();
  }

  buscar() {
    this.loading = true;
    this.errorMsg = null;
    this.auditService.getHistory({
      from: this.from,
      to: this.to,
      userId: this.userId || undefined,
      action: this.action || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: res => {
        this.logs = res.data;
        this.total = res.total;
        this.loading = false;
      },
      error: () => {
        this.errorMsg = 'Error al cargar el historial de auditoría';
        this.loading = false;
      }
    });
  }

  setPage(page: number) {
    this.page = page;
    this.buscar();
  }
}