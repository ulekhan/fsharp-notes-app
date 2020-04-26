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

    let private insertNote' (note: NewNote) (connection: SqlConnection) (transaction: DbTransaction) =
        async {
            let noteSql = "INSERT INTO Notes (Name, Status, Description)
                                   VALUES (@Name, @Status, @Description)
                                   SELECT CAST(SCOPE_IDENTITY() as int)"

            let todoItemSql = "INSERT INTO TodoItems (Name, Status, NoteId)
                                   VALUES (@Name, @Status, @NoteId) "

            let note =
                match note with
                | NewTick tick ->
                    {| tick with
                           Status = NoteStatus.Active
                           Items = List.empty
                           Description = tick.Description |}
                | NewTodo todo ->
                    {| todo with
                           Status = NoteStatus.Active
                           Items = todo.Items
                           Description = null |}

            let! noteId = connection.QuerySingleAsync<int>(noteSql, note, transaction) |> Async.AwaitTask

            for item in note.Items |> List.map (fun p -> {| p with NoteId = noteId |}) do
                do! connection.ExecuteAsync(todoItemSql, item, transaction)
                    |> Async.AwaitTask
                    |> Async.Ignore

            return noteId
        }
        |> Async.StartAsTask

    let createNote (note: NewNote) (connection: SqlConnection) =
        let bind conn = insertNote' note conn
        executeTransaction<int> connection bind

    let mapRowsToRecords (reader: IDataReader) =
        let idIndex = reader.GetOrdinal "noteId"
        let nameIndex = reader.GetOrdinal "noteName"
        let descriptionIndex = reader.GetOrdinal "noteDescription"
        let statusIndex = reader.GetOrdinal "noteStatus"
        let dateIndex = reader.GetOrdinal "noteDateModified"
        let itemId = reader.GetOrdinal "itemId"
        let itemStatus = reader.GetOrdinal "itemStatus"
        let itemDateModified = reader.GetOrdinal "itemDateModified"
        let itemName = reader.GetOrdinal "itemName"

        [ while reader.Read() do
            let hasDescription = not (reader.IsDBNull descriptionIndex)

            let todo =
                match hasDescription with
                | true ->
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

            let item =
                if not <| reader.IsDBNull itemId then
                    Some
                        { Id = reader.GetInt32 itemId
                          Name = reader.GetString itemName
                          Status = enum<TodoStatus> (reader.GetInt32 itemStatus)
                          DateModified = reader.GetDateTime itemDateModified }
                else
                    None

            yield (todo, item) ]

    let getNote (id: int) (connection: SqlConnection) =
        async {
            let sql = "SELECT n.id as noteId,
                              n.name as noteName,
                              n.status as noteStatus,
                              n.description as noteDescription,
                              n.dateModified as noteDateModified,
                              i.id as itemId,
                              i.status as itemStatus,
                              i.dateModified as itemDateModified,
                              i.name as itemName
                       FROM Notes as n left join TodoItems i on n.Id = i.NoteID WHERE n.Id = @Id"
            let! reader = connection.ExecuteReaderAsync(sql, {| Id = id |}) |> Async.AwaitTask
            let items = reader |> mapRowsToRecords

            return match items with
                   | [] -> sprintf "note with id %i does not exist" id |> Error
                   | _ ->
                       let note =
                           items
                           |> List.head
                           |> fst

                       let todoItems =
                           items
                           |> List.filter (fun it -> (snd it) |> Option.isSome)
                           |> List.map (fun p -> snd p)

                       Ok {| note with Items = todoItems |}
        }

    let getNotes (connection: SqlConnection) =
        async {
            let sql = "SELECT n.id as noteId,
                              n.name as noteName,
                              n.status as noteStatus,
                              n.description as noteDescription,
                              n.dateModified as noteDateModified,
                              i.id as itemId,
                              i.status as itemStatus,
                              i.dateModified as itemDateModified,
                              i.name as itemName
                       FROM Notes as n left join TodoItems i on n.Id = i.NoteId"
            let! reader = connection.ExecuteReaderAsync(sql, {| Id = id |}) |> Async.AwaitTask
            let items = reader |> mapRowsToRecords

            return items
                   |> List.groupBy (fun (note, _) -> note)
                   |> List.map (fun tuple ->
                       {| fst tuple with
                              Items =
                                  (snd tuple)
                                  |> List.filter (fun t -> (snd t) |> Option.isSome)
                                  |> List.map (fun t -> snd t) |})
                   |> Ok
        }
