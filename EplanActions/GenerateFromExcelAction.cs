using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.HEServices;
using LenarSoft.Generators;  // ← добавляем
using LenarSoft.Readers;      // ← добавляем
using LenarSoft.Models;       // ← добавляем
using System;
using System.Windows.Forms;

// Класс Action (Точка входа)
namespace LenarSoft.EplanActions
{
    /// Action для запуска генерации из EPLAN

    public class GenerateFromExcelAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Priority)
        {
            Name = "GenerateFromExcelAction"; // Имя экшена
            Priority = 0;                 // Приоритет (0 — стандарт)
            return true;
        }
        public bool Execute(ActionCallingContext ctx)
        {
            try
            {
                // 1. Проверяем, открыт ли проект
                var selectionSet = new SelectionSet();
                var project = selectionSet.GetCurrentProject(false);
                if (project == null)
                {
                    MessageBox.Show("Сначала откройте проект EPLAN!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 2. Выбираем Excel-файл
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|(*.xls)|*.xls";
                    openFileDialog.Title = "Выберите файл с данными (лист Физика)";

                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                        return true; // пользователь отменил

                    // 3. Читаем данные
                    var reader = new ExcelReaderService();
                    var signals = reader.ReadPhysicsSheet(openFileDialog.FileName);

                    // 4. Показываем результат (для проверки)
                    MessageBox.Show(
                        $"Прочитано сигналов: {signals.Count}\n\n" +
                        $"Первый сигнал: {signals[0]?.SignalName}\n" +
                        $"Клеммник: {signals[0]?.TerminalBlock}",
                        "Успешно",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // TODO: Здесь будет генерация страниц и клемм
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        return true;
        }
        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties properties)
        {
            return true;
        }
    }

}
