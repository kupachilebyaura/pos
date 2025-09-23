import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Customer {
  id: number;
  name: string;
  email?: string;
  phone?: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private apiUrl = 'http://localhost:5000/api/customer';

  constructor(private http: HttpClient) {}

  getPaged(page: number, pageSize: number, search: string) {
    const params = { page, pageSize, search };
    return this.http.get<{ total: number, customers: Customer[] }>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Customer> {
    return this.http.get<Customer>(`${this.apiUrl}/${id}`);
  }

  create(data: any): Observable<Customer> {
    return this.http.post<Customer>(this.apiUrl, data);
  }

  update(id: number, data: any): Observable<Customer> {
    return this.http.put<Customer>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}