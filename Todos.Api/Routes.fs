namespace Todos.Api

open System.Data.SqlClient
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Todos.Domain.Commands.Create
open Todos.Processing

module Routes =

    type CommandResult =
        { data: obj option
          error: string option }

    let prepareResponse command =
        match command with
        | Error err ->
            { error = Some err
              data = None }
        | Ok value ->
            { data = Some(value :> obj)
              error = None }

    let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
        choose
            [ POST >=> route "/notes" >=> fun next context ->
                task {
                    let connection = context.GetService<SqlConnection>()
                    let! command = context.BindJsonAsync<CreateNoteCommand>()

                    let! commandResult = NoteHandler.createNote command connection
                    let response = commandResult |> prepareResponse
                    return! json response next context
                }

              GET >=> route "/notes" >=> fun next context ->
                  task {
                      let connection = context.GetService<SqlConnection>()
                      let! queryResult = NoteHandler.getNotes connection
                      let response = queryResult |> prepareResponse
                      return! json response next context
                  }

              GET >=> routef "/notes/%i" (fun id next context ->
                          task {
                              let connection = context.GetService<SqlConnection>()
                              let! queryResult = NoteHandler.getNote id connection
                              return! queryResult
                                      |> prepareResponse
                                      |> json
                                      <| next
                                      <| context
                          }) ]
