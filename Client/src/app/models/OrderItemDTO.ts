import { WalmartProductDTO } from "./WalmartProductDTO";

export class OrderItemDTO {
  id?: number = undefined;
  walmartId?: number = undefined;
  name?: string = undefined;
  walmartProduct?: WalmartProductDTO | undefined = undefined;
}
