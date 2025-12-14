namespace LibraryManagementSystem

open System
open System.Collections.Generic
open System.IO
open System.Text.Json

module LibraryManager =

    
    let DataChanged = new Event<unit>()
    let notifyChange () = DataChanged.Trigger()

    let mutable books = ResizeArray<Book>()
    let mutable filePath = "library.json"


    let AddBook (title: string) (author: string) =
        let newId = if books.Count = 0 then 1 else books.[books.Count - 1].Id + 1
        let book = { Id = newId; Title = title.Trim(); Author = author.Trim(); IsAvailable = true }
        books.Add(book)
        notifyChange()

    let UpdateBook (id: int) (newTitle: string) (newAuthor: string) =
        let idx = books |> Seq.tryFindIndex (fun b -> b.Id = id)
        match idx with
        | Some i when books.[i].IsAvailable ->
            books.[i] <- { books.[i] with Title = newTitle.Trim(); Author = newAuthor.Trim() }
            notifyChange()
            true
        | _ -> false

    let DeleteBook (id: int) =
        let idx = books |> Seq.tryFindIndex (fun b -> b.Id = id)
        match idx with
        | Some i when books.[i].IsAvailable ->
            books.RemoveAt(i) |> ignore
            for j in 0 .. books.Count - 1 do
                books.[j] <- { books.[j] with Id = j + 1 }
            notifyChange()
            true
        | _ -> false

    let GetAllBooks () = books |> Seq.toList
