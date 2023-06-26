import { CompletedOrderProductDTO } from "./CompletedOrderProductDTO";

export class CompletedOrderDTO {
  id?: number = undefined;
  name?: string = undefined;
  userImport?: string | undefined = undefined;
  completedOrderProducts?: CompletedOrderProductDTO[] = undefined;
}
