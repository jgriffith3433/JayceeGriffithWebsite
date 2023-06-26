import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CalledIngredientDetailsDTO } from "../models/CalledIngredientDetailsDTO";
import { CalledIngredientDTO } from "../models/CalledIngredientDTO";
import { CreateCalledIngredientCommand } from "../models/CreateCalledIngredientCommand";
import { CreateRecipeCommand } from "../models/CreateRecipeCommand";
import { GetAllRecipesVM } from "../models/GetAllRecipesVM";
import { RecipeDTO } from "../models/RecipeDTO";
import { UpdateCalledIngredientCommand } from "../models/UpdateCalledIngredientCommand";
import { UpdateCalledIngredientDetailsCommand } from "../models/UpdateCalledIngredientDetailsCommand";
import { UpdateRecipeNameCommand } from "../models/UpdateRecipeNameCommand";
import { UpdateRecipeServesCommand } from "../models/UpdateRecipeServesCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class RecipesService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllRecipesVM> {
    return this.http.get(`${this.baseUri}/Recipes`).pipe((map(x => <GetAllRecipesVM>x)));
  }

  createRecipe(command: CreateRecipeCommand): Observable<RecipeDTO> {
    return this.http.post(`${this.baseUri}/Recipes`, command).pipe((map(x => <RecipeDTO>x)));
  }

  updateName(id: number | undefined, command: UpdateRecipeNameCommand): Observable<RecipeDTO> {
    return this.http.put(`${this.baseUri}/Recipes/UpdateName/${id}`, command).pipe((map(x => <RecipeDTO>x)));
  }

  updateServes(id: number | undefined, command: UpdateRecipeServesCommand): Observable<RecipeDTO> {
    return this.http.put(`${this.baseUri}/Recipes/UpdateServes/${id}`, command).pipe((map(x => <RecipeDTO>x)));
  }

  deleteRecipe(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/Recipes/${id}`).pipe((map(x => <number>x)));
  }

  createCalledIngredient(command: CreateCalledIngredientCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/Recipes/CreateCalledIngredient`, command).pipe((map(x => <number>x)));
  }

  getCalledIngredientDetails(id: number | undefined): Observable<CalledIngredientDetailsDTO> {
    return this.http.get(`${this.baseUri}/Recipes/GetCalledIngredientDetails/${id}`).pipe((map(x => <CalledIngredientDetailsDTO>x)));
  }

  searchKitchenProductName(id: number | undefined, search: string | null | undefined): Observable<CalledIngredientDetailsDTO> {
    let url = this.baseUri + "/Recipes/SearchKitchenProductName?";
    if (id === null)
      throw new Error("The parameter 'id' cannot be null.");
    else if (id !== undefined)
      url += "Id=" + encodeURIComponent("" + id) + "&";
    if (search !== undefined && search !== null)
      url += "Name=" + encodeURIComponent("" + search) + "&";
    url = url.replace(/[?&]$/, "");

    return this.http.get(`${url}`).pipe((map(x => <CalledIngredientDetailsDTO>x)));
  }

  updateCalledIngredient(id: number | undefined, command: UpdateCalledIngredientCommand): Observable<CalledIngredientDTO> {
    return this.http.put(`${this.baseUri}/Recipes/UpdateCalledIngredient/${id}`, command).pipe((map(x => <CalledIngredientDTO>x)));
  }

  updateCalledIngredientDetails(id: number | undefined, command: UpdateCalledIngredientDetailsCommand): Observable<CalledIngredientDetailsDTO> {
    return this.http.put(`${this.baseUri}/Recipes/UpdateCalledIngredientDetails/${id}`, command).pipe((map(x => <CalledIngredientDetailsDTO>x)));
  }

  deleteCalledIngredient(id: number): Observable<number> {
    return this.http.delete(`${this.baseUri}/Recipes/DeleteCalledIngredient/${id}`).pipe((map(x => <number>x)));
  }
}
