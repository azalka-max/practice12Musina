using Library.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Library.Pages
{
    public partial class BooksEditPage : Page
    {
        private int? _bookId;

        public BooksEditPage(int? bookId = null)
        {
            InitializeComponent();
            _bookId = bookId;
            Loaded += BooksEditPage_Loaded;
        }

        private void BooksEditPage_Loaded(object sender, RoutedEventArgs e)
        {
            LBAuthors.DisplayMemberPath = "FIO";

            InitStatus();
            InitGenres();
            LoadAuthors();
            if (_bookId.HasValue) LoadBook(_bookId.Value);
        }

        private void InitStatus()
        {
            CBStatus.ItemsSource = new[] { "Available", "Issued", "Reserved" };
        }

        private void InitGenres()
        {
            try
            {
                var genres = App.DB.Books
                    .Select(b => b.Genre)
                    .Distinct()
                    .Where(g => g != null)
                    .OrderBy(g => g)
                    .ToList();

                CBGenre.ItemsSource = genres;
            }
            catch
            {
                CBGenre.ItemsSource = new List<string>();
            }
        }

        private void LoadAuthors()
        {
            try
            {
                var authors = App.DB.Authors.OrderBy(a => a.FIO).ToList();
                LBAuthors.ItemsSource = authors;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список авторов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBook(int id)
        {
            try
            {
                var book = App.DB.Books.Find(id);
                if (book == null)
                {
                    MessageBox.Show("Книга не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TBName.Text = book.Name ?? "";
                TBYear.Text = book.YearOfPublicatin.HasValue ? book.YearOfPublicatin.Value.ToString() : "";
                if (!string.IsNullOrEmpty(book.Genre))
                    CBGenre.SelectedItem = book.Genre;
                if (!string.IsNullOrEmpty(book.Status))
                    CBStatus.SelectedItem = book.Status;

                var selectedAuthorIds = App.DB.BookAuthors
                    .Where(ba => ba.IdBooks == book.IdBooks)
                    .Select(ba => ba.IdAuthors)
                    .ToList();

                if (!selectedAuthorIds.Any() && book.IdAuthors.HasValue)
                    selectedAuthorIds.Add(book.IdAuthors.Value);

                var items = LBAuthors.ItemsSource as IEnumerable<object>;
                if (items != null)
                {
                    LBAuthors.SelectedItems.Clear();
                    var authorsList = items.Cast<dynamic>().ToList();
                    foreach (var a in authorsList)
                    {
                        int aid = (int)a.IdAuthors;
                        if (selectedAuthorIds.Contains(aid))
                            LBAuthors.SelectedItems.Add(a);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке книги: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = TBName.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Введите название книги", "Валидация", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Books book;
                if (_bookId.HasValue)
                {
                    book = App.DB.Books.Find(_bookId.Value);
                    if (book == null)
                    {
                        MessageBox.Show("Книга не найдена для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    book = new Books();
                    App.DB.Books.Add(book);
                }

                book.Name = name;

                if (int.TryParse(TBYear.Text?.Trim(), out int y))
                    book.YearOfPublicatin = y;
                else
                    book.YearOfPublicatin = null;

                book.Genre = CBGenre.SelectedItem as string;
                book.Status = CBStatus.SelectedItem as string;

                App.DB.SaveChanges();

                var old = App.DB.BookAuthors.Where(ba => ba.IdBooks == book.IdBooks).ToList();
                foreach (var o in old) App.DB.BookAuthors.Remove(o);


                if (LBAuthors.SelectedItems != null && LBAuthors.SelectedItems.Count > 0)
                {
                    foreach (var sel in LBAuthors.SelectedItems)
                    {

                        dynamic a = sel;
                        int aid = (int)a.IdAuthors;
                        App.DB.BookAuthors.Add(new BookAuthors { IdBooks = book.IdBooks, IdAuthors = aid });
                    }

                    dynamic first = LBAuthors.SelectedItems[0];
                    book.IdAuthors = (int)first.IdAuthors;
                }
                else
                {
                    book.IdAuthors = null;
                }

                App.DB.SaveChanges();

                MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                if (NavigationService != null && NavigationService.CanGoBack) NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
