import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface ProductReport {
  productId: number;
  productName: string;
  totalSold: number;
  totalIncome: number;
}

@Component({
  selector: 'app-reportes-por-producto',
  templateUrl: './reportes-por-producto.component.html'
})
export class ReportesPorProductoComponent {
  fromDate: string;
  toDate: string;
  loading = false;
  errorMsg: string | null = null;
  reportData: ProductReport[] = [];

  constructor(private http: HttpClient) {
    const today = new Date();
    this.fromDate = today.toISOString().substring(0, 10);
    this.toDate = today.toISOString().substring(0, 10);
  }

  consultar() {
    this.errorMsg = null;
    this.loading = true;
    this.http.get<ProductReport[]>(`/api/report/sales-by-product?from=${this.fromDate}&to=${this.toDate}`).subscribe({
      next: data => {
        this.reportData = data;
        this.loading = false;
      },
      error: () => {
        this.errorMsg = 'Error al consultar el reporte';
        this.loading = false;
      }
    });
  }

  descargarExcel() {
    this.descargarArchivo('excel');
  }

  descargarPdf() {
    this.descargarArchivo('pdf');
  }

  private descargarArchivo(tipo: 'excel' | 'pdf') {
    this.errorMsg = null;
    this.loading = true;
    const url = tipo === 'excel'
      ? `/api/report/sales-by-product-excel?from=${this.fromDate}&to=${this.toDate}`
      : `/api/report/sales-by-product-pdf?from=${this.fromDate}&to=${this.toDate}`;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: blob => {
        const a = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        a.href = objectUrl;
        a.download = tipo === 'excel'
          ? `ventas_por_producto_${this.fromDate}_a_${this.toDate}.xlsx`
          : `ventas_por_producto_${this.fromDate}_a_${this.toDate}.pdf`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(objectUrl);
        this.loading = false;
      },
      error: () => {
        this.errorMsg = 'Error al descargar el reporte';
        this.loading = false;
      }
    });
  }
}