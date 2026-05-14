using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Test
{
    public class ExcelReader
    {
        [Obsolete]
        public string GetValueFromCell(string filePath, string sheetName, int row, int col)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // Установка контекста лицензии (требование EPPlus 5+)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (!fileInfo.Exists)
                throw new FileNotFoundException("Файл не найден по пути: " + filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                // Получаем лист по имени
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetName];

                if (worksheet == null)
                    return "Лист не найден";

                // Читаем значение ячейки
                var cellValue = worksheet.Cells[row, col].Value;

                return cellValue?.ToString() ?? string.Empty;
            }
        }

        internal string ReadFirstCell(string excelPath)
        {
            throw new NotImplementedException();
        }
    }
}
