import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { GetAllCalledIngredientsVM } from "../models/GetAllCalledIngredientsVM";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class CalledIngredientsService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllCalledIngredientsVM> {
    return this.http.get(`${this.baseUri}/CalledIngredients`).pipe((map(x => <GetAllCalledIngredientsVM>x)));
  }
}
