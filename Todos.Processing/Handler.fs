namespace Todos.Processing

open System.Data.SqlClient
open Todos.Data
open Todos.Domain.Commands
open Todos.Domain
open System

module NoteHandler =

    let getNote id = DataProvider.getNote id

    let createNote (command: CreateNoteCommand) (connection: SqlConnection) =
        async {
            let note = NoteValidator.createNote command DateTime.UtcNow
            match note with
            | Error err -> return Error err
            | Ok note -> return! DataProvider.createNote note connection
        }
