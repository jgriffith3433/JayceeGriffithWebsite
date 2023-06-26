import { KitchenProductDTO } from "./KitchenProductDTO";

export class CalledIngredientDTO {
  id?: number = undefined;
  name?: string = undefined;
  kitchenProduct?: KitchenProductDTO = undefined;
  amount?: number | undefined = undefined;
  kitchenUnitType?: number = undefined;
  kitchenProductId?: number = undefined;
}
