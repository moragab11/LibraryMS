namespace LibraryManagementSystem

open System
open System.Drawing
open System.Windows.Forms
open LibraryManagementSystem.LibraryManager

type MainForm() as this =
    inherit Form()

    // العناصر
    let lblTitle = new Label(Text = "F# Library Management System", Dock = DockStyle.Top, Height = 60,
                            TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 20.0f, FontStyle.Bold),
                            ForeColor = Color.DarkBlue)

    let searchPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 70, Padding = Padding(15))
    let txtSearch = new TextBox(Width = 350, Font = new Font("Segoe UI", 11.0f))
    let btnSearch = new Button(Text = "Search", Width = 110, Height = 35)

    let inputPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 110, Padding = Padding(15))
    let txtTitle = new TextBox(Width = 220, PlaceholderText = "Book Title")
    let txtAuthor = new TextBox(Width = 220, PlaceholderText = "Author Name")
    let btnAdd = new Button(Text = "Add Book", Width = 130, Height = 40, BackColor = Color.ForestGreen, ForeColor = Color.White,
                            Font = new Font("Segoe UI", 10.0f, FontStyle.Bold))

    let actionPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 80, Padding = Padding(15))
    let txtId = new TextBox(Width = 100, PlaceholderText = "ID")
    let btnBorrow = new Button(Text = "Borrow", Width = 100, Height = 35, BackColor = Color.Orange)
    let btnReturn = new Button(Text = "Return", Width = 100, Height = 35, BackColor = Color.RoyalBlue, ForeColor = Color.White)
    let btnEdit = new Button(Text = "Edit", Width = 90, Height = 35, BackColor = Color.Goldenrod)
    let btnDelete = new Button(Text = "Delete", Width = 90, Height = 35, BackColor = Color.Crimson, ForeColor = Color.White)
    let btnSaveEdit = new Button(Text = "Save Edit", Width = 110, Height = 35, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, Visible = false)

    let lstBooks = new ListView(Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true,
                                Font = new Font("Segoe UI", 10.5f))

    let mutable editingId = -1

    let refreshList () =
        lstBooks.Items.Clear()
        for book in GetAllBooks() do
            let item = ListViewItem([| string book.Id; book.Title; book.Author; if book.IsAvailable then "Available" else "Borrowed" |])
            item.BackColor <- if book.IsAvailable then Color.White else Color.LightCoral
            lstBooks.Items.Add(item) |> ignore

    do
        // الأعمدة
        lstBooks.Columns.Add("ID", 70) |> ignore
        lstBooks.Columns.Add("Title", 300) |> ignore
        lstBooks.Columns.Add("Author", 220) |> ignore
        lstBooks.Columns.Add("Status", 120) |> ignore

        // ترتيب العناصر
        searchPanel.Controls.AddRange([| new Label(Text = "Search:", Width = 70); txtSearch; btnSearch |])
        inputPanel.Controls.AddRange([| new Label(Text = "Title:", Width = 60); txtTitle; new Label(Text = " Author:", Width = 70); txtAuthor; btnAdd |])
        actionPanel.Controls.AddRange([|
            new Label(Text = "Book ID:", Width = 80)
            txtId
            btnBorrow
            btnReturn
            btnEdit
            btnDelete
            btnSaveEdit
        |])

        this.Controls.AddRange([| lstBooks; actionPanel; inputPanel; searchPanel; lblTitle |])

        this.Text <- "F# Library Management System - Full CRUD"
        this.Size <- Size(940, 680)
        this.StartPosition <- FormStartPosition.CenterScreen

        // تحميل البيانات عند التشغيل
        LoadData()
        this.Load.Add(fun _ -> refreshList())

        // ===================== الأحداث =====================

        btnSearch.Click.Add(fun _ ->
            let results = SearchBooks txtSearch.Text
            lstBooks.Items.Clear()
            for book in results do
                let item = ListViewItem([| string book.Id; book.Title; book.Author; if book.IsAvailable then "Available" else "Borrowed" |])
                item.BackColor <- if book.IsAvailable then Color.White else Color.LightCoral
                lstBooks.Items.Add(item) |> ignore
        )

        btnAdd.Click.Add(fun _ ->
            if String.IsNullOrWhiteSpace(txtTitle.Text) || String.IsNullOrWhiteSpace(txtAuthor.Text) then
                MessageBox.Show("Please enter both title and author.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
            else
                AddBook txtTitle.Text txtAuthor.Text
                txtTitle.Clear(); txtAuthor.Clear()
                refreshList()
        )

        btnBorrow.Click.Add(fun _ ->
            match Int32.TryParse(txtId.Text) with
            | (true, id) ->
                if BorrowBook id then
                    MessageBox.Show("Book borrowed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                    txtId.Clear(); refreshList()
                else
                    MessageBox.Show("Book is not available or doesn't exist.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
            | _ -> MessageBox.Show("Please enter a valid ID.", "Invalid ID", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
        )

        btnReturn.Click.Add(fun _ ->
            match Int32.TryParse(txtId.Text) with
            | (true, id) ->
                if ReturnBook id then
                    MessageBox.Show("Book returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                    txtId.Clear(); refreshList()
                else
                    MessageBox.Show("This book wasn't borrowed.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
            | _ -> MessageBox.Show("Please enter a valid ID.", "Invalid ID", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
        )

        // تعديل الكتاب
        btnEdit.Click.Add(fun _ ->
            if lstBooks.SelectedItems.Count = 0 then
                MessageBox.Show("Please select a book to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
            else
                let item = lstBooks.SelectedItems.[0]
                let id = int item.SubItems.[0].Text
                let book = GetAllBooks() |> List.tryFind (fun b -> b.Id = id)
                match book with
                | Some b when not b.IsAvailable ->
                    MessageBox.Show("Cannot edit: This book is currently BORROWED.", "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Stop) |> ignore
                | Some b ->
                    editingId <- id
                    txtTitle.Text <- b.Title
                    txtAuthor.Text <- b.Author
                    btnSaveEdit.Visible <- true
                    btnAdd.Enabled <- false
                | None -> ()
        )

        btnSaveEdit.Click.Add(fun _ ->
            if editingId > 0 then
                if UpdateBook editingId txtTitle.Text txtAuthor.Text then
                    MessageBox.Show("Book updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                    editingId <- -1
                    txtTitle.Clear(); txtAuthor.Clear()
                    btnSaveEdit.Visible <- false
                    btnAdd.Enabled <- true
                    refreshList()
                else
                    MessageBox.Show("Failed to update: Book might be borrowed or not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        )

        // حذف الكتاب
        btnDelete.Click.Add(fun _ ->
            if lstBooks.SelectedItems.Count = 0 then
                MessageBox.Show("Please select a book to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
            else
                let item = lstBooks.SelectedItems.[0]
                let id = int item.SubItems.[0].Text
                let title = item.SubItems.[1].Text
                let book = GetAllBooks() |> List.tryFind (fun b -> b.Id = id)

                match book with
                | Some b when not b.IsAvailable ->
                    MessageBox.Show($"Cannot delete \"{title}\"\nThis book is currently BORROWED.", "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Stop) |> ignore
                | Some _ ->
                    if MessageBox.Show($"Are you sure you want to delete \"{title}\"?", "Confirm Delete", 
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes then
                        if DeleteBook id then
                            MessageBox.Show("Book deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                            refreshList()
                | None -> ()
        )

        // حفظ البيانات عند الإغلاق
        this.FormClosing.Add(fun _ -> SaveData())