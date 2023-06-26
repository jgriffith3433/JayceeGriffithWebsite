import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CreateProductCommand } from '../../../models/CreateProductCommand';
import { WalmartProductDTO } from '../../../models/WalmartProductDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateProductCommand } from '../../../models/UpdateProductCommand';
import { UpdateProductNameCommand } from '../../../models/UpdateProductNameCommand';
import { UpdateWalmartProductSizeCommand } from '../../../models/UpdateWalmartProductSizeCommand';
import { UpdateProductUnitTypeCommand } from '../../../models/UpdateProductUnitTypeCommand';
import { WalmartProductsService } from '../../../providers/walmart-products.service';
//UpdateProductCommand,
//UnitTypeDTO

@Component({
  selector: 'app-walmart-products',
  templateUrl: './walmart-products.component.html'
})
export class ProductsComponent implements OnInit {
  debug: boolean = false;
  walmartProducts?: WalmartProductDTO[];
  kitchenUnitTypes?: UnitTypeDTO[];
  selectedProductName?: WalmartProductDTO;
  selectedProductSize?: WalmartProductDTO;
  selectedProductUnitType?: WalmartProductDTO;
  productEditor: any = {};
  newProductEditor: any = {};
  productModalRef: BsModalRef;
  newProductModalRef: BsModalRef;
  deleteProductModalRef: BsModalRef;

  constructor(
    private walmartProductsService: WalmartProductsService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }


  ngOnInit(): void {
    this.walmartProductsService.getAll().subscribe(
      result => {
        this.walmartProducts = result.walmartProducts;
        this.kitchenUnitTypes = result.kitchenUnitTypes;
      },
      error => console.error(error)
    );
  }

  getWalmartLinkFromProduct(walmartProduct: WalmartProductDTO): string {
    return "https://www.walmart.com/ip/" + walmartProduct.name + "/" + walmartProduct.walmartId;
  }

