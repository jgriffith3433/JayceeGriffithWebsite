import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CreateProductCommand } from "../models/CreateProductCommand";
import { GetAllWalmartProductsVM } from "../models/GetAllWalmartProductsVM";
import { ProductDetailsDTO } from "../models/ProductDetailsDTO";
import { WalmartProductDTO } from "../models/WalmartProductDTO";
import { UpdateProductCommand } from "../models/UpdateProductCommand";
import { UpdateProductNameCommand } from "../models/UpdateProductNameCommand";
import { UpdateWalmartProductSizeCommand } from "../models/UpdateWalmartProductSizeCommand";
import { UpdateProductUnitTypeCommand } from "../models/UpdateProductUnitTypeCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class WalmartProductsService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll() {
    return this.http.get(`${this.baseUri}/WalmartProducts`).pipe((map(x => <GetAllWalmartProductsVM>x)));
  }

  create(command: any): Observable<number> {
    return this.http.post(`${this.baseUri}/WalmartProducts`, this._cast(command, CreateProductCommand)).pipe((map(x => <number>x)));
  }

  update(id: number | undefined, command: any): Observable<WalmartProductDTO> {
    return this.http.put(`${this.baseUri}/WalmartProducts/${id}`, this._cast(command, UpdateProductCommand)).pipe((map(x => <WalmartProductDTO>x)));
  }

  updateName(id: number | undefined, command: any): Observable<WalmartProductDTO> {
    return this.http.put(`${this.baseUri}/WalmartProducts/UpdateProductName/${id}`, this._cast(command, UpdateProductNameCommand)).pipe((map(x => <WalmartProductDTO>x)));
  }

  updateUnitType(id: number | undefined, command: any): Observable<WalmartProductDTO> {
    return this.http.put(`${this.baseUri}/WalmartProducts/UpdateUnitType/${id}`, this._cast(command, UpdateProductUnitTypeCommand)).pipe((map(x => <WalmartProductDTO>x)));
  }

  updateSize(id: number | undefined, command: any): Observable<WalmartProductDTO> {
    return this.http.put(`${this.baseUri}/WalmartProducts/UpdateSize/${id}`, this._cast(command, UpdateWalmartProductSizeCommand)).pipe((map(x => <WalmartProductDTO>x)));
  }

  getProductDetails(id: number | undefined): Observable<WalmartProductDTO> {
    let url = this.baseUri + "/WalmartProducts/GetProductDetails?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <ProductDetailsDTO>x)));
  }

  delete(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/WalmartProducts/${id}`).pipe((map(x => <number>x)));
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
