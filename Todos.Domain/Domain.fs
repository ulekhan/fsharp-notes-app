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
    { Name: string
      Status: NoteStatus
      Items: TodoItem seq
      DateModified: DateTime }

type Tick =
    { Name: string
      Description: string
      Status: NoteStatus
      DateModified: DateTime }
    
type Note =
    | Tick of Tick
    | Todo of Todo

type TodoResponseModel =
    { Id: int
      Name: string
      Status: NoteStatus
      Description: string option
      Items: TodoItem list option
      DateModified: DateTime }