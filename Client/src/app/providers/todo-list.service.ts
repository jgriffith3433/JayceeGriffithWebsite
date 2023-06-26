import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { TodoListDTO } from "../models/TodoListDTO";
import { CreateOrUpdateTodoListDTO } from "../models/CreateOrUpdateTodoListDTO";
import { Config } from "./config";

@Injectable({
  providedIn: 'root'
})
export class TodoListService {
  constructor(private http: HttpClient) { }

  get baseUri() {
    return `${Config.api}`;
  }

  getById(id: number) {
    return this.http.get(`${this.baseUri}/TodoList/${id}`).pipe((map(x => <TodoListDTO>x)));
  }

  create(todoList: CreateOrUpdateTodoListDTO) {
    return this.http.post(`${this.baseUri}/TodoList`, todoList).pipe((map(x => <TodoListDTO>x)));
  }

  update(id: number, todoList: CreateOrUpdateTodoListDTO) {
    return this.http.put(`${this.baseUri}/TodoList/${id}`, todoList).pipe((map(x => <TodoListDTO>x)));
  }

  add(todoList: CreateOrUpdateTodoListDTO) {
    return this.http.post(`${this.baseUri}/TodoList`, todoList).pipe((map(x => <TodoListDTO>x)));
  }

  delete(id: number) {
    return this.http.delete(`${this.baseUri}/TodoList/${id}`);
  }
}
