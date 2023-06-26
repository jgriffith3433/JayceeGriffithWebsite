import { UnitTypeDTO } from "./UnitTypeDTO";

export class UpdateCookedRecipeCalledIngredientDetailsCommand {
  id?: number = undefined;
  kitchenUnitType?: UnitTypeDTO = undefined;
  kitchenProductId?: number = undefined;
  name?: string = undefined;
  amount?: number = undefined;
}
