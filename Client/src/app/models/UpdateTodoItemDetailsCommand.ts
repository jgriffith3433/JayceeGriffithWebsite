import { PriorityLevelDTO } from "./PriorityLevelDTO";

export class UpdateTodoItemDetailsCommand {
  id?: number = undefined;
  listId?: number = undefined;
  priority?: PriorityLevelDTO = undefined;
  note?: string = undefined;
}
