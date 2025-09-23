import { Component } from '@angular/core';
import { CashConsolidatedService, CashConsolidated } from '../../services/cash-consolidated.service';

@Component({
  selector: 'app-caja-consolidado',
  templateUrl: './caja-consolidado.component.html'
})
export class CajaConsolidadoComponent {
  from: string = '';
  to: string = '';
  groupBy: string = 'day';
  data: CashConsolidated[] = [];
  loading = false;
  errorMsg: string | null = null;

  constructor(private service: CashConsolidatedService) {}

  buscar() {
    if (!this.from || !this.to) return;
    this.loading = true;
    this.errorMsg = null;
    this.service.getConsolidated({ from: this.from, to: this.to, groupBy: this.groupBy }).subscribe({
      next: res => {
        this.data = res;
        this.loading = false;
      },
      error: () => {
        this.errorMsg = 'Error al cargar el consolidado';
        this.loading = false;
      }
    });
  }

  descargar(tipo: 'excel' | 'pdf') {
    if (!this.from || !this.to) return;
    const obs = tipo === 'excel'
      ? this.service.descargarExcel(this.from, this.to, this.groupBy)
      : this.service.descargarPdf(this.from, this.to, this.groupBy);

    obs.subscribe(blob => {
      const a = document.createElement('a');
      const objectUrl = URL.createObjectURL(blob);
      a.href = objectUrl;
      a.download = tipo === 'excel'
        ? `consolidado_arqueos_${this.from}_a_${this.to}.xlsx`
        : `consolidado_arqueos_${this.from}_a_${this.to}.pdf`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(objectUrl);
    });
  }
}