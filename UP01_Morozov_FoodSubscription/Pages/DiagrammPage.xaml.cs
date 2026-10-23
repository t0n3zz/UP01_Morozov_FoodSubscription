using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using UP01_Morozov_FoodSubscription.Data;
using UP01_Morozov_FoodSubscription.Helpers;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace UP01_Morozov_FoodSubscription.Pages
{
    /// <summary>
    /// Диаграмма расходов пользователя по категориям продуктовых наборов
    /// и выгрузка отчётов в Microsoft Word (.docx, .pdf) и Microsoft Excel (.xlsx).
    /// </summary>
    public partial class DiagrammPage : Page
    {
        private readonly FoodSubscriptionEntities _context = AppData.GetContext();

        public DiagrammPage()
        {
            InitializeComponent();

            InitChart();

            CmbUser.ItemsSource = _context.User.OrderBy(u => u.FIO).ToList();
            CmbDiagram.ItemsSource = new List<SeriesChartType>
            {
                SeriesChartType.Column,
                SeriesChartType.Bar,
                SeriesChartType.Pie,
                SeriesChartType.Line,
                SeriesChartType.Doughnut
            };

            CmbUser.SelectedIndex = 0;
            CmbDiagram.SelectedIndex = 0;
        }

        /// <summary>Настраивает область построения и ряд данных диаграммы.</summary>
        private void InitChart()
        {
            ChartSubscriptions.BackColor = System.Drawing.Color.White;
            ChartSubscriptions.ChartAreas.Add(new ChartArea("MainArea"));
            ChartSubscriptions.Series.Add(new Series("SubscriptionsSeries")
            {
                ChartArea = "MainArea",
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true
            });
            ChartSubscriptions.Legends.Add(new Legend("MainLegend") { Enabled = false });
        }

        /// <summary>Отображает суммы подписок выбранного пользователя по категориям.</summary>
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (!(CmbUser.SelectedItem is User currentUser) ||
                !(CmbDiagram.SelectedItem is SeriesChartType currentType))
                return;

            Series currentSeries = ChartSubscriptions.Series.FirstOrDefault();
            if (currentSeries == null)
                return;

            currentSeries.ChartType = currentType;
            currentSeries.Points.Clear();

            List<Category> categories = _context.Category.OrderBy(c => c.Name).ToList();
            List<Subscription> subscriptions = _context.Subscription.ToList();

            foreach (Category category in categories)
            {
                decimal sum = subscriptions
                    .Where(s => s.UserID == currentUser.ID && s.CategoryID == category.ID)
                    .Sum(s => s.Price * s.Num);

                currentSeries.Points.AddXY(category.Name, sum);
            }

            ChartSubscriptions.Titles.Clear();
            ChartSubscriptions.Titles.Add("Расходы пользователя " + currentUser.FIO + " по категориям, руб.");
        }

        /// <summary>Выгрузка отчёта по всем пользователям в Microsoft Word и PDF.</summary>
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            List<User> allUsers = _context.User.OrderBy(u => u.FIO).ToList();
            List<Category> allCategories = _context.Category.OrderBy(c => c.Name).ToList();

            if (!allUsers.Any())
            {
                MessageHelper.Warning("В базе данных нет пользователей для формирования отчёта!");
                return;
            }

            Word.Application application = null;
            try
            {
                application = new Word.Application();
                application.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;

                Word.Document document = application.Documents.Add();

                foreach (User user in allUsers)
                {
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.Font.Name = "Times New Roman";
                    userRange.Font.Size = 16;
                    userRange.Font.Bold = 1;
                    userRange.InsertParagraphAfter();

                    document.Paragraphs.Add();

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table table = document.Tables.Add(tableRange, allCategories.Count + 1, 2);

                    table.Borders.InsideLineStyle = table.Borders.OutsideLineStyle =
                        Word.WdLineStyle.wdLineStyleSingle;
                    table.Range.Cells.VerticalAlignment =
                        Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Word.Range cellRange = table.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = table.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";
                    table.Rows[1].Range.Font.Name = "Times New Roman";
                    table.Rows[1].Range.Font.Size = 14;
                    table.Rows[1].Range.Bold = 1;
                    table.Rows[1].Range.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    List<Subscription> userSubscriptions = user.Subscription.ToList();

                    for (int i = 0; i < allCategories.Count; i++)
                    {
                        Category currentCategory = allCategories[i];

                        cellRange = table.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = table.Cell(i + 2, 2).Range;
                        cellRange.Text = userSubscriptions
                            .Where(s => s.CategoryID == currentCategory.ID)
                            .Sum(s => s.Num * s.Price)
                            .ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    document.Paragraphs.Add();

                    Subscription maxSubscription = userSubscriptions
                        .OrderByDescending(s => s.Price * s.Num).FirstOrDefault();
                    if (maxSubscription != null)
                    {
                        Word.Paragraph maxParagraph = document.Paragraphs.Add();
                        Word.Range maxRange = maxParagraph.Range;
                        maxRange.Text = string.Format(
                            "Самая дорогая подписка — {0} за {1} руб. от {2}",
                            maxSubscription.Name,
                            (maxSubscription.Price * maxSubscription.Num).ToString("N2"),
                            maxSubscription.Date.ToString("dd.MM.yyyy"));
                        maxRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxRange.Font.Size = 12;
                        maxRange.InsertParagraphAfter();
                    }

                    Subscription minSubscription = userSubscriptions
                        .OrderBy(s => s.Price * s.Num).FirstOrDefault();
                    if (minSubscription != null)
                    {
                        Word.Paragraph minParagraph = document.Paragraphs.Add();
                        Word.Range minRange = minParagraph.Range;
                        minRange.Text = string.Format(
                            "Самая дешёвая подписка — {0} за {1} руб. от {2}",
                            minSubscription.Name,
                            (minSubscription.Price * minSubscription.Num).ToString("N2"),
                            minSubscription.Date.ToString("dd.MM.yyyy"));
                        minRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minRange.Font.Size = 12;
                        minRange.InsertParagraphAfter();
                    }

                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = "Отчёт сформирован " + DateTime.Now.ToString("dd.MM.yyyy");
                    headerRange.Font.Size = 10;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                string docxPath = BuildReportPath("Отчет_по_подпискам.docx");
                string pdfPath = BuildReportPath("Отчет_по_подпискам.pdf");

                document.SaveAs2(docxPath);
                document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF);

                application.DisplayAlerts = Word.WdAlertLevel.wdAlertsAll;
                application.Visible = true;

                MessageHelper.Info("Отчёт сохранён:\n" + docxPath + "\n" + pdfPath);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                QuitSafely(application);
                MessageHelper.Error(
                    "Не удалось запустить Microsoft Word.\n\n" +
                    "Для выгрузки отчётов на компьютере должен быть установлен " +
                    "настольный Microsoft Office (Word и Excel).");
            }
            catch (Exception ex)
            {
                QuitSafely(application);
                MessageHelper.Error("Не удалось сформировать отчёт Microsoft Word.\n" + ex.Message);
            }
        }

        /// <summary>Выгрузка отчёта по всем пользователям в Microsoft Excel.</summary>
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            List<User> allUsers = _context.User.OrderBy(u => u.FIO).ToList();
            List<Category> allCategories = _context.Category.OrderBy(c => c.Name).ToList();

            if (!allUsers.Any())
            {
                MessageHelper.Warning("В базе данных нет пользователей для формирования отчёта!");
                return;
            }

            Excel.Application application = null;
            try
            {
                application = new Excel.Application();
                application.DisplayAlerts = false;

                Excel.Workbook workbook = application.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.Sheets[1];
                worksheet.Name = "Подписки";

                int row = 1;

                foreach (User user in allUsers)
                {
                    worksheet.Cells[row, 1] = user.FIO;
                    worksheet.Cells[row, 1].Font.Bold = true;
                    worksheet.Cells[row, 1].Font.Size = 14;
                    row++;

                    worksheet.Cells[row, 1] = "Категория";
                    worksheet.Cells[row, 2] = "Сумма расходов, руб.";
                    worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 2]].Font.Bold = true;
                    worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 2]].Interior.Color =
                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(234, 235, 237));
                    row++;

                    int firstDataRow = row;
                    List<Subscription> userSubscriptions = user.Subscription.ToList();

                    foreach (Category category in allCategories)
                    {
                        decimal sum = userSubscriptions
                            .Where(s => s.CategoryID == category.ID)
                            .Sum(s => s.Num * s.Price);

                        worksheet.Cells[row, 1] = category.Name;
                        worksheet.Cells[row, 2] = sum;
                        worksheet.Cells[row, 2].NumberFormat = "#,##0.00";
                        row++;
                    }

                    worksheet.Cells[row, 1] = "Итого";
                    worksheet.Cells[row, 1].Font.Bold = true;
                    worksheet.Cells[row, 2].Formula =
                        string.Format("=SUM(B{0}:B{1})", firstDataRow, row - 1);
                    worksheet.Cells[row, 2].Font.Bold = true;
                    worksheet.Cells[row, 2].NumberFormat = "#,##0.00";

                    Excel.Range tableRange = worksheet.Range[
                        worksheet.Cells[firstDataRow - 1, 1], worksheet.Cells[row, 2]];
                    tableRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    row += 2;
                }

                worksheet.Columns.AutoFit();

                string xlsxPath = BuildReportPath("Отчет_по_подпискам.xlsx");
                workbook.SaveAs(xlsxPath, Excel.XlFileFormat.xlOpenXMLWorkbook);

                application.DisplayAlerts = true;
                application.Visible = true;

                MessageHelper.Info("Отчёт сохранён:\n" + xlsxPath);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                QuitSafely(application);
                MessageHelper.Error(
                    "Не удалось запустить Microsoft Excel.\n\n" +
                    "Для выгрузки отчётов на компьютере должен быть установлен " +
                    "настольный Microsoft Office (Word и Excel).");
            }
            catch (Exception ex)
            {
                QuitSafely(application);
                MessageHelper.Error("Не удалось сформировать отчёт Microsoft Excel.\n" + ex.Message);
            }
        }

        /// <summary>Закрывает приложение Microsoft Office, если оно было запущено.</summary>
        private static void QuitSafely(object application)
        {
            if (application == null)
                return;

            try
            {
                Word.Application word = application as Word.Application;
                if (word != null)
                {
                    word.Quit();
                    return;
                }

                Excel.Application excel = application as Excel.Application;
                if (excel != null)
                    excel.Quit();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
            }
        }

        /// <summary>Формирует путь к файлу отчёта в папке «Документы».</summary>
        private static string BuildReportPath(string fileName)
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Отчеты_UP01_Morozov");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, fileName);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AdminPage());
        }
    }
}
