import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CreateKitchenProductCommand } from '../../../models/CreateKitchenProductCommand';
import { KitchenProductDetailsDTO } from '../../../models/KitchenProductDetailsDTO';
import { KitchenProductDTO } from '../../../models/KitchenProductDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateKitchenProductCommand } from '../../../models/UpdateKitchenProductCommand';
import { UpdateKitchenProductDetailsCommand } from '../../../models/UpdateKitchenProductDetailsCommand';
import { WalmartProductDTO } from '../../../models/WalmartProductDTO';
import { KitchenProductsService } from '../../../providers/kitchen-products.service';
//KitchenProductClient,
//KitchenProductDTO,
//KitchenProductDetailsVm,
//CreateKitchenProductCommand,
//UpdateKitchenProductCommand,
//UpdateKitchenProductDetailsCommand,
//UnitTypeDTO

export interface ObjectConstructor {
  keys<T>(o: T): (keyof T)[];
}
@Component({
  selector: 'app-kitchen-products',
  templateUrl: './kitchen-products.component.html'
})
export class KitchenProductsComponent implements OnInit {
  public kitchenProducts?: KitchenProductDTO[];
  kitchenUnitTypes?: UnitTypeDTO[];
  debug = false;
  selectedKitchenProductAmount: KitchenProductDTO | null;
  kitchenProductEditor: any = {};
  newKitchenProductEditor: any = {};
  kitchenProductModalRef: BsModalRef;
  newKitchenProductModalRef: BsModalRef;
  deleteKitchenProductModalRef: BsModalRef;

  constructor(
    private kitchenProductService: KitchenProductsService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.kitchenProductService.getAll().subscribe(
      result => {
        this.kitchenProducts = result.kitchenProducts;
        this.kitchenUnitTypes = result.kitchenUnitTypes;
      },
      error => console.error(error)
    );
  }

