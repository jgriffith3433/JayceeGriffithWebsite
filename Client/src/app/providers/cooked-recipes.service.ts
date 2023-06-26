import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CookedRecipeCalledIngredientDetailsDTO } from "../models/CookedRecipeCalledIngredientDetailsDTO";
import { CookedRecipeCalledIngredientDTO } from "../models/CookedRecipeCalledIngredientDTO";
import { CookedRecipeDTO } from "../models/CookedRecipeDTO";
import { CreateCookedRecipeCalledIngredientCommand } from "../models/CreateCookedRecipeCalledIngredientCommand";
import { CreateCookedRecipeCommand } from "../models/CreateCookedRecipeCommand";
import { GetAllCookedRecipesVM } from "../models/GetAllCookedRecipesVM";
import { UpdateCookedRecipeCalledIngredientCommand } from "../models/UpdateCookedRecipeCalledIngredientCommand";
import { UpdateCookedRecipeCalledIngredientDetailsCommand } from "../models/UpdateCookedRecipeCalledIngredientDetailsCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class CookedRecipesService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAllCookedRecipes(): Observable<GetAllCookedRecipesVM> {
    return this.http.get(`${this.baseUri}/CookedRecipes`).pipe((map(x => <GetAllCookedRecipesVM>x)));
  }

  createCookedRecipe(command: CreateCookedRecipeCommand): Observable<CookedRecipeDTO> {
    return this.http.post(`${this.baseUri}/CookedRecipes`, command).pipe((map(x => <CookedRecipeDTO>x)));
  }

  deleteCookedRecipe(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/CookedRecipes/${id}`).pipe((map(x => <number>x)));
  }

  //CookedRecipeCalledIngredients
  getCookedRecipeCalledIngredientDetails(id: number | undefined): Observable<CookedRecipeCalledIngredientDetailsDTO> {
    return this.http.get(`${this.baseUri}/CookedRecipes/GetCookedRecipeCalledIngredientDetails/${id}`).pipe((map(x => <CookedRecipeCalledIngredientDetailsDTO>x)));
  }

  searchKitchenProductName(id: number | undefined, search: string | null | undefined): Observable<CookedRecipeCalledIngredientDetailsDTO> {
    let url = this.baseUri + "/CookedRecipes/SearchKitchenProductName?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    if (search !== undefined && search !== null)
      url += "Name=" + encodeURIComponent("" + search) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <CookedRecipeCalledIngredientDetailsDTO>x)));
  }

  createCookedRecipeCalledIngredient(command: CreateCookedRecipeCalledIngredientCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/CookedRecipes/CreateCookedRecipeCalledIngredient`, command).pipe((map(x => <number>x)));
  }

  updateCookedRecipeCalledIngredient(id: number | undefined, command: UpdateCookedRecipeCalledIngredientCommand): Observable<CookedRecipeCalledIngredientDTO> {
    return this.http.put(`${this.baseUri}/CookedRecipes/UpdateCookedRecipeCalledIngredient/${id}`, command).pipe((map(x => <CookedRecipeCalledIngredientDTO>x)));
  }

  updateCookedRecipeCalledIngredientDetails(id: number | undefined, command: UpdateCookedRecipeCalledIngredientDetailsCommand): Observable<CookedRecipeCalledIngredientDetailsDTO> {
    return this.http.put(`${this.baseUri}/CookedRecipes/UpdateCookedRecipeCalledIngredientDetails/${id}`, command).pipe((map(x => <CookedRecipeCalledIngredientDetailsDTO>x)));
  }

  deleteCookedRecipeCalledIngredient(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/CookedRecipes/DeleteCookedRecipeCalledIngredient/${id}`).pipe((map(x => <number>x)));
  }
}
