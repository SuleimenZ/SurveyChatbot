using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table.PivotTable;
using SurveyChatbot.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveyChatbot.Utility
{
    public class ExcelSerializer
    {
        public string FolderPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ExcelReports");
        private int _tableStartRowId = 3;
        private int _tableStartColumnId = 1;
        private Survey _survey;
        private Report[] _reports;

        public ExcelSerializer()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
        public ExcelSerializer(string folderPath) : this()
        {
            FolderPath = folderPath;
        }

        public async Task<Stream?> Serialize(Survey survey, Report[] reports)
        {
            _survey = survey;
            _reports = reports;
            try
            {

                if (!Directory.Exists(FolderPath)) { Directory.CreateDirectory(FolderPath); }
                string stamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
                string path = Path.Combine(FolderPath, $"{_survey.Name} {stamp}.xlsx");
                var file = new FileInfo(path);
                if (file.Exists) { file.Delete(); }

                using ExcelPackage package = new(file);
                //Data sheet
                var sheet = package.Workbook.Worksheets.Add("Data");
                GenerateLayout(sheet);
                FillData(sheet);

                //Detailed stats sheet
                GenerateExtraStats(package, sheet);

                await package.SaveAsync();
                return File.OpenRead(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void GenerateLayout(ExcelWorksheet sheet)
        {
            sheet.Row(_tableStartRowId).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Row(_tableStartRowId).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Row(_tableStartRowId).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Cells.Style.Font.Size = 20;

            //Name
            sheet.Cells[1,2,1, 1 + _survey.Questions.Count()].Merge = true;
            sheet.Cells[1, 2].Value = $"{_survey.Name} ({_survey.SearchId})";
            sheet.Cells[1, 2].Style.Font.Size = 25;
            sheet.Cells[1, 2].Style.Font.Bold = true;
            //Description
            sheet.Cells[2, 2, 2, 1 + _survey.Questions.Count()].Merge = true;
            sheet.Cells[2, 2].Value = _survey.Description;

            sheet.Cells[_tableStartRowId, _tableStartColumnId].Value = "ID";
            sheet.View.FreezePanes(_tableStartRowId + 1, _tableStartColumnId + 1);
            sheet.Cells[_tableStartRowId, _tableStartColumnId + 1, _tableStartRowId, _tableStartColumnId + _survey.Questions.Count()].AutoFilter = true;

            for (int i = 0; i < _survey.Questions.Count; i++)
            {
                sheet.Column(_tableStartColumnId + 1 + i).Width = 30;
                sheet.Column(_tableStartColumnId + 1 + i).BestFit = true;
                sheet.Cells[_tableStartRowId, _tableStartColumnId + 1 + i].Value = _survey.Questions[i].Text;
            }
        }

        private void FillData(ExcelWorksheet sheet)
        {
            sheet.Row(_tableStartRowId).Style.Font.Bold = true;

            for (int i = 0; i < _reports.Length; i++)
            {
                sheet.Row(_tableStartRowId + 1 + i).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells[_tableStartRowId + 1 + i, _tableStartColumnId].Value = i + 1;
                sheet.Cells[_tableStartRowId + 1 + i, _tableStartColumnId].Style.Font.Bold = true;

                for (int k = 0; k < _reports[i].Survey.Questions.Count(); k++)
                {
                    sheet.Cells[_tableStartRowId + 1 + i, _tableStartColumnId + 1 + k].Value = String.Join(", ", _reports[i].GetSelectedAnswers(k));
                    sheet.Cells[_tableStartRowId + 1 + i, _tableStartColumnId + 1 + k].Style.Font.Italic = true;
                }
            }
        }

        private void GenerateExtraStats(ExcelPackage package, ExcelWorksheet dataSheet)
        {
            for (int i = 0; i < _survey.Questions.Count(); i++)
            {
                var detailsSheet = package.Workbook.Worksheets.Add($"Question {i + 1} extra");
                var dataRange = dataSheet.Cells[_tableStartRowId, _tableStartColumnId + 1 + i, _tableStartRowId + _reports.Length, _tableStartColumnId + 1 + i];
                var pivotTable = detailsSheet.PivotTables.Add(detailsSheet.Cells["A1"], dataRange, "PivotTable");

                var answers = pivotTable.RowFields.Add(pivotTable.Fields[0]);
                answers.Name = "Answers";

                var percent = pivotTable.DataFields.Add(pivotTable.Fields[0]);
                percent.Name = $"%";
                percent.ShowDataAs.SetPercentOfTotal();
                percent.Format = "0.0%;";
                percent.Function = DataFieldFunctions.Count;


                ExcelPieChart chart = detailsSheet.Drawings.AddChart("Pie Chart", eChartType.Pie, pivotTable).As.Chart.PieChart;
                chart.SetPosition(1, 0, 4, 0);
                chart.SetSize(600, 400);
                chart.Title.Text = _survey.Questions[i].Text;
                chart.Title.Font.Size = 20;
                chart.Legend.Font.Size = 16;
                chart.Legend.Font.Bold = true;
                chart.DataLabel.ShowPercent = true;
                chart.DataLabel.Font.Size = 16;
                chart.DataLabel.Position = eLabelPosition.Center;
            }
        }
        private bool answerIsSelected(Report report, int questionId, int answerId)
        {
            return ((report.Answers[questionId] >> answerId) & 1) != 0;
        }
    }
}
