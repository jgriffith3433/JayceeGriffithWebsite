import { WalmartProductDTO } from "./WalmartProductDTO";

export class KitchenProductDetailsDTO {
  id?: number = undefined;
  name?: string = undefined;
  amount?: number = undefined;
  kitchenUnitType?: number = undefined;
  productId?: number | undefined = undefined;
  walmartProduct?: WalmartProductDTO;
  productSearchItems?: WalmartProductDTO[] = undefined;
}
