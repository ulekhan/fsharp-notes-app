namespace Todos.Domain.Models

open System

type NoteStatus =
    | Active = 1
    | Archived = 2

type TodoStatus =
    | New = 1
    | Completed = 2

type TodoItem =
    { Id: int
      Name: string
      Status: TodoStatus
      DateModified: DateTime }

type Todo =
    { Id: int
      Name: string
      Status: NoteStatus
      Items: TodoItem list
      DateModified: DateTime }

type Tick =
    { Id: int
      Name: string
      Description: string
      Status: NoteStatus
      DateModified: DateTime }

type Note =
    | Tick of Tick
    | Todo of Todo

type NewTodoItem =
    { Name: string
      Status: TodoStatus }
    
type NewTodo =
    { Name: string
      Items: NewTodoItem list }

type NewTick =
    { Name: string
      Description: string }

type NewNote =
    | NewTick of NewTick
    | NewTodo of NewTodo

type TodoResponseModel =
    { Id: int
      Name: string
      Status: NoteStatus
      Description: string option
      Items: TodoItem list option
      DateModified: DateTime }