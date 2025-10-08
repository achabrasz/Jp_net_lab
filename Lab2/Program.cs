using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml.Drawing.Chart;

class Program
{
    static void Main(string[] args)
    {
        ExcelPackage.License.SetNonCommercialPersonal("license");
        try {
            var dir = new DirectoryInfo(@"/Users/Adam/Studia");
            if (!dir.Exists)
            {
                Console.WriteLine($"Directory not found: {dir}");
                return;
            }

            var file = new FileInfo(@"../../../labEpp.xlsx");
            if (file.Exists)
            {
                try { file.Delete(); }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"No access to delete file: {file}");
                    return;
                }
                catch (IOException)
                {
                    Console.WriteLine($"File in use or IO error: {file}");
                    return;
                }
            }

            using (ExcelPackage ep = new ExcelPackage(file))
            {
                var ws = ep.Workbook.Worksheets.Add("Struktura Katalogu");
                int counter = 1;
                var files = new List<FileInfo>();
                ScanDirGroupedBySubdir(dir, 2, 0, ws, ref counter, ref files);

                var ws2 = ep.Workbook.Worksheets.Add("Statystyki");
                var topFiles = files
                    .OrderByDescending(f => f.Length)
                    .Take(10)
                    .ToList();

                ws2.Cells[1, 1].Value = "Name";
                ws2.Cells[1, 2].Value = "Extension";
                ws2.Cells[1, 3].Value = "Size";
                ws2.Cells[1, 4].Value = "Directory";

                for (int i = 0; i < topFiles.Count; i++)
                {
                    ws2.Cells[i + 2, 1].Value = topFiles[i].Name;
                    ws2.Cells[i + 2, 2].Value = topFiles[i].Extension;
                    ws2.Cells[i + 2, 3].Value = topFiles[i].Length;
                    ws2.Cells[i + 2, 4].Value = topFiles[i].DirectoryName;
                }

                var extStats = topFiles
                    .GroupBy(f => f.Extension)
                    .Select(g => new
                    {
                        Extension = g.Key,
                        Count = g.Count(),
                        Size = g.Sum(f => f.Length)
                    })
                    .OrderByDescending(e => e.Count)
                    .ToList();

                int chartDataRow = topFiles.Count + 4;
                for (int i = 0; i < extStats.Count; i++)
                {
                    ws2.Cells[chartDataRow + i, 1].Value = extStats[i].Extension;
                    ws2.Cells[chartDataRow + i, 2].Value = extStats[i].Count;
                    ws2.Cells[chartDataRow + i, 3].Value = extStats[i].Size;
                }

                var chartCount = ws2.Drawings.AddChart("PieCount", eChartType.Pie) as ExcelPieChart;
                chartCount.Title.Text = "File Count by Extension (Top 10)";
                chartCount.SetPosition(chartDataRow - 1, 0, 5, 0);
                chartCount.SetSize(400, 300);
                chartCount.Series.Add(
                    ExcelRange.GetAddress(chartDataRow, 2, chartDataRow + extStats.Count - 1, 2),
                    ExcelRange.GetAddress(chartDataRow, 1, chartDataRow + extStats.Count - 1, 1)
                );
                chartCount.DataLabel.ShowPercent = true;

                var chartSize = ws2.Drawings.AddChart("PieSize", eChartType.Pie) as ExcelPieChart;
                chartSize.Title.Text = "File Size by Extension (Top 10)";
                chartSize.SetPosition(chartDataRow - 1, 0, 10, 0);
                chartSize.SetSize(400, 300);
                chartSize.Series.Add(
                    ExcelRange.GetAddress(chartDataRow, 3, chartDataRow + extStats.Count - 1, 3),
                    ExcelRange.GetAddress(chartDataRow, 1, chartDataRow + extStats.Count - 1, 1)
                );
                chartSize.DataLabel.ShowPercent = true;

                ep.Save();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void ScanDirGroupedBySubdir(DirectoryInfo dir, int maxDepth, int currentDepth, ExcelWorksheet ws, ref int counter, ref List<FileInfo> files)
    {
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            int startRow = counter;
            ws.Cells[counter, 1].Value = subDir.Name;
            ws.Cells[counter, 2].Value = subDir.Extension;
            ws.Cells[counter, 4].Value = subDir.Attributes;
            ws.Cells[counter, 5].Value = currentDepth;
            counter++;
            var size = 0;
            foreach (FileInfo plik in subDir.GetFiles())
            {
                files.Add(plik);
                ws.Cells[counter, 1].Value = plik.Name;
                ws.Cells[counter, 2].Value = plik.Extension;
                ws.Cells[counter, 3].Value = plik.Length;
                ws.Cells[counter, 4].Value = plik.Attributes;
                ws.Cells[counter, 5].Value = currentDepth + 1;
                size += (int)plik.Length;
                counter++;
            }
            ws.Cells[startRow, 3].Value = size;
            if (counter > startRow)
            {
                for (int i = startRow; i < counter; i++)
                    ws.Row(i).OutlineLevel = currentDepth + 1;
            }
            if (currentDepth + 1 < maxDepth)
                ScanDirGroupedBySubdir(subDir, maxDepth, currentDepth + 1, ws, ref counter, ref files);
        }
    }
}