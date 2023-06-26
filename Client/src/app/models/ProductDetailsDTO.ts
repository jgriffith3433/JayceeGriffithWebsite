import { WalmartProductDTO } from "./WalmartProductDTO";

export class ProductDetailsDTO {
  id?: number;
  name?: string;
  walmartId?: number = undefined;
  walmartLink?: string = undefined;
  walmartSize?: string = undefined;
  walmartItemResponse?: string = undefined;
  walmartSearchResponse?: string = undefined;
  error?: string = undefined;
  size?: number = undefined;
  price?: number = undefined;
  verified?: boolean = undefined;
  kitchenUnitType?: number = undefined;
  kitchenProductId?: number = undefined;
}
