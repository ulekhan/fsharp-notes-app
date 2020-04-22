namespace Todos.Data

open System
open System.Data
open System.Data.Common
open System.Threading.Tasks
open Dapper
open System.Data.SqlClient
open Todos.Domain.Models

module DataProvider =
    let createConnection (settings: DataSettings) =
        let connection = new SqlConnection(settings.ConnectionString)
        connection

    let private executeTransaction<'T> (connection: SqlConnection) (command: SqlConnection -> DbTransaction -> Task<'T>) =
        async {
            do! connection.OpenAsync()
                |> Async.AwaitTask
                |> Async.Ignore
            let task = connection.BeginTransactionAsync()
            let! transaction = task.AsTask() |> Async.AwaitTask

            let! result =
                async {
                    try
                        try
                            let! commandResult = command connection transaction |> Async.AwaitTask
                            do! transaction.CommitAsync()
                                |> Async.AwaitTask
                                |> Async.Ignore
                            return Ok commandResult
                        with ex ->
                            Console.WriteLine ex
                            do! transaction.RollbackAsync()
                                |> Async.AwaitTask
                                |> Async.Ignore
                            return Error ex.Message
                    finally
                        connection.CloseAsync()
                        |> Async.AwaitTask
                        |> Async.Ignore
                        |> ignore
                }

            return result
        }

    let private insertNote' (note: Note) (connection: SqlConnection) (transaction: DbTransaction) =
        let sql = "INSERT INTO Notes (Name, Status, Description)
                               VALUES (@Name, @Status, @Description)
                               SELECT CAST(SCOPE_IDENTITY() as int)"

        let item =
            match note with
            | Tick tick ->
                {| Name = tick.Name
                   Status = tick.Status
                   Description = tick.Description |}
            | Todo todo ->
                {| Name = todo.Name
                   Status = todo.Status
                   Description = null |}

        let task = connection.QuerySingleAsync<int>(sql, item, transaction)
        task

    let createNote (note: Note) (connection: SqlConnection) =
        let bind conn = insertNote' note conn
        executeTransaction connection bind


    let mapRowsToRecords (reader: IDataReader) =
        let idIndex = reader.GetOrdinal "Id"
        let nameIndex = reader.GetOrdinal "Name"
        let descriptionIndex = reader.GetOrdinal "Description"
        let statusIndex = reader.GetOrdinal "Status"
        let dateIndex = reader.GetOrdinal "DateModified"

        [ while reader.Read() do
            let description = reader.GetString descriptionIndex

            let item =
                match description with
                | null ->
                    { Id = reader.GetInt32 idIndex
                      Name = reader.GetString nameIndex
                      Status = enum<NoteStatus> (reader.GetInt32 statusIndex)
                      Description = reader.GetString descriptionIndex |> Some
                      Items = None
                      DateModified = reader.GetDateTime dateIndex }
                | _ ->
                    { Id = reader.GetInt32 idIndex
                      Name = reader.GetString nameIndex
                      Status = enum<NoteStatus> (reader.GetInt32 statusIndex)
                      Description = None
                      Items = list.Empty |> Some
                      DateModified = reader.GetDateTime dateIndex }

            yield item ]

    let getNote (id: int) (connection: SqlConnection) =
        async {
            let sql = "SELECT * FROM Notes WHERE Id = @Id"
            let! reader = connection.ExecuteReaderAsync(sql, {| Id = id |}) |> Async.AwaitTask
            let items = reader |> mapRowsToRecords

            return match items with
                   | [ x ] -> x |> Ok
                   | _ -> sprintf "note with id %i does not exist" id |> Error
        }
