import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { GetAllOrdersVm } from "../models/GetAllOrdersVm";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllOrdersVm> {
    return this.http.get(`${this.baseUri}/Orders`).pipe((map(x => <GetAllOrdersVm>x)));
  }
}
