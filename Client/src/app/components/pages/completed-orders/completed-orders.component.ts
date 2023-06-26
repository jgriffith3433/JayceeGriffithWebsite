import { Component, TemplateRef, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CompletedOrderDTO } from '../../../models/CompletedOrderDTO';
import { CompletedOrderProductDTO } from '../../../models/CompletedOrderProductDTO';
import { CreateCompletedOrderCommand } from '../../../models/CreateCompletedOrderCommand';
import { CreateCompletedOrderProductCommand } from '../../../models/CreateCompletedOrderProductCommand';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateCompletedOrderCommand } from '../../../models/UpdateCompletedOrderCommand';
import { UpdateCompletedOrderProductCommand } from '../../../models/UpdateCompletedOrderProductCommand';
import { CompletedOrdersService } from '../../../providers/completed-orders.service';
//CompletedOrdersClient,
//CompletedOrderDTO,
//CompletedOrderProductDTO,
//UnitTypeDTO,
//CreateCompletedOrderCommand,
//UpdateCompletedOrderCommand,
//CreateCompletedOrderProductCommand,
//UpdateCompletedOrderProductCommand

@Component({
  selector: 'app-completed-orders-component',
  templateUrl: './completed-orders.component.html',
  styleUrls: ['./completed-orders.component.scss']
})
export class CompletedOrdersComponent implements OnInit {
  debug = false;
  completedOrders: CompletedOrderDTO[];
  kitchenUnitTypes: UnitTypeDTO[];
  selectedCompletedOrder: CompletedOrderDTO | undefined;
  selectedCompletedOrderProduct: CompletedOrderProductDTO | undefined;
  newCompletedOrderEditor: any = {};
  completedOrderOptionsEditor: any = {};
  completedOrderProductDetailsEditor: any = {};
  newCompletedOrderModalRef: BsModalRef;
  completedOrderOptionsModalRef: BsModalRef;
  deleteCompletedOrderModalRef: BsModalRef;
  completedOrderProductDetailsModalRef: BsModalRef;

  constructor(
    private completedOrdersService: CompletedOrdersService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.completedOrdersService.getAll().subscribe(
      result => {
        this.completedOrders = result.completedOrders || [] as CompletedOrderDTO[];
        if (this.completedOrders.length) {
          this.selectedCompletedOrder = this.completedOrders[0];
        }
      },
      error => console.error(error)
    );
  }

  // Completed Orders
  remainingCompletedOrderProducts(completedOrder: CompletedOrderDTO): number | undefined {
    return completedOrder.completedOrderProducts?.filter(t => !t.walmartId).length;
  }

