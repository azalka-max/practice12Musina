using Library.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    /// <summary>
    /// Логика взаимодействия для BooksPage.xaml
    /// </summary>
    public partial class BooksPage : Page
    {
        public BooksPage()
        {
            InitializeComponent();
            this.Loaded += BooksPage_Loaded;
            LVBooks.ItemsSource = App.DB.Books.ToList();
        }

        private void BooksPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDataFromDb();
        }

        private void LoadDataFromDb()
        {
            var list = (from b in App.DB.Books
                        join a in App.DB.Authors on b.IdAuthors equals a.IdAuthors into gj
                        from a in gj.DefaultIfEmpty()
                        select new BookViewModel
                        {
                            IdBooks = b.IdBooks,
                            Name = b.Name,
                            Genre = b.Genre,
                            FIO = a != null ? a.FIO : string.Empty
                        }).ToList();

            LVBooks.ItemsSource = list;

            CBGenre.ItemsSource = list
                .Where(x => !string.IsNullOrEmpty(x.Genre))
                .Select(x => x.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
            CBGenre.SelectedIndex = -1;
        }

        private void ApplyFilters()
        {
            var text = (TBSearch.Text ?? "").Trim().ToLower();
            var author = (TBAuthor.Text ?? "").Trim().ToLower();
            var genre = CBGenre.SelectedItem as string;

            var query = from b in App.DB.Books
                        join a in App.DB.Authors on b.IdAuthors equals a.IdAuthors into gj
                        from a in gj.DefaultIfEmpty()
                        select new BookViewModel
                        {
                            IdBooks = b.IdBooks,
                            Name = b.Name,
                            Genre = b.Genre,
                            FIO = a != null ? a.FIO : string.Empty
                        };

            if (!string.IsNullOrEmpty(text))
                query = query.Where(x => (x.Name ?? "").ToLower().Contains(text));

            if (!string.IsNullOrEmpty(author))
                query = query.Where(x => (x.FIO ?? "").ToLower().Contains(author));

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(x => x.Genre == genre);

            LVBooks.ItemsSource = query.ToList();
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TBAuthor_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CBGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddBookPage());
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (LVBooks.SelectedItem is Books selected)
            {
                NavigationService.Navigate(new EditPage());
            }
            else
            {
                MessageBox.Show("Выберите книгу для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LVBooks.SelectedItem is Books selected)
            {
                var result = MessageBox.Show($"Удалить книгу \"{selected.Name}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var book = App.DB.Books.FirstOrDefault(b => b.IdBooks == selected.IdBooks);
                    if (book != null)
                    {
                        App.DB.Books.Remove(book);
                        App.DB.SaveChanges();
                        LoadDataFromDb();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            TBSearch.Text = "";
            TBAuthor.Text = "";
            CBGenre.SelectedItem = null;
            LoadDataFromDb();
        }

        private class BookViewModel
        {
            public int IdBooks { get; set; }
            public string Name { get; set; }
            public string Genre { get; set; }
            public string FIO { get; set; }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
