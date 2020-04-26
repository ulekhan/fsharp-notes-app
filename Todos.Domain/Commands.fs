namespace Todos.Domain.Commands

open Todos.Domain.Models

module Response =
    type TodoResponseModel =
        { Id: int
          Name: string
          Description: string
          Status: NoteStatus
          Items: list<TodoItem> }

module Create =

    [<CLIMutable>]
    type CreateTodoCommand =
        { Name: string
          Items: NewTodoItem list }

    [<CLIMutable>]
    type CreateTickCommand =
        { Name: string
          Description: string }

    [<CLIMutable>]
    type CreateNoteCommand =
        { Name: string
          Description: Option<string>
          Items: NewTodoItem list }

module Delete =
    type DeleteNoteCommand =
        { Id: int }

module Get =
    type GetNoteQuery =
        { Id: int }

    type GetNotesQuery =
        { PageNumber: int
          Count: int }

module Edit =
    [<CLIMutable>]
    type EditTodoCommand =
        { Id: int
          Name: string
          Status: NoteStatus
          Items: list<TodoItem> }

    [<CLIMutable>]
    type EditTickCommand =
        { Id: int
          Name: string
          Status: NoteStatus
          Description: string }

    [<CLIMutable>]
    type EditNoteCommand =
        { Id: int
          Name: string
          Status: NoteStatus
          Description: string option
          Items: TodoItem list option }
