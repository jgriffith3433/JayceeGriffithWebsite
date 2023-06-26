import { Component, TemplateRef, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { OrderDTO } from '../../../models/OrderDTO';
import { OrderItemDTO } from '../../../models/OrderItemDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { OrdersService } from '../../../providers/orders.service';

@Component({
  selector: 'app-orders-component',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnInit {
  debug = false;
  orders: OrderDTO[];
  kitchenUnitTypes: UnitTypeDTO[];
  selectedOrder: OrderDTO | undefined;
  newOrderEditor: any = {};
  OrderOptionsEditor: any = {};
  OrderProductDetailsEditor: any = {};
  newOrderModalRef: BsModalRef;
  OrderOptionsModalRef: BsModalRef;
  deleteOrderModalRef: BsModalRef;
  OrderProductDetailsModalRef: BsModalRef;

  constructor(
    private ordersService: OrdersService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.ordersService.getAll().subscribe(
      result => {
        this.orders = result.orders || [] as OrderDTO[];
        if (this.orders.length) {
          this.selectedOrder = this.orders[0];
        }
      },
      error => console.error(error)
    );
  }

  //  Orders
  //remainingOrderProducts(order: OrderDTO): number | undefined {
  //  return order.orderItems?.filter(t => !t.walmartId).length;
  //}

  //showNewOrderModal(template: TemplateRef<any>): void {
  //  this.newOrderModalRef = this.modalService.show(template);
  //  setTimeout(() => document.getElementById('name')?.focus(), 250);
  //}

  //newOrderCancelled(): void {
  //  this.newOrderModalRef.hide();
  //  this.newOrderEditor = {};
  //}

  //addOrder(): void {
  //  const Order = {
  //    id: 0,
  //    name: this.newOrderEditor.name,
  //    userImport: this.newOrderEditor.userImport,
  //    orderItems: []
  //  } as OrderDTO;

  //  this.ordersService.create(Order as CreateOrderCommand).subscribe(
  //    result => {
  //      this.ordersService.get(result).subscribe(
  //        result => {
  //          this.orders.push(result);
  //          this.selectedOrder = result;
  //          this.newOrderModalRef.hide();
  //          this.newOrderEditor = {};
  //        },
  //        error => console.error(error)
  //      );
  //    },
  //    error => {
  //      const errors = error.errors;

  //      if (errors && errors.Title) {
  //        this.newOrderEditor.error = errors.Title[0];
  //      }

  //      setTimeout(() => document.getElementById('name')?.focus(), 250);
  //    }
  //  );
  //}

  //showOrderOptionsModal(template: TemplateRef<any>) {
  //  if (this.selectedOrder) {
  //    this.OrderOptionsEditor = {
  //      id: this.selectedOrder.id,
  //      name: this.selectedOrder.name,
  //      userImport: this.selectedOrder.userImport
  //    };
  //    this.OrderOptionsModalRef = this.modalService.show(template);
  //  }
  //}

  //updateOrderOptions() {
  //  if (this.selectedOrder) {
  //    const updateOrderCommand = this.OrderOptionsEditor as UpdateOrderCommand;
  //    this.ordersService.update(this.selectedOrder.id, updateOrderCommand).subscribe(
  //      () => {
  //        if (this.selectedOrder) {
  //          this.selectedOrder.name = this.OrderOptionsEditor.name;
  //          this.selectedOrder.userImport = this.OrderOptionsEditor.userImport;
  //          this.OrderOptionsModalRef.hide();
  //          this.OrderOptionsEditor = {};
  //        }
  //      },
  //      error => console.error(error)
  //    );
  //  }
  //}

  //confirmDeleteOrder(template: TemplateRef<any>) {
  //  this.OrderOptionsModalRef.hide();
  //  this.deleteOrderModalRef = this.modalService.show(template);
  //}

  //deleteOrderConfirmed(): void {
  //  this.ordersService.delete(this.selectedOrder?.id).subscribe(
  //    () => {
  //      this.deleteOrderModalRef.hide();
  //      this.orders = this.orders.filter(t => t.id !== this.selectedOrder?.id);
  //      this.selectedOrder = this.orders.length ? this.orders[0] : undefined;
  //    },
  //    error => console.error(error)
  //  );
  //}

  //// Products
  //showOrderProductDetailsModal(template: TemplateRef<any>, orderItem: OrderItemDTO): void {
  //  if (this.selectedOrder) {
  //    this.ordersService.getOrderProduct(orderItem.id).subscribe(
  //      result => {
  //        if (this.selectedOrder) {
  //          if (this.selectedOrder.orderItems) {
  //            this.selectedOrderProduct = result;
  //            for (var i = this.selectedOrder.orderItems.length - 1; i >= 0; i--) {
  //              if (this.selectedOrder.orderItems[i].id == this.selectedOrderProduct?.id) {
  //                if (this.selectedOrderProduct) {
  //                  this.selectedOrder.orderItems[i] = this.selectedOrderProduct;
  //                }
  //                break;
  //              }
  //            }
  //          }
  //          this.OrderProductDetailsEditor = {
  //            ...this.selectedOrderProduct,
  //            search: this.selectedOrderProduct?.name
  //          };
  //          if (this.selectedOrderProduct?.walmartSearchResponse) {
  //            this.OrderProductDetailsEditor.walmartSearchItems = JSON.parse(this.selectedOrderProduct.walmartSearchResponse).items;
  //          }

  //          this.OrderProductDetailsModalRef = this.modalService.show(template);
  //        }
  //      },
  //      error => console.error(error)
  //    );
  //  }
  //}

  getWalmartLinkFromOrderProduct(orderItem: OrderItemDTO): string {
    return "https://www.walmart.com/ip/" + orderItem.name + "/" + orderItem.walmartId;
  }

  //searchOrderProductName(): void {
  //  this.ordersService.searchOrderProductName(this.OrderProductDetailsEditor.id, this.OrderProductDetailsEditor.search).subscribe(
  //    result => {
  //      if (this.selectedOrder) {
  //        if (this.selectedOrder.orderItems) {
  //          this.selectedOrderProduct = result;
  //          for (var i = this.selectedOrder.orderItems.length - 1; i >= 0; i--) {
  //            if (this.selectedOrder.orderItems[i].id == this.selectedOrderProduct?.id) {
  //              if (this.selectedOrderProduct) {
  //                this.selectedOrder.orderItems[i] = this.selectedOrderProduct;
  //              }
  //              break;
  //            }
  //          }
  //        }
  //        var oldSearch = this.OrderProductDetailsEditor.search;
  //        this.OrderProductDetailsEditor = {
  //          ...this.selectedOrderProduct,
  //          search: oldSearch
  //        };
  //        if (this.selectedOrderProduct?.walmartSearchResponse) {
  //          this.OrderProductDetailsEditor.walmartSearchItems = JSON.parse(this.selectedOrderProduct.walmartSearchResponse).items;
  //        }
  //      }
  //    },
  //    error => {
  //      const errors = error.errors;

  //      if (errors && errors.Title) {
  //        this.OrderProductDetailsEditor.error = errors.Title[0];
  //      }

  //      setTimeout(() => document.getElementById('name')?.focus(), 250);
  //    }
  //  );
  //}

  //updateOrderProductDetails(): void {
  //  const orderItem = this.OrderProductDetailsEditor as UpdateOrderProductCommand;
  //  this.ordersService.updateOrderProduct(this.selectedOrderProduct?.id, orderItem).subscribe(
  //    result => {
  //      if (this.selectedOrder) {
  //        if (this.selectedOrder.orderItems) {
  //          this.selectedOrderProduct = result;
  //          for (var i = this.selectedOrder.orderItems.length - 1; i >= 0; i--) {
  //            if (this.selectedOrder.orderItems[i].id == this.selectedOrderProduct?.id) {
  //              if (this.selectedOrderProduct) {
  //                this.selectedOrder.orderItems[i] = this.selectedOrderProduct;
  //              }
  //              break;
  //            }
  //          }
  //        }
  //        this.selectedOrderProduct = undefined;
  //        this.OrderProductDetailsModalRef.hide();
  //        this.OrderProductDetailsEditor = {};
  //      }
  //    },
  //    error => console.error(error)
  //  );
  //}

  //addOrderProduct() {
  //  const orderItem = {
  //    id: 0,
  //    name: '',
  //  } as OrderItemDTO;
  //  if (this.selectedOrder) {
  //    this.selectedOrder.orderItems?.push(orderItem);
  //    if (this.selectedOrder.orderItems) {
  //      const index = this.selectedOrder.orderItems.length - 1;
  //      this.editOrderProduct(orderItem, 'OrderProductName' + index);
  //    }
  //  }
  //}

  //editOrderProduct(orderItem: OrderItemDTO, inputId: string): void {
  //  this.selectedOrderProduct = orderItem;
  //  setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  //}

  //updateOrderProduct(orderItem: OrderItemDTO, pressedEnter: boolean = false): void {
  //  const isNewOrderProduct = orderItem.id === 0;

  //  if (!orderItem.name?.trim()) {
  //    this.deleteOrderProduct(orderItem);
  //    return;
  //  }

  //  if (orderItem.id === 0) {
  //    this.ordersService
  //      .createOrderProduct({
  //        ...orderItem, OrderId: this.selectedOrder?.id
  //      } as CreateOrderProductCommand)
  //      .subscribe(
  //        result => {
  //          orderItem.id = result;
  //        },
  //        error => console.error(error)
  //      );
  //  } else {
  //    this.ordersService.updateOrderProduct(orderItem.id, orderItem).subscribe(
  //      () => console.log('Update succeeded.'),
  //      error => console.error(error)
  //    );
  //  }

  //  this.selectedOrderProduct = undefined;

  //  if (isNewOrderProduct && pressedEnter) {
  //    setTimeout(() => this.addOrderProduct(), 250);
  //  }
  //}

  //deleteOrderProduct(orderItem: OrderItemDTO | undefined) {
  //  if (this.OrderProductDetailsModalRef) {
  //    this.OrderProductDetailsModalRef.hide();
  //  }

  //  if (this.selectedOrder && this.selectedOrderProduct && orderItem) {
  //    if (orderItem.id === 0) {
  //      const OrderProductIndex = this.selectedOrder.orderItems?.indexOf(this.selectedOrderProduct);
  //      if (OrderProductIndex) {
  //        this.selectedOrder.orderItems?.splice(OrderProductIndex, 1);
  //      }
  //    }
  //    else {
  //      this.ordersService.deleteOrderProduct(orderItem.id).subscribe(
  //        result => {
  //          if (this.selectedOrder && this.selectedOrderProduct) {
  //            this.selectedOrder.orderItems = this.selectedOrder.orderItems?.filter(
  //              t => t.id !== orderItem.id
  //            );
  //          }
  //        },
  //        error => console.error(error)
  //      );
  //    }
  //  }
  //}
}
