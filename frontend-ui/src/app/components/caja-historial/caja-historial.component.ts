import { Component, OnInit } from '@angular/core';
import { CashHistoryService, CashSessionHistory } from '../../services/cash-history.service';

@Component({
  selector: 'app-caja-historial',
  templateUrl: './caja-historial.component.html'
})
export class CajaHistorialComponent implements OnInit {
  from: string = '';
  to: string = '';
  userId: number | null = null;
  page: number = 1;
  pageSize: number = 20;
  total: number = 0;
  sessions: CashSessionHistory[] = [];
  loading = false;
  errorMsg: string | null = null;

  constructor(private historyService: CashHistoryService) {}

  ngOnInit() {
    this.buscar();
  }

  buscar() {
    this.loading = true;
    this.errorMsg = null;
    this.historyService.getHistory({
      from: this.from,
      to: this.to,
      userId: this.userId || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: res => {
        this.sessions = res.data;
        this.total = res.total;
        this.loading = false;
      },
      error: () => {
        this.errorMsg = 'Error al cargar el historial de arqueos';
        this.loading = false;
      }
    });
  }

  setPage(page: number) {
    this.page = page;
    this.buscar();
  }
}