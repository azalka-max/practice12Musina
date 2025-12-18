using System;
using System.Collections.Generic;
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
    public partial class IssuseLogPage : Page
    {
        public IssuseLogPage()
        {
            InitializeComponent();
            Loaded += IssueLogPage_Loaded;
        }
        private void IssueLogPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var issues = App.DB.BookIssues
                    .OrderByDescending(i => i.IssuesDate)
                    .ToList();

                var list = new List<IssueListItem>();

                foreach (var i in issues)
                {
                    string bookName = "";
                    string readerFio = "";

                    if (i.IdBooks.HasValue)
                    {
                        var b = App.DB.Books.Find(i.IdBooks.Value);
                        if (b != null) bookName = b.Name ?? "";
                    }

                    if (i.IdReaders.HasValue)
                    {
                        var r = App.DB.Readers.Find(i.IdReaders.Value);
                        if (r != null) readerFio = r.FIO ?? "";
                    }

                    list.Add(new IssueListItem
                    {
                        Id = i.Id,
                        Book = new BookLite { Name = bookName },
                        Reader = new ReaderLite { FullName = readerFio },
                        IssueDate = FormatDate(i.IssuesDate),
                        DueDate = FormatDate(i.PlannedReturnDate),
                        ReturnDate = FormatActualReturn(i.ActualReturnDate)
                    });
                }

                LVIssues.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки журнала выдач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            var selected = LVIssues.SelectedItem as IssueListItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите запись в журнале.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Отметить возврат выбранной записи?", "Подтвердите", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var issue = App.DB.BookIssues.Find(selected.Id);
                if (issue == null)
                {
                    MessageBox.Show("Запись не найдена в базе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                issue.ActualReturnDate = DateTime.Now.ToString("yyyy-MM-dd");

                if (issue.IdBooks.HasValue)
                {
                    var book = App.DB.Books.Find(issue.IdBooks.Value);
                    if (book != null)
                    {
                        book.Status = "Available";
                    }
                }

                App.DB.SaveChanges();

                MessageBox.Show("Возврат отмечен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отметке возврата: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatDate(DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToString("dd.MM.yyyy") : "";
        }


        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private string FormatActualReturn(string actualReturnDate)
        {
            if (string.IsNullOrWhiteSpace(actualReturnDate)) return "";
            if (DateTime.TryParse(actualReturnDate, out var d))
                return d.ToString("dd.MM.yyyy");
            return actualReturnDate;
        }
    }
        public class BookLite
        {
            public string Name { get; set; }
        }

        public class ReaderLite
        {
            public string FullName { get; set; }
        }

        public class IssueListItem
        {
            public int Id { get; set; }
            public BookLite Book { get; set; }
            public ReaderLite Reader { get; set; }
            public string IssueDate { get; set; }
            public string DueDate { get; set; }
            public string ReturnDate { get; set; }
        }
}
