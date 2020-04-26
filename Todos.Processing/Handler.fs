namespace Todos.Processing

open System.Data.SqlClient
open Todos.Data
open Todos.Domain
open System
open Todos.Domain.Commands.Create

module NoteHandler =
 
    let getNote id = DataProvider.getNote id

    let createNote (command: CreateNoteCommand) (connection: SqlConnection) =
        async {
            let note = NoteValidator.createNote command DateTime.UtcNow
            match note with
            | Error err -> return Error err
            | Ok note -> return! DataProvider.createNote note connection
        }
        
    let getNotes = DataProvider.getNotes
