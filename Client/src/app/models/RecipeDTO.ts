import { CalledIngredientDetailsDTO } from "./CalledIngredientDetailsDTO";

export class RecipeDTO {
  id?: number = undefined;
  name?: string = undefined;
  serves?: number = undefined;
  userImport?: string = undefined;
  calledIngredients?: CalledIngredientDetailsDTO[] = undefined;
}
