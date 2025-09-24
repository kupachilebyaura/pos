import { Component, OnInit } from '@angular/core';
import { Sale, SaleService } from '../../services/sale.service';

@Component({
  selector: 'app-sale-history',
  templateUrl: './sale-history.component.html'
})
export class SaleHistoryComponent implements OnInit {
  sales: Sale[] = [];
  page = 1;
  pageSize = 10;
  total = 0;

  constructor(private saleService: SaleService) {}

  ngOnInit() {
    this.loadSales();
  }

  loadSales() {
    this.saleService.getSales(this.page, this.pageSize).subscribe(res => {
      this.sales = res.sales;
      this.total = res.total;
    });
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadSales();
  }
}