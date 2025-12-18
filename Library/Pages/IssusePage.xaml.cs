using Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class IssusePage : Page
    {
        private readonly int? _preselectBookId;

        public IssusePage(int? bookId = null)
        {
            InitializeComponent();
            _preselectBookId = bookId;
            Loaded += (_, __) => LoadData();
        }

        private void LoadData()
        {
            try
            {
                CBBooks.ItemsSource = App.DB.Books
                    .Where(b => b.Status == "Available")
                    .OrderBy(b => b.Name)
                    .ToList();

                CBReaders.ItemsSource = App.DB.Readers
                    .OrderBy(r => r.FIO)
                    .ToList();

                if (_preselectBookId.HasValue)
                {
                    int id = _preselectBookId.Value;
                    var pre = App.DB.Books.FirstOrDefault(b => b.IdBooks == id);
                    if (pre != null) CBBooks.SelectedItem = pre;
                }

                DPReturnDate.SelectedDate = DateTime.Now.Date.AddDays(14);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnIssue_Click(object sender, RoutedEventArgs e)
        {
            if (CBBooks.SelectedItem == null) { MessageBox.Show("Выберите книгу"); return; }
            if (CBReaders.SelectedItem == null) { MessageBox.Show("Выберите читателя"); return; }
            if (!DPReturnDate.SelectedDate.HasValue) { MessageBox.Show("Выберите дату возврата"); return; }

            try
            {
                var book = (dynamic)CBBooks.SelectedItem;
                var reader = (dynamic)CBReaders.SelectedItem;

                var bookEntity = App.DB.Books.Find((int)book.IdBooks);
                if (bookEntity == null) { MessageBox.Show("Книга не найдена"); return; }
                if (!string.Equals(bookEntity.Status, "Available", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Книга недоступна для выдачи"); return;
                }

                var issue = new BookIssues
                {
                    IdBooks = bookEntity.IdBooks,
                    IdReaders = (int)reader.IdReaders,
                    IssuesDate = DateTime.Now,
                    PlannedReturnDate = DPReturnDate.SelectedDate.Value,
                    ActualReturnDate = null
                };

                App.DB.BookIssues.Add(issue);
                bookEntity.Status = "Issued";
                App.DB.SaveChanges();

                MessageBox.Show("Книга выдана", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                if (NavigationService?.CanGoBack == true) NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выдаче: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