  showNewProductModal(template: TemplateRef<any>): void {
    this.newProductModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('name')?.focus(), 250);
  }

  newProductCancelled(): void {
    this.newProductModalRef.hide();
    this.newProductEditor = {};
  }

  getUnitTypeNameFromUnitTypeValue(kitchenUnitTypeValue: number | undefined): string {
    if (this.kitchenUnitTypes) {
      for (var kitchenUnitType of this.kitchenUnitTypes) {
        if (kitchenUnitType.value == kitchenUnitTypeValue) {
          return kitchenUnitType.name || "Unknown";
        }
      }
    }
    return "Unknown";
  }

  addProduct(): void {
    const walmartProduct = {
      id: 0,
      name: this.newProductEditor.name
    } as WalmartProductDTO;

    this.walmartProductsService.create(walmartProduct as CreateProductCommand).subscribe(
      result => {
        walmartProduct.id = result;
        this.walmartProducts?.push(walmartProduct);
        this.newProductModalRef.hide();
        this.newProductEditor = {};
      },
      error => {
        const errors = error.errors;

        if (errors && errors.Title) {
          this.newProductEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('name')?.focus(), 250);
      }
    );
  }

  editProductName(walmartProduct: WalmartProductDTO, inputId: string): void {
    this.selectedProductName = walmartProduct;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  editWalmartProductUnitType(walmartProduct: WalmartProductDTO, inputId: string): void {
    this.selectedProductUnitType = walmartProduct;
    setTimeout(() => {
      document.getElementById(inputId)?.focus();
      (<HTMLSelectElement>document.getElementById(inputId)).size = (<HTMLSelectElement>document.getElementById(inputId)).length;
    }, 100);
  }

  editProductSize(walmartProduct: WalmartProductDTO, inputId: string): void {
    this.selectedProductSize = walmartProduct;
    setTimeout(() => { document.getElementById(inputId)?.focus(); }, 100);
  }

  updateProductName(walmartProduct: WalmartProductDTO, pressedEnter: boolean = false): void {
    this.walmartProductsService.updateName(walmartProduct.id, walmartProduct).subscribe(
      result => {
        if (this.walmartProducts) {
          for (var i = this.walmartProducts.length - 1; i >= 0; i--) {
            if (this.walmartProducts[i].id == result.id) {
              this.walmartProducts[i] = result;
              break;
            }
          }
          this.selectedProductName = undefined;
        }
      },
      error => console.error(error)
    );
  }

  updateProductUnitType(walmartProduct: WalmartProductDTO, pressedEnter: boolean = false): void {
    this.walmartProductsService.updateUnitType(walmartProduct.id, walmartProduct).subscribe(
      result => {
        if (this.walmartProducts) {
          for (var i = this.walmartProducts.length - 1; i >= 0; i--) {
            if (this.walmartProducts[i].id == result.id) {
              this.walmartProducts[i] = result;
              break;
            }
          }
          this.selectedProductUnitType = undefined;
        }
      },
      error => console.error(error)
    );
  }

  updateProductSize(walmartProduct: WalmartProductDTO, pressedEnter: boolean = false): void {
    const updateProductSizeCommand = walmartProduct as UpdateWalmartProductSizeCommand;
    this.walmartProductsService.updateSize(walmartProduct.id, updateProductSizeCommand).subscribe(
      result => {
        if (this.walmartProducts) {
          for (var i = this.walmartProducts.length - 1; i >= 0; i--) {
            if (this.walmartProducts[i].id == result.id) {
              this.walmartProducts[i] = result;
              break;
            }
          }
          this.selectedProductSize = undefined;
        }
      },
      error => console.error(error)
    );
  }

  searchProductName(): void {
    const updateProductNameCommand = this.productEditor as UpdateProductCommand;
    this.walmartProductsService.updateName(this.productEditor.id, updateProductNameCommand).subscribe(
      result => {
        this.productEditor = result;
        if (this.walmartProducts) {
          for (var i = this.walmartProducts.length - 1; i >= 0; i--) {
            if (this.walmartProducts[i].id == this.productEditor.id) {
              this.walmartProducts[i] = this.productEditor;
              break;
            }
          }
          this.productEditor.walmartSearchItems = [];
          if (this.productEditor.walmartSearchResponse) {
            this.productEditor.walmartSearchItems = JSON.parse(this.productEditor.walmartSearchResponse).items;
            this.productEditor.walmartSearchItems.unshift({});
          }
        }
      },
      error => console.error(error)
    );
  }

  updateProductDetails(): void {
    const updateProductCommand = this.productEditor as UpdateProductCommand;
    this.walmartProductsService.update(this.productEditor.id, updateProductCommand).subscribe(
      result => {
        this.productEditor = result;
        if (this.walmartProducts) {
          for (var i = this.walmartProducts.length - 1; i >= 0; i--) {
            if (this.walmartProducts[i].id == this.productEditor.id) {
              this.walmartProducts[i] = this.productEditor;
              break;
            }
          }
          this.productEditor.walmartSearchItems = [];
          if (this.productEditor.walmartSearchResponse) {
            this.productEditor.walmartSearchItems = JSON.parse(this.productEditor.walmartSearchResponse).items;
            this.productEditor.walmartSearchItems.unshift({});
          }
          this.productModalRef.hide();
          this.productEditor = null;
        }
      },
      error => console.error(error)
    );
  }

  showProductDetailsModal(template: TemplateRef<any>, walmartProduct: WalmartProductDTO): void {
    this.walmartProductsService.getProductDetails(walmartProduct.id).subscribe(
      result => {
        if (this.walmartProducts) {
          for (var i = this.walmartProducts.length - 1; i >= 0; i--) {
            if (this.walmartProducts[i].id == result.id) {
              this.walmartProducts[i] = result;
              break;
            }
          }

          this.productEditor = {
            ...result
          };
          this.productEditor.walmartSearchItems = [];
          if (this.productEditor.walmartSearchResponse) {
            this.productEditor.walmartSearchItems = JSON.parse(this.productEditor.walmartSearchResponse).items;
            this.productEditor.walmartSearchItems.unshift({});
          }

          this.productModalRef = this.modalService.show(template);
        }
      },
      error => console.error(error)
    );
  }

  deleteProduct(walmartProduct: WalmartProductDTO | undefined) {
    if (walmartProduct) {
      if (this.productModalRef) {
        this.productModalRef.hide();
      }
      if (this.walmartProducts) {
        if (walmartProduct.id === 0) {
          if (this.selectedProductName) {
            const completedOrderProductIndex = this.walmartProducts.indexOf(this.selectedProductName);
            this.walmartProducts.splice(completedOrderProductIndex, 1);
          }
        } else {
          this.walmartProductsService.delete(walmartProduct.id).subscribe(
            () =>
            (this.walmartProducts = this.walmartProducts?.filter(
              t => t.id !== walmartProduct.id
            )),
            error => console.error(error)
          );
        }
      }
    }
  }
}
