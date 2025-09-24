import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Role {
  id: number;
  name: string;
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class RoleService {
  private apiUrl = '/api/role';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiUrl);
  }

  create(role: Partial<Role>): Observable<Role> {
    return this.http.post<Role>(this.apiUrl, role);
  }

  update(id: number, role: Partial<Role>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, role);
  }
}