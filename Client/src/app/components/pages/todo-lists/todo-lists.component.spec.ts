import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TodoListsComponent } from './todo-lists.component';

describe('TodoListComponent', () => {
  let component: TodoListsComponent;
  let fixture: ComponentFixture<TodoListsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TodoListsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TodoListsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
