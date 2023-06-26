import { PriorityLevelDTO } from "./PriorityLevelDTO";
import { TodoListDTO } from "./TodoListDTO";

export class GetAllTodoListsVM {
  lists?: TodoListDTO[] = undefined;
  priorityLevels?: PriorityLevelDTO[] = undefined;
}
