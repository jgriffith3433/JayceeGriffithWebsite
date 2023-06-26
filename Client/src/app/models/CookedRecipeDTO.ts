import { CookedRecipeCalledIngredientDetailsDTO } from "./CookedRecipeCalledIngredientDetailsDTO";
import { RecipeDTO } from "./RecipeDTO";

export class CookedRecipeDTO {
  id?: number = undefined;
  recipe?: RecipeDTO = undefined;
  recipeId?: number = undefined;
  cookedRecipeCalledIngredients?: CookedRecipeCalledIngredientDetailsDTO[] = undefined;
}
