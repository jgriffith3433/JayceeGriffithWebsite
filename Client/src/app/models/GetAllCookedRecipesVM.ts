import { CookedRecipeDTO } from "./CookedRecipeDTO";
import { RecipesOptionDTO } from "./RecipesOptionDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export class GetAllCookedRecipesVM {
  kitchenUnitTypes?: UnitTypeDTO[] = undefined;
  cookedRecipes?: CookedRecipeDTO[] = undefined;
  recipesOptions?: RecipesOptionDTO[] = undefined;
}
