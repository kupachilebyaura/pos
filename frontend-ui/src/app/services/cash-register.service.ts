import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class CashRegisterService {
  private apiUrl = '/api/cashregister';

  constructor(private http: HttpClient) {}

  open(initialAmount: number) {
    return this.http.post(`${this.apiUrl}/open`, initialAmount);
  }

  close(finalAmount: number) {
    return this.http.post(`${this.apiUrl}/close`, finalAmount);
  }

  registerMovement(movement: { type: string; amount: number; description?: string }) {
    return this.http.post(`${this.apiUrl}/movement`, movement);
  }

  getCurrentSession() {
    return this.http.get(`${this.apiUrl}/session`);
  }
}