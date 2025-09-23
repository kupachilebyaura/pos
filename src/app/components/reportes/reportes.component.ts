import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-reportes',
  templateUrl: './reportes.component.html'
})
export class ReportesComponent {
  fromDate: string;
  toDate: string;
  loading = false;
  errorMsg: string | null = null;

  constructor(private http: HttpClient) {
    const today = new Date();
    this.fromDate = today.toISOString().substring(0, 10);
    this.toDate = today.toISOString().substring(0, 10);
  }

  downloadExcel() {
    this.downloadFile('excel');
  }

  downloadPdf() {
    this.downloadFile('pdf');
  }

  downloadFile(type: 'excel' | 'pdf') {
    this.errorMsg = null;
    this.loading = true;
    const url = type === 'excel'
      ? `/api/report/sales-excel?from=${this.fromDate}&to=${this.toDate}`
      : `/api/report/sales-pdf?from=${this.fromDate}&to=${this.toDate}`;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: blob => {
        const a = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        a.href = objectUrl;
        a.download = type === 'excel'
          ? `reporte_ventas_${this.fromDate}_a_${this.toDate}.xlsx`
          : `reporte_ventas_${this.fromDate}_a_${this.toDate}.pdf`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(objectUrl);
        this.loading = false;
      },
      error: err => {
        this.errorMsg = 'Error al descargar el reporte';
        this.loading = false;
      }
    });
  }
}