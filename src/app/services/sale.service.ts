import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SaleDetail {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  subtotal: number;
}

export interface Sale {
  id: number;
  date: string;
  total: number;
  paymentMethod: string;
  customerName?: string;
  details: SaleDetail[];
}

@Injectable({ providedIn: 'root' })
export class SaleService {
  private apiUrl = 'http://localhost:5000/api/sale';

  constructor(private http: HttpClient) {}

  registerSale(items: any[], customerId: number | null, paymentMethod: string): Observable<any> {
    return this.http.post(this.apiUrl, {
      items,
      customerId,
      paymentMethod
    });
  }

  getSales(page: number, pageSize: number) {
    const params = { page, pageSize };
    return this.http.get<{ total: number; sales: Sale[] }>(this.apiUrl, { params });
  }
}