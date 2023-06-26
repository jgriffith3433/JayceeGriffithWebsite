import { TodoItemDTO } from "./TodoItemDTO";

export class TodoListDTO {
  id?: number = undefined;
  title?: string = undefined;
  color?: string = undefined;
  items?: TodoItemDTO[] = undefined;
}
