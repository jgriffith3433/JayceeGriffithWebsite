import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CreateKitchenProductCommand } from "../models/CreateKitchenProductCommand";
import { GetAllKitchenProductsVM } from "../models/GetAllKitchenProductsVM";
import { KitchenProductDetailsDTO } from "../models/KitchenProductDetailsDTO";
import { KitchenProductDTO } from "../models/KitchenProductDTO";
import { UpdateKitchenProductCommand } from "../models/UpdateKitchenProductCommand";
import { UpdateKitchenProductDetailsCommand } from "../models/UpdateKitchenProductDetailsCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class KitchenProductsService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllKitchenProductsVM> {
    return this.http.get(`${this.baseUri}/KitchenProducts`).pipe((map(x => <GetAllKitchenProductsVM>x)));
  }

  update(id: number | undefined, command: UpdateKitchenProductCommand): Observable<KitchenProductDTO> {
    return this.http.put(`${this.baseUri}/KitchenProducts/${id}`, command).pipe((map(x => <KitchenProductDTO>x)));
  }

  create(command: CreateKitchenProductCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/KitchenProducts`, command).pipe((map(x => <number>x)));
  }

  getKitchenProductDetails(id: number | undefined | null, name: string | undefined | null): Observable<KitchenProductDetailsDTO> {
    let url = this.baseUri + "/KitchenProducts/GetKitchenProductDetails?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    if (name !== undefined && name !== null)
      url += "Name=" + encodeURIComponent("" + name) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <KitchenProductDetailsDTO>x)));
  }

  updateKitchenProductDetails(id: number | undefined, command: UpdateKitchenProductDetailsCommand): Observable<KitchenProductDTO> {
    return this.http.put(`${this.baseUri}/KitchenProducts/UpdateKitchenProductDetails/${id}`, command).pipe((map(x => <KitchenProductDTO>x)));
  }

  delete(id: number): Observable<number> {
    return this.http.delete(`${this.baseUri}/KitchenProducts/${id}`).pipe((map(x => <number>x)));
  }
}
