namespace Todos.Domain

open System
open Todos.Domain.Commands.Create
open Todos.Domain.Models

module NoteValidator =
    let private createTick (command: CreateTickCommand) =
        let creatTick () =
            NewTick
                { Name = command.Name
                  Description = command.Description }

        match command with
        | x when x.Description.Length > 50 -> Error "description length must be less than 50"
        | x when x.Description.Length = 0 -> Error "description cannot be empty"
        | x when x.Name.Length > 50 -> Error "name length must be less than 50"
        | x when x.Name.Length = 0 -> Error "name cannot be empty"
        | _ -> creatTick () |> Ok

    let private createTodo (command: CreateTodoCommand) =
        let creatTodo () =
            NewTodo
                { Name = command.Name
                  Items = command.Items }

        match command with
        | x when x.Items.Length > 50 -> Error "items length must be less than 50"
        | x when x.Items.Length = 0 -> Error "items cannot be empty"
        | x when x.Name.Length > 50 -> Error "name length must be less than 50"
        | x when x.Name.Length = 0 -> Error "name cannot be empty"
        | _ -> creatTodo () |> Ok

    let createNote (command: CreateNoteCommand) (date: DateTime): Result<NewNote, string> =
        match command with
        | c when c.Description.IsNone && c.Items |> List.isEmpty ->
            Error "neither items nor description is presented"
        | c when c.Description.IsSome && not (c.Items |> List.isEmpty) ->
            Error "both description and items cannot exist at the same time"
        | c when not c.Items.IsEmpty ->
            createTodo
                { Name = c.Name
                  Items = c.Items }
        | c when c.Description.IsSome ->
            createTick
                { Name = c.Name
                  Description = c.Description.Value }
        | _ -> Error "invalid command"
