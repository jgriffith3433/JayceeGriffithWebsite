import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CompletedOrderDTO } from "../models/CompletedOrderDTO";
import { CompletedOrderProductDTO } from "../models/CompletedOrderProductDTO";
import { CreateCompletedOrderCommand } from "../models/CreateCompletedOrderCommand";
import { CreateCompletedOrderProductCommand } from "../models/CreateCompletedOrderProductCommand";
import { GetAllCompletedOrdersVm } from "../models/GetAllCompletedOrdersVm";
import { UpdateCompletedOrderCommand } from "../models/UpdateCompletedOrderCommand";
import { UpdateCompletedOrderProductCommand } from "../models/UpdateCompletedOrderProductCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class CompletedOrdersService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllCompletedOrdersVm> {
    return this.http.get(`${this.baseUri}/CompletedOrders`).pipe((map(x => <GetAllCompletedOrdersVm>x)));
  }

  get(id: number): Observable<CompletedOrderDTO> {
    return this.http.get(`${this.baseUri}/CompletedOrders/${id}`).pipe((map(x => <CompletedOrderDTO>x)));
  }

  create(command: CreateCompletedOrderCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/CompletedOrders`, command).pipe((map(x => <number>x)));
  }

  update(id: number | undefined, command: UpdateCompletedOrderCommand): Observable<number> {
    return this.http.put(`${this.baseUri}/CompletedOrders/${id}`, command).pipe((map(x => <number>x)));
  }

  delete(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/CompletedOrders/${id}`).pipe((map(x => <number>x)));
  }

  getCompletedOrderProduct(id: number | undefined): Observable<CompletedOrderProductDTO> {
    return this.http.get(`${this.baseUri}/CompletedOrders/GetCompletedOrderProduct/${id}`).pipe((map(x => <CompletedOrderProductDTO>x)));
  }

  searchCompletedOrderProductName(id: number | undefined, search: string | null | undefined): Observable<CompletedOrderProductDTO> {
    let url = this.baseUri + "/CompletedOrders/SearchCompletedOrderProductName?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    if (search !== undefined && search !== null)
      url += "Name=" + encodeURIComponent("" + search) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <CompletedOrderProductDTO>x)));
  }

  createCompletedOrderProduct(command: CreateCompletedOrderProductCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/CompletedOrders/CreateCompletedOrderProduct`, command).pipe((map(x => <number>x)));
  }

  updateCompletedOrderProduct(id: number | undefined, command: UpdateCompletedOrderProductCommand): Observable<CompletedOrderProductDTO> {
    return this.http.put(`${this.baseUri}/CompletedOrders/UpdateCompletedOrderProduct/${id}`, command).pipe((map(x => <CompletedOrderProductDTO>x)));
  }

  deleteCompletedOrderProduct(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/CompletedOrders/DeleteCompletedOrderProduct/${id}`).pipe((map(x => <number>x)));
  }
}
