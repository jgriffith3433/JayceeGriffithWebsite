import { CalledIngredientDTO } from "./CalledIngredientDTO";
import { KitchenProductDTO } from "./KitchenProductDTO";
import { UnitTypeDTO } from "./UnitTypeDTO";

export class CookedRecipeCalledIngredientDTO {
  id?: number = undefined;
  cookedRecipeId?: number = undefined;
  calledIngredient?: CalledIngredientDTO = undefined;
  kitchenProduct?: KitchenProductDTO = undefined;
  kitchenProductId?: number = undefined;
  name?: string = undefined;
  kitchenUnitType?: UnitTypeDTO = undefined;
  amount?: number = undefined;
  kitchenProductSearchItems?: KitchenProductDTO[] = undefined;
}
