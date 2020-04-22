namespace Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Todos.Api
open System.Data.SqlClient
open Giraffe.Serialization
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Todos.Data
open Giraffe

module Program =
    let exitCode = 0

    let configureApp (app : IApplicationBuilder) =
        app.UseRouting() |> ignore
        app.UseCors("default") |> ignore
        app.UseGiraffe Routes.handlers

    let configureServices (services : IServiceCollection)  =
        let serializerSettings = JsonSerializerSettings(Converters = [| IdiomaticDuConverter() |])
        
        let configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        let connectionString = configuration.GetConnectionString("mssql")
        
        let dataSettings: DataSettings = { ConnectionString = connectionString }
        services.AddScoped<SqlConnection>(fun c -> DataProvider.createConnection (dataSettings)) |> ignore

        services.AddGiraffe() |> ignore
        services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(serializerSettings)) |> ignore

        services.AddCors(fun e ->
            e.AddPolicy
                ("default",
                 (fun c ->
                     c.AllowAnyHeader() |> ignore
                     c.AllowCredentials() |> ignore
                     c.AllowAnyMethod() |> ignore)))
        |> ignore
    
    let configureAppConfiguration (_: WebHostBuilderContext) (config: IConfigurationBuilder) =
        config.AddJsonFile("appsettings.local.json", true) |>ignore
    
    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.Configure configureApp |> ignore
                webBuilder.ConfigureServices configureServices |> ignore
                webBuilder.ConfigureAppConfiguration configureAppConfiguration |> ignore)
            
    [<EntryPoint>]
    let main args =
        CreateHostBuilder(args).Build().Run()
        exitCode


