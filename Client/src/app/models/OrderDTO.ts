import { OrderItemDTO } from "./OrderItemDTO";

export class OrderDTO {
  id?: number = undefined;
  orderItems?: OrderItemDTO[] = undefined;
}
