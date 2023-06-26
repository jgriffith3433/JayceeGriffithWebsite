import { KitchenProductDTO } from "./KitchenProductDTO";

export class CalledIngredientDetailsDTO {
  id?: number = undefined;
  name?: string = undefined;
  kitchenProduct?: KitchenProductDTO = undefined;
  amount?: number | undefined = undefined;
  kitchenUnitType?: number = undefined;
  kitchenProductId?: number = undefined;
  kitchenProductSearchItems?: KitchenProductDTO[] = undefined;
}
