create schema dbo
go

create table Notes
(
	Id int identity constraint PK_Notes primary key,
	Name nvarchar(200) not null,
	Status int not null,
	Description nvarchar(max),
	DateModified datetime2 default getutcdate() not null
)
go

create table TodoItems
(
	Id int identity constraint PK_TodoItems primary key,
	Status int not null,
	DateModified datetime2 default getutcdate() not null,
	NoteId int constraint FK_TodoItems_Notes_NoteId references Notes,
	Name nvarchar(200) not null
)
go

create index IX_TodoItems_NoteId
	on TodoItems (NoteId)
go