  showNewKitchenProductModal(template: TemplateRef<any>): void {
    this.newKitchenProductModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('name')?.focus(), 250);
  }

  newKitchenProductCancelled(): void {
    this.newKitchenProductModalRef.hide();
    this.newKitchenProductEditor = {};
  }

  getUnitTypeNameFromUnitTypeValue(kitchenUnitTypeValue: number | undefined): string | undefined {
    if (this.kitchenUnitTypes) {
      for (var kitchenUnitType of this.kitchenUnitTypes) {
        if (kitchenUnitType.value == kitchenUnitTypeValue) {
          return kitchenUnitType.name;
        }
      }
    }
    return "Unknown";
  }

  getWalmartLinkFromProduct(walmartProduct: WalmartProductDTO | undefined): string {
    if (walmartProduct) {
      return "https://www.walmart.com/ip/" + walmartProduct.name + "/" + walmartProduct.walmartId;
    }
    else {
      return "https://www.walmart.com";
    }
  }

  addKitchenProduct(): void {
    const kitchenProduct = {
      id: 0,
      name: this.newKitchenProductEditor.name,
      productSearchItems: []
    } as KitchenProductDetailsDTO;

    this.kitchenProductService.create(kitchenProduct as CreateKitchenProductCommand).subscribe(
      result => {
        kitchenProduct.id = result;
        this.kitchenProducts?.push(kitchenProduct);
        this.newKitchenProductModalRef.hide();
        this.newKitchenProductEditor = {};
      },
      error => {
        const errors = error.errors;

        if (errors && errors.Title) {
          this.newKitchenProductEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('userImport')?.focus(), 250);
      }
    );
  }

  editKitchenProductAmount(kitchenProduct: KitchenProductDTO, inputId: string): void {
    this.selectedKitchenProductAmount = kitchenProduct;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateKitchenProductAmount(kitchenProduct: KitchenProductDTO, pressedEnter: boolean = false): void {
    const updateKitchenProductCommand = this._cast(kitchenProduct, UpdateKitchenProductCommand);
    this.kitchenProductService.update(kitchenProduct.id, updateKitchenProductCommand).subscribe(
      result => {
        if (this.kitchenProducts) {
          for (var i = this.kitchenProducts.length - 1; i >= 0; i--) {
            if (this.kitchenProducts[i].id == result.id) {
              this.kitchenProducts[i] = result;
              break;
            }
          }
        }
        this.selectedKitchenProductAmount = null;
      },
      error => console.error(error)
    );
  }

  updateKitchenProductDetails(): void {
    const updateKitchenProductDetailsCommand = this._cast(this.kitchenProductEditor, UpdateKitchenProductDetailsCommand);

    this.kitchenProductService.updateKitchenProductDetails(this.kitchenProductEditor.id, updateKitchenProductDetailsCommand).subscribe(
      result => {
        if (this.kitchenProducts) {
          for (var i = this.kitchenProducts.length - 1; i >= 0; i--) {
            if (this.kitchenProducts[i].id == result.id) {
              this.kitchenProducts[i] = result;
              break;
            }
          }
          if (this.kitchenProductEditor.id != result.id) {
            //kitchen product was merged into another kitchen product. remove the old one
            for (var i = this.kitchenProducts.length - 1; i >= 0; i--) {
              if (this.kitchenProducts[i].id == this.kitchenProductEditor.id) {
                this.kitchenProducts.splice(i, 1);
                break;
              }
            }
          }
          this.kitchenProductModalRef.hide();
          this.kitchenProductEditor = {};
        }
      },
      error => console.error(error)
    );
  }

  //_cast<T extends Object>(object: any): T {
  //  type OptionsFlags<T> = {
  //    [Property in keyof T]: boolean;
  //  };
  //  type FeatureOptions = OptionsFlags<T>;

  //  //const specificMembers: string[] = Object.keys(new Specific());
  //  //const specific: ISpecific = lodash.pick(extended, specificMembers);
  //  //console.log(specific); // {a: "type", b: "script"}

  //  //let ob: ObjectConstructor = new { ...object };
  //  //type typeKeys = keyof T;
  //  //getProperty<T>()
  //  var returnObject: FeatureOptions = {} as FeatureOptions;
  //  Object.keys(returnObject).forEach((key) => {
  //    console.log(object[key])
  //  })
  //  type MyIdentityType<T> = T;
  //  let returnd: MyIdentityType<T> = {} as MyIdentityType<T>;
  //  let returnValue: T = { } as T;
  //  Object.keys(returnd).forEach(key => {
  //    const value = (object as any)[key];
  //    (returnd as any)[key] = value;
  //  });
  //  return returnd;
  //}

  showKitchenProductDetailsModal(template: TemplateRef<any>, kitchenProduct: KitchenProductDTO): void {
    this.kitchenProductService.getKitchenProductDetails(kitchenProduct.id, kitchenProduct.name).subscribe(
      result => {
        if (this.kitchenProducts) {
          for (var i = this.kitchenProducts.length - 1; i >= 0; i--) {
            if (this.kitchenProducts[i].id == result.id) {
              this.kitchenProducts[i] = result;
              break;
            }
          }

          this.kitchenProductEditor = {
            ...result,
            name: result.name
          };

          this.kitchenProductModalRef = this.modalService.show(template);
        }
      },
      error => console.error(error)
    );
  }

  searchProductName(): void {
    this.kitchenProductService.getKitchenProductDetails(this.kitchenProductEditor.id, this.kitchenProductEditor.name).subscribe(
      result => {
        if (this.kitchenProducts) {
          for (var i = this.kitchenProducts?.length - 1; i >= 0; i--) {
            if (this.kitchenProducts[i].id == result.id) {
              this.kitchenProducts[i] = result;
              break;
            }
          }
          var oldName = this.kitchenProductEditor.name;
          this.kitchenProductEditor = {
            ...result,
            name: oldName
          };
        }
      },
      error => {
        const errors = error.errors;

        if (errors && errors.Title) {
          this.kitchenProductEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('name')?.focus(), 250);
      }
    );
  }

  confirmDeleteKitchenProduct(template: TemplateRef<any>) {
    this.kitchenProductModalRef.hide();
    this.deleteKitchenProductModalRef = this.modalService.show(template);
  }

  deleteKitchenProductConfirmed(): void {
    this.kitchenProductService.delete(this.kitchenProductEditor.id).subscribe(
      () => {
        this.deleteKitchenProductModalRef.hide();
        this.kitchenProducts = this.kitchenProducts?.filter(t => t.id !== this.kitchenProductEditor.id);
      },
      error => console.error(error)
    );
  }

  _cast<K extends T, T>(obj: K, tClass: { new(...args: any[]): K }): K {
    let returnObject: K = new tClass();
    for (let p in returnObject) {
      const value = obj[p] || undefined;
      if (value != undefined) {
        returnObject[p] = value;
      }
    }
    return returnObject;
  }

}
