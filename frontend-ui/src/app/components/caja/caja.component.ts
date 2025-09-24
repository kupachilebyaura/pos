import { Component, OnInit } from '@angular/core';
import { CashRegisterService } from '../../services/cash-register.service';

@Component({
  selector: 'app-caja',
  templateUrl: './caja.component.html'
})
export class CajaComponent implements OnInit {
  session: any = null;
  initialAmount: number = 0;
  finalAmount: number = 0;
  movementType: string = '';
  movementAmount: number = 0;
  movementDescription: string = '';
  movimientoMsg: string | null = null;
  movimientoErr: string | null = null;
  openMsg: string | null = null;
  closeMsg: string | null = null;
  errorMsg: string | null = null;
  arqueoDisponible = false;
  ultimoArqueoSessionId: number | null = null;

  constructor(private cashService: CashRegisterService) {}

  ngOnInit() {
    this.loadSession();
  }

  loadSession() {
    this.cashService.getCurrentSession().subscribe({
      next: s => {
        this.session = s;
        if (s && s.id) {
          this.ultimoArqueoSessionId = s.id;
        }
      },
      error: () => this.session = null
    });
  }

  abrirCaja() {
    this.cashService.open(this.initialAmount).subscribe({
      next: () => {
        this.openMsg = 'Caja abierta correctamente.';
        this.loadSession();
      },
      error: err => {
        this.errorMsg = err.error?.message || 'Error al abrir caja.';
      }
    });
  }

  cerrarCaja() {
    this.cashService.close(this.finalAmount).subscribe({
      next: res => {
        this.closeMsg = `Caja cerrada. Diferencia: $${res.difference}. Esperado: $${res.esperado}`;
        this.arqueoDisponible = true;
        this.ultimoArqueoSessionId = res.id;
        this.session = null;
      },
      error: err => {
        this.errorMsg = err.error?.message || 'Error al cerrar caja.';
      }
    });
  }

  registrarMovimiento() {
    this.movimientoMsg = null;
    this.movimientoErr = null;
    this.cashService.registerMovement({
      type: this.movementType,
      amount: this.movementAmount,
      description: this.movementDescription,
    }).subscribe({
      next: () => {
        this.movimientoMsg = 'Movimiento registrado correctamente.';
        this.loadSession();
        this.movementAmount = 0;
        this.movementDescription = '';
      },
      error: err => {
        this.movimientoErr = err.error?.message || 'Error al registrar movimiento.';
      }
    });
  }

  descargarArqueo(tipo: 'excel' | 'pdf') {
    if (!this.ultimoArqueoSessionId) return;
    const url = tipo === 'excel'
      ? `/api/cash/arqueo-excel?sessionId=${this.ultimoArqueoSessionId}`
      : `/api/cash/arqueo-pdf?sessionId=${this.ultimoArqueoSessionId}`;
    fetch(url, { credentials: 'include' })
      .then(resp => resp.blob())
      .then(blob => {
        const a = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        a.href = objectUrl;
        a.download = tipo === 'excel'
          ? `arqueo_caja_${this.ultimoArqueoSessionId}.xlsx`
          : `arqueo_caja_${this.ultimoArqueoSessionId}.pdf`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(objectUrl);
      });
  }
}