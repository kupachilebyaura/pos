import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
  code?: string;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = 'http://localhost:5000/api/product';

  constructor(private http: HttpClient) {}

  getPaged(page: number, pageSize: number, search: string) {
    const params = { page, pageSize, search };
    return this.http.get<{ total: number, products: Product[] }>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  createWithImageUrl(data: any): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, data);
  }

  updateWithImageUrl(id: number, data: any): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}