namespace Todos.Domain.Commands

open Todos.Domain.Models

type TodoResponseModel =
    { Id: int
      Name: string
      Description: string
      Status: NoteStatus
      Items: list<TodoItem> }

type CreateTodoCommand =
    { Name: string
      Items: list<TodoItem>}

[<CLIMutable>]
type CreateTickCommand =
    { Name: string
      Description: string}
    
[<CLIMutable>]
type CreateNoteCommand =
    { Name: string
      Description: Option<string>
      Items: Option<TodoItem list>}

type DeleteNoteCommand =
    { Id: int }

type GetNoteCommand =
    { Id: int }

type GetNotesCommand = {
    PageNumber: int
    Count: int
}

[<CLIMutable>]
type EditTodoCommand =
    { Id: int
      Name: string
      Status: NoteStatus
      Items: list<TodoItem>}

[<CLIMutable>]
type EditTickCommand =
    { Id: int
      Name: string
      Status: NoteStatus
      Description: string }

[<CLIMutable>]
type EditNoteCommand =
    {
        Id: int
        Name: string
        Status: NoteStatus
        Description: string option
        Items: TodoItem list option 
    }