  showNewCompletedOrderModal(template: TemplateRef<any>): void {
    this.newCompletedOrderModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('name')?.focus(), 250);
  }

  newCompletedOrderCancelled(): void {
    this.newCompletedOrderModalRef.hide();
    this.newCompletedOrderEditor = {};
  }

  addCompletedOrder(): void {
    const completedOrder = {
      id: 0,
      name: this.newCompletedOrderEditor.name,
      userImport: this.newCompletedOrderEditor.userImport,
      completedOrderProducts: []
    } as CompletedOrderDTO;

    this.completedOrdersService.create(completedOrder as CreateCompletedOrderCommand).subscribe(
      result => {
        this.completedOrdersService.get(result).subscribe(
          result => {
            this.completedOrders.push(result);
            this.selectedCompletedOrder = result;
            this.newCompletedOrderModalRef.hide();
            this.newCompletedOrderEditor = {};
          },
          error => console.error(error)
        );
      },
      error => {
        const errors = error.errors;

        if (errors && errors.Title) {
          this.newCompletedOrderEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('name')?.focus(), 250);
      }
    );
  }

  showCompletedOrderOptionsModal(template: TemplateRef<any>) {
    if (this.selectedCompletedOrder) {
      this.completedOrderOptionsEditor = {
        id: this.selectedCompletedOrder.id,
        name: this.selectedCompletedOrder.name,
        userImport: this.selectedCompletedOrder.userImport
      };
      this.completedOrderOptionsModalRef = this.modalService.show(template);
    }
  }

  updateCompletedOrderOptions() {
    if (this.selectedCompletedOrder) {
      const updateCompletedOrderCommand = this.completedOrderOptionsEditor as UpdateCompletedOrderCommand;
      this.completedOrdersService.update(this.selectedCompletedOrder.id, updateCompletedOrderCommand).subscribe(
        () => {
          if (this.selectedCompletedOrder) {
            this.selectedCompletedOrder.name = this.completedOrderOptionsEditor.name;
            this.selectedCompletedOrder.userImport = this.completedOrderOptionsEditor.userImport;
            this.completedOrderOptionsModalRef.hide();
            this.completedOrderOptionsEditor = {};
          }
        },
        error => console.error(error)
      );
    }
  }

  confirmDeleteCompletedOrder(template: TemplateRef<any>) {
    this.completedOrderOptionsModalRef.hide();
    this.deleteCompletedOrderModalRef = this.modalService.show(template);
  }

  deleteCompletedOrderConfirmed(): void {
    this.completedOrdersService.delete(this.selectedCompletedOrder?.id).subscribe(
      () => {
        this.deleteCompletedOrderModalRef.hide();
        this.completedOrders = this.completedOrders.filter(t => t.id !== this.selectedCompletedOrder?.id);
        this.selectedCompletedOrder = this.completedOrders.length ? this.completedOrders[0] : undefined;
      },
      error => console.error(error)
    );
  }

  // Products
  showCompletedOrderProductDetailsModal(template: TemplateRef<any>, completedOrderProduct: CompletedOrderProductDTO): void {
    if (this.selectedCompletedOrder) {
      this.completedOrdersService.getCompletedOrderProduct(completedOrderProduct.id).subscribe(
        result => {
          if (this.selectedCompletedOrder) {
            if (this.selectedCompletedOrder.completedOrderProducts) {
              this.selectedCompletedOrderProduct = result;
              for (var i = this.selectedCompletedOrder.completedOrderProducts.length - 1; i >= 0; i--) {
                if (this.selectedCompletedOrder.completedOrderProducts[i].id == this.selectedCompletedOrderProduct?.id) {
                  if (this.selectedCompletedOrderProduct) {
                    this.selectedCompletedOrder.completedOrderProducts[i] = this.selectedCompletedOrderProduct;
                  }
                  break;
                }
              }
            }
            this.completedOrderProductDetailsEditor = {
              ...this.selectedCompletedOrderProduct,
              search: this.selectedCompletedOrderProduct?.name
            };
            if (this.selectedCompletedOrderProduct?.walmartSearchResponse) {
              this.completedOrderProductDetailsEditor.walmartSearchItems = JSON.parse(this.selectedCompletedOrderProduct.walmartSearchResponse).items;
            }

            this.completedOrderProductDetailsModalRef = this.modalService.show(template);
          }
        },
        error => console.error(error)
      );
    }
  }

  getWalmartLinkFromProductDetailsEditor(): string {
    if (this.completedOrderProductDetailsEditor.walmartSearchItems) {
      for (var walmartSearchItem of this.completedOrderProductDetailsEditor.walmartSearchItems) {
        if (walmartSearchItem.itemId == this.completedOrderProductDetailsEditor.walmartId) {
          return "https://www.walmart.com/ip/" + walmartSearchItem.name + "/" + walmartSearchItem.itemId;
        }
      }
    }
    return "#";
  }

  searchCompletedOrderProductName(): void {
    this.completedOrdersService.searchCompletedOrderProductName(this.completedOrderProductDetailsEditor.id, this.completedOrderProductDetailsEditor.search).subscribe(
      result => {
        if (this.selectedCompletedOrder) {
          if (this.selectedCompletedOrder.completedOrderProducts) {
            this.selectedCompletedOrderProduct = result;
            for (var i = this.selectedCompletedOrder.completedOrderProducts.length - 1; i >= 0; i--) {
              if (this.selectedCompletedOrder.completedOrderProducts[i].id == this.selectedCompletedOrderProduct?.id) {
                if (this.selectedCompletedOrderProduct) {
                  this.selectedCompletedOrder.completedOrderProducts[i] = this.selectedCompletedOrderProduct;
                }
                break;
              }
            }
          }
          var oldSearch = this.completedOrderProductDetailsEditor.search;
          this.completedOrderProductDetailsEditor = {
            ...this.selectedCompletedOrderProduct,
            search: oldSearch
          };
          if (this.selectedCompletedOrderProduct?.walmartSearchResponse) {
            this.completedOrderProductDetailsEditor.walmartSearchItems = JSON.parse(this.selectedCompletedOrderProduct.walmartSearchResponse).items;
          }
        }
      },
      error => {
        const errors = error.errors;

        if (errors && errors.Title) {
          this.completedOrderProductDetailsEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('name')?.focus(), 250);
      }
    );
  }

  updateCompletedOrderProductDetails(): void {
    const completedOrderProduct = this.completedOrderProductDetailsEditor as UpdateCompletedOrderProductCommand;
    this.completedOrdersService.updateCompletedOrderProduct(this.selectedCompletedOrderProduct?.id, completedOrderProduct).subscribe(
      result => {
        if (this.selectedCompletedOrder) {
          if (this.selectedCompletedOrder.completedOrderProducts) {
            this.selectedCompletedOrderProduct = result;
            for (var i = this.selectedCompletedOrder.completedOrderProducts.length - 1; i >= 0; i--) {
              if (this.selectedCompletedOrder.completedOrderProducts[i].id == this.selectedCompletedOrderProduct?.id) {
                if (this.selectedCompletedOrderProduct) {
                  this.selectedCompletedOrder.completedOrderProducts[i] = this.selectedCompletedOrderProduct;
                }
                break;
              }
            }
          }
          this.selectedCompletedOrderProduct = undefined;
          this.completedOrderProductDetailsModalRef.hide();
          this.completedOrderProductDetailsEditor = {};
        }
      },
      error => console.error(error)
    );
  }

  addCompletedOrderProduct() {
    const completedOrderProduct = {
      id: 0,
      name: '',
    } as CompletedOrderProductDTO;
    if (this.selectedCompletedOrder) {
      this.selectedCompletedOrder.completedOrderProducts?.push(completedOrderProduct);
      if (this.selectedCompletedOrder.completedOrderProducts) {
        const index = this.selectedCompletedOrder.completedOrderProducts.length - 1;
        this.editCompletedOrderProduct(completedOrderProduct, 'completedOrderProductName' + index);
      }
    }
  }

  editCompletedOrderProduct(completedOrderProduct: CompletedOrderProductDTO, inputId: string): void {
    this.selectedCompletedOrderProduct = completedOrderProduct;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateCompletedOrderProduct(completedOrderProduct: CompletedOrderProductDTO, pressedEnter: boolean = false): void {
    const isNewCompletedOrderProduct = completedOrderProduct.id === 0;

    if (!completedOrderProduct.name?.trim()) {
      this.deleteCompletedOrderProduct(completedOrderProduct);
      return;
    }

    if (completedOrderProduct.id === 0) {
      this.completedOrdersService
        .createCompletedOrderProduct({
          ...completedOrderProduct, completedOrderId: this.selectedCompletedOrder?.id
        } as CreateCompletedOrderProductCommand)
        .subscribe(
          result => {
            completedOrderProduct.id = result;
          },
          error => console.error(error)
        );
    } else {
      this.completedOrdersService.updateCompletedOrderProduct(completedOrderProduct.id, completedOrderProduct).subscribe(
        () => console.log('Update succeeded.'),
        error => console.error(error)
      );
    }

    this.selectedCompletedOrderProduct = undefined;

    if (isNewCompletedOrderProduct && pressedEnter) {
      setTimeout(() => this.addCompletedOrderProduct(), 250);
    }
  }

  deleteCompletedOrderProduct(completedOrderProduct: CompletedOrderProductDTO | undefined) {
    if (this.completedOrderProductDetailsModalRef) {
      this.completedOrderProductDetailsModalRef.hide();
    }

    if (this.selectedCompletedOrder && this.selectedCompletedOrderProduct && completedOrderProduct) {
      if (completedOrderProduct.id === 0) {
        const completedOrderProductIndex = this.selectedCompletedOrder.completedOrderProducts?.indexOf(this.selectedCompletedOrderProduct);
        if (completedOrderProductIndex) {
          this.selectedCompletedOrder.completedOrderProducts?.splice(completedOrderProductIndex, 1);
        }
      }
      else {
        this.completedOrdersService.deleteCompletedOrderProduct(completedOrderProduct.id).subscribe(
          result => {
            if (this.selectedCompletedOrder && this.selectedCompletedOrderProduct) {
              this.selectedCompletedOrder.completedOrderProducts = this.selectedCompletedOrder.completedOrderProducts?.filter(
                t => t.id !== completedOrderProduct.id
              );
            }
          },
          error => console.error(error)
        );
      }
    }
  }
}
