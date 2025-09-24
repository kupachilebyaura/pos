import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface TopProduct {
  productId: number;
  productName: string;
  totalSold: number;
  totalIncome: number;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  topProducts: TopProduct[] = [];
  loading = false;
  errorMsg: string | null = null;
  chartOptions: any;
  chartReady = false;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getTopProducts();
  }

  getTopProducts() {
    this.loading = true;
    this.http.get<TopProduct[]>('/api/dashboard/top-products').subscribe({
      next: data => {
        this.topProducts = data;
        this.loading = false;
        this.setChart();
      },
      error: () => {
        this.errorMsg = 'Error al cargar datos de productos más vendidos';
        this.loading = false;
      }
    });
  }

  setChart() {
    this.chartOptions = {
      series: [{
        name: 'Cantidad Vendida',
        data: this.topProducts.map(tp => tp.totalSold)
      }],
      chart: {
        type: 'bar',
        height: 350
      },
      xaxis: {
        categories: this.topProducts.map(tp => tp.productName)
      }
    };
    this.chartReady = true;
  }
}