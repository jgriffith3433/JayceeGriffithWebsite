import { Component, OnInit } from '@angular/core';
import { UntypedFormBuilder, UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateOrUpdateTodoListDTO } from 'src/app/models/CreateOrUpdateTodoListDTO';
import { TodoListService } from 'src/app/providers/todo-list.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-todo-list',
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.scss']
})
export class TodoListComponent implements OnInit {

  id: any = 0;
  todoList: CreateOrUpdateTodoListDTO = {
    title: '',
    color: '#FFFFFF',
  };
  form: UntypedFormGroup;
  isEdit: boolean = false;
  errors: string[] = [];
  isError: boolean = false;

  constructor(private activatedRoute: ActivatedRoute, private todoListService: TodoListService, private formBuilder: UntypedFormBuilder, private router: Router) {
    this.form = this.formBuilder.group({
      title: new UntypedFormControl(this.todoList.title, []),
      color: new UntypedFormControl(this.todoList.color, [])
    })
  }

  ngOnInit(): void {
    this.activatedRoute.paramMap.subscribe((map) => {
      if (map.has('id')) {
        this.id = map.get('id');
        this.todoListService.getById(this.id).subscribe(x => {
          this.todoList = {
            title: x.title,
            color: x.color
          }
          this.form.setValue(this.todoList);
          this.isEdit = true;
        })
      }
    });
  }

  public get TodoList() {
    if (this.todoList === null) return null;
    else return this.todoList;
  }

  save() {
    console.log(this.form.value);
    if (this.isEdit) {
      this.todoListService.update(this.id, <CreateOrUpdateTodoListDTO>this.form.value).subscribe({
        next: res => {
          this.back();
        },
        error: (err) => {
          if (err.ok === false) {
            this.isError = true;
            let e = err.error;
            this.errors = <string[]>e.errors;
          }
        }
      });
    } else {
      this.todoListService.add(<CreateOrUpdateTodoListDTO>this.form.value).subscribe({
        next: res => {
          this.back();
        },
        error: (err) => {
          if (err.ok === false) {
            this.isError = true;
            let e = err.error;
            this.errors = <string[]>e.errors;
          }
        }
      });
    }
  }

  delete() {
    let response = confirm('Are you sure you want to delete?');
    if (response === true) {
      this.todoListService.delete(this.id).subscribe({
        next: res => {
          this.back();
        },
        error: (err) => {
          if (err.ok === false) {
            this.isError = true;
            let e = err.error;
            this.errors = <string[]>e.errors;
          }
        }
      });
    }
  }

  back() {
    this.router.navigate(['todo']);
  }
}
