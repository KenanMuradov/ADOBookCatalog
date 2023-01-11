using ADOBookCatalog.Models;
using ADOBookCatalog.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ADOBookCatalog;

public partial class MainWindow : Window
{
    DbConnection? connection = null;

    public MainWindow(string? connectionString)
    {
        InitializeComponent();
        DataContext = this;
        connection = new SqlConnection(connectionString);

    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            connection?.Open();

            using SqlCommand command = new SqlCommand("SELECT * FROM Authors", (SqlConnection)connection);
            var result = command.ExecuteReader();

            int line = 0;

            while (result.Read())
            {
                if (line == 0)
                {
                    line++;
                    continue;
                }

                int? id = result["Id"] as int?;
                string? firstName = result["FirstName"] as string;
                string? lastName = result["LastName"] as string;

                AuthorsCombobox.Items.Add(id + " " + firstName + " " + lastName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    private void AuthorsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CategoriesCombobox.IsEnabled)
            CategoriesCombobox.IsEnabled = !CategoriesCombobox.IsEnabled;

        CategoriesCombobox.Items.Clear();

        try
        {
            connection?.Open();

            var id = AuthorsCombobox.SelectedItem.ToString().Split(' ')[0];

            using SqlCommand command = new SqlCommand($"SELECT DISTINCT Categories.[Name] FROM Categories\r\nJOIN Books ON Id_Category = Categories.Id\r\nJOIN Authors ON Id_Author = Authors.Id\r\nWHERE Authors.Id = {id}", (SqlConnection)connection);
            var result = command.ExecuteReader();

            while (result.Read())
                CategoriesCombobox.Items.Add(result["Name"] as string);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    private void CategoriesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoriesCombobox.Items.IsEmpty)
            return;

        try
        {
            connection?.Open();

            var id = AuthorsCombobox.SelectedItem.ToString().Split(' ')[0];
            var name = CategoriesCombobox.SelectedItem.ToString();

            using SqlCommand command = new SqlCommand($"SELECT * FROM Books\r\nJOIN Categories ON Categories.Id = Id_Category \r\nJOIN Authors ON Authors.Id = Id_Author \r\nWHERE Categories.Name = '{name}' AND Id_Author = {id}\r\n", (SqlConnection)connection);
            var result = command.ExecuteReader();

            ListBooks.Items.Clear();

            while (result.Read())
            {
                var bookId = result["Id"] as int?;
                var bookName = result["Name"] as string;
                var bookPages = result["Pages"] as int?;
                var bookYearPress = result["YearPress"] as int?;
                var IdAuthor = result["Id_Author"] as int?;
                var IdTheme = result["Id_Themes"] as int?;
                var IdCategory = result["Id_Category"] as int?;
                var IdPress = result["Id_Press"] as int?;
                var bookComment = result["Comment"] as string;
                var bookQuantity = result["Quantity"] as int?;

                var book = new Book(bookId, bookName, bookPages, bookYearPress, bookComment, bookQuantity, IdAuthor, IdTheme, IdCategory, IdPress);
                ListBooks.Items.Add(book);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    private void ButtonSearch_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchTxt.Text))
            return;

        try
        {
            connection?.Open();

            using SqlCommand command = new SqlCommand($"SELECT * FROM Books\r\nWHERE Name LIKE '%{SearchTxt.Text}%'", (SqlConnection)connection);
            var result = command.ExecuteReader();

            ListBooks.Items.Clear();

            while (result.Read())
            {
                var bookId = result["Id"] as int?;
                var bookName = result["Name"] as string;
                var bookPages = result["Pages"] as int?;
                var bookYearPress = result["YearPress"] as int?;
                var IdAuthor = result["Id_Author"] as int?;
                var IdTheme = result["Id_Themes"] as int?;
                var IdCategory = result["Id_Category"] as int?;
                var IdPress = result["Id_Press"] as int?;
                var bookComment = result["Comment"] as string;
                var bookQuantity = result["Quantity"] as int?;
                
                var book = new Book(bookId, bookName, bookPages, bookYearPress, bookComment, bookQuantity,IdAuthor,IdTheme,IdCategory,IdPress);
                ListBooks.Items.Add(book);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        AddView addView = new(connection);

        addView.ShowDialog();
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ListBooks.SelectedItem is null)
            return;

        try
        {
            connection?.Open();

            var id = (ListBooks.SelectedItem as Book).Id;

            SqlCommand command = new SqlCommand($"DELETE FROM Books WHERE Id = {id}", (SqlConnection)connection);
            command.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            connection?.Close();
        }
    }

    private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (ListBooks.SelectedItem is null)
            return;

        var book = ListBooks.SelectedItem as Book;

        if (book != null)
        {
            UpdateView updateView = new(connection, book.Id, book.Name, book.Pages, book.YearPress, book.IdAuthor, book.IdCategory, book.IdTheme, book.IdPress, book.Comment, book.Quantity);

            updateView.ShowDialog();
        }
    }
}
