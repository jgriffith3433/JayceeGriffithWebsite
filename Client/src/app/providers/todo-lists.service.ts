import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { CreateTodoItemCommand } from "../models/CreateTodoItemCommand";
import { CreateTodoListCommand } from "../models/CreateTodoListCommand";
import { GetAllTodoListsVM } from "../models/GetAllTodoListsVM";
import { TodoItemDTO } from "../models/TodoItemDTO";
import { TodoListDTO } from "../models/TodoListDTO";
import { UpdateTodoItemCommand } from "../models/UpdateTodoItemCommand";
import { UpdateTodoItemDetailsCommand } from "../models/UpdateTodoItemDetailsCommand";
import { UpdateTodoListCommand } from "../models/UpdateTodoListCommand";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class TodoListsService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getAll(): Observable<GetAllTodoListsVM> {
    return this.http.get(`${this.baseUri}/TodoLists`).pipe((map(x => <GetAllTodoListsVM>x)));
  }

  create(command: CreateTodoListCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/TodoLists`, this._cast(command, CreateTodoListCommand)).pipe((map(x => <number>x)));
  }

  update(id: number | undefined, command: UpdateTodoListCommand): Observable<TodoListDTO> {
    return this.http.put(`${this.baseUri}/TodoLists/${id}`, this._cast(command, UpdateTodoListCommand)).pipe((map(x => <TodoListDTO>x)));
  }

  delete(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/TodoLists/${id}`).pipe((map(x => <number>x)));
  }

  createTodoItem(command: CreateTodoItemCommand): Observable<number> {
    return this.http.post(`${this.baseUri}/TodoLists/CreateTodoItem`, this._cast(command, CreateTodoItemCommand)).pipe((map(x => <number>x)));
  }

  updateTodoItem(id: number | undefined, command: UpdateTodoItemCommand): Observable<TodoItemDTO> {
    return this.http.put(`${this.baseUri}/TodoLists/UpdateTodoItem/${id}`, this._cast(command, UpdateTodoItemCommand)).pipe((map(x => <TodoItemDTO>x)));
  }

  updateTodoItemDetails(id: number | undefined, command: UpdateTodoItemDetailsCommand): Observable<TodoItemDTO> {
    return this.http.put(`${this.baseUri}/TodoLists/UpdateTodoItemDetails/${id}`, this._cast(command, UpdateTodoItemDetailsCommand)).pipe((map(x => <TodoItemDTO>x)));
  }

  deleteTodoItem(id: number | undefined): Observable<number> {
    return this.http.delete(`${this.baseUri}/TodoLists/DeleteTodoItem/${id}`).pipe((map(x => <number>x)));
  }

  _cast<K extends T, T>(obj: K, tClass: { new(...args: any[]): K }): K {
    let returnObject: K = new tClass();
    for (let p in returnObject) {
      const value = obj[p] || undefined;
      if (value != undefined) {
        returnObject[p] = value;
      }
    }
    return returnObject;
  }
}
