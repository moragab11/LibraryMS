module Tests

open Xunit
open LibraryManagementSystem.LibraryManager
open System.IO
open LibraryManagementSystem

let private cleanup() =
    LibraryManager.books.Clear()
    if File.Exists("library.json") then File.Delete("library.json")

[<Fact>]
let ``Add book increases count`` () =
    cleanup()
    LoadData()
    AddBook "1984" "George Orwell"
    let books = GetAllBooks()
    Assert.Single(books)
    Assert.Equal("1984", books.Head.Title)

[<Fact>]
let ``Search works`` () =
    cleanup()
    LoadData()
    AddBook "F# Programming" "Ahmed"
    AddBook "C# Basics" "Mohamed"
    let results = SearchBooks "f#"
    Assert.Single(results)
    Assert.Equal("F# Programming", results.Head.Title)

[<Fact>]
let ``Borrow and Return works`` () =
    cleanup()
    LoadData()
    AddBook "Test Book" "Test Author"
    let book = GetAllBooks().Head
    Assert.True(BorrowBook book.Id)
    Assert.True(ReturnBook book.Id)

[<Fact>]
let ``Edit book updates data`` () =
    cleanup()
    LoadData()
    AddBook "Old Title" "Old Author"
    let book = GetAllBooks().Head
    Assert.True(UpdateBook book.Id "New Title" "New Author")
    let updated = GetAllBooks().Head
    Assert.Equal("New Title", updated.Title)

[<Fact>]
let ``Delete book removes it and renumbers`` () =
    cleanup()
    LoadData()
    AddBook "Book1" "A"
    AddBook "Book2" "B"
    let book2 = GetAllBooks() |> List.find (fun b -> b.Title = "Book2")
    Assert.True(DeleteBook book2.Id)
    let remaining = GetAllBooks()
    Assert.Single(remaining)
    Assert.Equal("Book1", remaining.Head.Title)