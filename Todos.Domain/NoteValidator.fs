namespace Todos.Domain

open System
open Todos.Domain.Commands
open Todos.Domain.Models

module NoteValidator =
    let private createTick (command: CreateTickCommand) (date: DateTime) =
        let creatTick () =
            Tick
                { Name = command.Name
                  Status = NoteStatus.Active
                  Description = command.Description
                  DateModified = date }

        match command with
        | x when x.Description.Length > 50 -> Error "description length must be less than 50"
        | x when x.Description.Length = 0 -> Error "description cannot be empty"
        | x when x.Name.Length > 50 -> Error "name length must be less than 50"
        | x when x.Name.Length = 0 -> Error "name cannot be empty"
        | _ -> creatTick () |> Ok

    let private createTodo (command: CreateTodoCommand) (date: DateTime) =
        let creatTodo () =
            Todo
                { Name = command.Name
                  Status = NoteStatus.Active
                  Items = command.Items
                  DateModified = date }

        match command with
        | x when x.Items.Length > 50 -> Error "items length must be less than 50"
        | x when x.Items.Length = 0 -> Error "items cannot be empty"
        | x when x.Name.Length > 50 -> Error "name length must be less than 50"
        | x when x.Name.Length = 0 -> Error "name cannot be empty"
        | _ -> creatTodo () |> Ok

    let createNote (command: CreateNoteCommand) (date: DateTime): Result<Note, string> =
        match command with
        | c when c.Description.IsNone && (c.Items.IsNone || c.Items.Value |> List.isEmpty) ->
            Error "neither items nor description is presented"
        | c when c.Description.IsSome && c.Items.IsSome && c.Items.Value
                                                           |> List.isEmpty
                                                           <> true ->
            Error "both description and items cannot exist at the same time"
        | c when c.Items.IsSome ->
            createTodo
                { Name = c.Name
                  Items = c.Items.Value } date
        | c when c.Description.IsSome ->
            createTick
                { Name = c.Name
                  Description = c.Description.Value } date
        | _ -> Error "invalid command"
