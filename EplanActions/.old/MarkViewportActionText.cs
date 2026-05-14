using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.E3D;
using Eplan.EplApi.DataModel.Graphics;
using Eplan.EplApi.EServices.Ged;
using Eplan.EplApi.HEServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

// Класс Action (Точка входа)
namespace LenarSoft.EplanActions
{
    public class MarkViewportAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Priority)
        {
            Name = "MarkViewportAction"; // Имя экшена
            Priority = 0;                 // Приоритет (0 — стандарт)
            return true;
        }

        public bool Execute(ActionCallingContext oActionCallingContext)
        {
            try
            {
                SelectionSet selection = new SelectionSet();
                StorableObject[] selectedObjects = selection.SelectionRecursive;

                if (selectedObjects.Length == 0)
                {
                    MessageBox.Show(
                        "В обзоре модели ничего не выделено.\n\n" +
                        "1. Перейдите в обзор модели (Layout Space)\n" +
                        "2. Зажмите Shift и выделите дин-рейки\n" +
                        "3. Запустите эту команду снова",
                        "Нет выделения",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true;
                }

                // Находим все выделенные рейки
                List<object> selectedRails = new List<object>();
                List<string> allDeviceNames = new List<string>();

                foreach (var obj in selectedObjects)
                {
                    if (obj is MountingRail rail)
                    {
                        selectedRails.Add(rail);

                        // Собираем устройства на рейке
                        foreach (StorableObject child in rail.Children)
                        {
                            if (child is Function3D f3d && !string.IsNullOrWhiteSpace(f3d.VisibleName))
                            {
                                allDeviceNames.Add(f3d.VisibleName.Trim());
                            }
                        }
                    }
                    else if (obj is ViewPart viewPart)
                    {
                        selectedRails.Add(viewPart);

                        // Для ViewPart пытаемся получить исходный объект через Source
                        try
                        {
                            StorableObject source = viewPart.Source;
                            if (source is MountingRail rail2)
                            {
                                foreach (StorableObject child in rail2.Children)
                                {
                                    if (child is Function3D f3d && !string.IsNullOrWhiteSpace(f3d.VisibleName))
                                    {
                                        allDeviceNames.Add(f3d.VisibleName.Trim());
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }

                if (selectedRails.Count == 0)
                {
                    MessageBox.Show(
                        "Среди выделенных объектов нет дин-реек.\n\n" +
                        "Убедитесь, что вы выделили именно дин-рейки в обзоре модели.",
                        "Нет реек",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true;
                }

                // Убираем дубликаты и сортируем
                allDeviceNames = allDeviceNames.Distinct().OrderBy(n => n).ToList();

                if (allDeviceNames.Count == 0)
                {
                    MessageBox.Show("На выделенных рейках не найдено устройств.",
                        "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                // Формируем отчет
                string report = $"📊 Выделено дин-реек: {selectedRails.Count}\n";
                report += $"📦 Найдено устройств: {allDeviceNames.Count}\n\n";
                report += "Устройства:\n";
                report += string.Join("\n", allDeviceNames);

                MessageBox.Show(report, "Устройства на дин-рейках",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Спрашиваем, хочет ли пользователь скопировать список
                if (MessageBox.Show("Скопировать список устройств в буфер обмена?",
                    "Копировать", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Clipboard.SetText(string.Join("\r\n", allDeviceNames));
                    MessageBox.Show("Список скопирован в буфер обмена!", "Готово",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\nСтек: {ex.StackTrace}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties props) { }
    }

}
