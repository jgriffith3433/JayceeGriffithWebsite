import { Component, OnInit, TemplateRef } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { CreateTodoItemCommand } from '../../../models/CreateTodoItemCommand';
import { CreateTodoListCommand } from '../../../models/CreateTodoListCommand';
import { PriorityLevelDTO } from '../../../models/PriorityLevelDTO';
import { TodoItemDTO } from '../../../models/TodoItemDTO';
import { TodoListDTO } from '../../../models/TodoListDTO';
import { UpdateTodoItemCommand } from '../../../models/UpdateTodoItemCommand';
import { UpdateTodoItemDetailsCommand } from '../../../models/UpdateTodoItemDetailsCommand';
import { UpdateTodoListCommand } from '../../../models/UpdateTodoListCommand';
import { TodoListsService } from '../../../providers/todo-lists.service';

@Component({
  selector: 'app-todo-list',
  templateUrl: './todo-lists.component.html',
  styleUrls: ['./todo-lists.component.scss']
})
export class TodoListsComponent implements OnInit {
  lists: TodoListDTO[];
  priorityLevels: PriorityLevelDTO[];
  selectedList: TodoListDTO | undefined;
  selectedItem: TodoItemDTO | undefined;
  newListEditor: any = {};
  listOptionsEditor: any = {};
  itemDetailsEditor: any = {};
  newListModalRef: BsModalRef;
  listOptionsModalRef: BsModalRef;
  deleteListModalRef: BsModalRef;
  itemDetailsModalRef: BsModalRef;

  constructor(
    private todoListsService: TodoListsService,
    private modalService: BsModalService
  ) { }

  ngOnInit(): void {
    this.todoListsService.getAll().subscribe(
      result => {
        if (result.lists && result.priorityLevels) {
          this.lists = result.lists;
          this.priorityLevels = result.priorityLevels;
          if (this.lists.length) {
            this.selectedList = this.lists[0];
          }
        }
      },
      error => console.error(error)
    );
  }


  // Lists
  remainingItems(list: TodoListDTO): number | undefined {
    return list.items?.filter(t => !t.done).length;
  }

  showNewListModal(template: TemplateRef<any>): void {
    this.newListModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('title')?.focus(), 250);
  }

  newListCancelled(): void {
    this.newListModalRef.hide();
    this.newListEditor = {};
  }

  addList(): void {
    const list = {
      id: 0,
      title: this.newListEditor.title,
      color: '#FFFFFF',
      items: []
    } as TodoListDTO;

    this.todoListsService.create(list as CreateTodoListCommand).subscribe(
      result => {
        list.id = result;
        this.lists.push(list);
        this.selectedList = list;
        this.newListModalRef.hide();
        this.newListEditor = {};
      },
      error => {
        const errors = error.errors;

        if (errors && errors.Title) {
          this.newListEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('title')?.focus(), 250);
      }
    );
  }

  showListOptionsModal(template: TemplateRef<any>) {
    if (this.selectedList) {
      this.listOptionsEditor = {
        id: this.selectedList.id,
        title: this.selectedList.title
      };
    }
    this.listOptionsModalRef = this.modalService.show(template);
  }

  updateListOptions() {
    const list = this.listOptionsEditor as UpdateTodoListCommand;
    this.todoListsService.update(this.selectedList?.id, list).subscribe(
      result => {
        if (this.selectedList) {
          this.selectedList.title = this.listOptionsEditor.title;
          this.listOptionsModalRef.hide();
          this.listOptionsEditor = {};
        }
      },
      error => console.error(error)
    );
  }

  confirmDeleteList(template: TemplateRef<any>) {
    this.listOptionsModalRef.hide();
    this.deleteListModalRef = this.modalService.show(template);
  }

  deleteListConfirmed(): void {
    if (this.selectedList) {
      this.todoListsService.delete(this.selectedList.id).subscribe(
        result => {
          this.deleteListModalRef.hide();
          this.lists = this.lists.filter(t => t.id !== this.selectedList?.id);
          this.selectedList = this.lists.length ? this.lists[0] : undefined;
        },
        error => console.error(error)
      );
    }
  }

  // Items
  showItemDetailsModal(template: TemplateRef<any>, item: TodoItemDTO): void {
    this.selectedItem = item;
    this.itemDetailsEditor = {
      ...this.selectedItem
    };

    this.itemDetailsModalRef = this.modalService.show(template);
    this.itemDetailsModalRef.onHide?.subscribe(() => {
      this.selectedItem = undefined;
    });
  }

  updateItemDetails(): void {
    const item = this.itemDetailsEditor as UpdateTodoItemDetailsCommand;
    this.todoListsService.updateTodoItemDetails(this.selectedItem?.id, item).subscribe(
      result => {
        if (this.selectedList && this.selectedItem) {
          if (this.selectedItem.listId !== this.itemDetailsEditor.listId) {
            this.selectedList.items = this.selectedList.items?.filter(
              i => i.id !== this.selectedItem?.id
            );
            const listIndex = this.lists.findIndex(
              l => l.id === this.itemDetailsEditor.listId
            );
            this.selectedItem.listId = this.itemDetailsEditor.listId;
            this.lists[listIndex].items?.push(this.selectedItem);
          }

          this.selectedItem.priority = this.itemDetailsEditor.priority;
          this.selectedItem.note = this.itemDetailsEditor.note;
          this.itemDetailsModalRef.hide();
          this.itemDetailsEditor = {};
        }
      },
      error => {
        this.itemDetailsEditor.errors = error.errors;
        if (this.itemDetailsEditor.errors && this.itemDetailsEditor.errors.Priority) {
          setTimeout(() => document.getElementById('priority')?.focus(), 250);
        }
      }
    );
  }

  addItem() {
    if (this.selectedList && this.selectedList.items) {
      const item = {
        id: 0,
        listId: this.selectedList.id,
        priority: this.priorityLevels[0].value,
        title: '',
        done: false
      } as TodoItemDTO;

      this.selectedList.items.push(item);
      const index = this.selectedList.items.length - 1;
      this.editItem(item, 'itemTitle' + index);
    }
  }

  editItem(item: TodoItemDTO, inputId: string): void {
    this.selectedItem = item;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateItem(item: TodoItemDTO, pressedEnter: boolean = false): void {
    const isNewItem = item.id === 0;

    if (!item.title?.trim()) {
      this.deleteItem(item);
      return;
    }

    if (item.id === 0) {
      this.todoListsService
        .createTodoItem({
          ...item, listId: this.selectedList?.id
        } as CreateTodoItemCommand)
        .subscribe(
          result => {
            item.id = result;
          },
          error => console.error(error)
        );
    } else {
      this.todoListsService.updateTodoItem(item.id, item).subscribe(
        () => console.log('Update succeeded.'),
        error => console.error(error)
      );
    }

    this.selectedItem = undefined;

    if (isNewItem && pressedEnter) {
      setTimeout(() => this.addItem(), 250);
    }
  }

  deleteItem(item: TodoItemDTO | undefined) {
    if (this.itemDetailsModalRef) {
      this.itemDetailsModalRef.hide();
    }
    if (this.selectedList && this.selectedList.items && this.selectedItem && item) {
      if (item.id === 0) {
        const itemIndex = this.selectedList.items.indexOf(this.selectedItem);
        this.selectedList.items.splice(itemIndex, 1);
      } else {
        this.todoListsService.deleteTodoItem(item.id).subscribe(
          result => {
            if (this.selectedList && this.selectedList.items) {
              this.selectedList.items = this.selectedList.items.filter(t => t.id !== item.id);
            }
          },
          error => console.error(error)
        );
      }
    }
  }
}
