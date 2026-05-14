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
                        "Выделите дин-рейку в обзоре модели.",
                        "Нет выделения",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true;
                }

                // Получаем текущий проект
                Project project = selection.GetCurrentProject(false);
                if (project == null)
                {
                    MessageBox.Show("Не удалось получить текущий проект.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }

                // Получаем выбранную страницу
                Page selectedPage = selection.GetSelectedPages().FirstOrDefault();
                if (selectedPage == null)
                {
                    MessageBox.Show(
                        "Выберите страницу с видовым экраном.",
                        "Нет страницы",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true;
                }

                // Создаем словарь ViewPart → ViewPlacement для выбранной страницы
                Dictionary<ViewPart, ViewPlacement> viewPartMap = CreateViewPartMap(project, selectedPage);

                // Находим первую выделенную рейку
                object selectedRail = null;
                List<string> deviceNames = new List<string>();
                ViewPart railViewPart = null;

                foreach (var obj in selectedObjects)
                {
                    if (obj is MountingRail rail)
                    {
                        selectedRail = rail;

                        // Собираем имена устройств на рейке
                        foreach (StorableObject child in rail.Children)
                        {
                            if (child is Function3D f3d && !string.IsNullOrWhiteSpace(f3d.VisibleName))
                            {
                                deviceNames.Add(f3d.VisibleName.Trim());
                            }
                        }

                        // Ищем ViewPart для этой рейки на странице
                        railViewPart = FindViewPartForRail(rail, viewPartMap.Keys);
                        break;
                    }
                    else if (obj is ViewPart viewPart)
                    {
                        selectedRail = viewPart;
                        railViewPart = viewPart;

                        try
                        {
                            StorableObject source = viewPart.Source;
                            if (source is MountingRail rail2)
                            {
                                foreach (StorableObject child in rail2.Children)
                                {
                                    if (child is Function3D f3d && !string.IsNullOrWhiteSpace(f3d.VisibleName))
                                    {
                                        deviceNames.Add(f3d.VisibleName.Trim());
                                    }
                                }
                            }
                        }
                        catch { }
                        break;
                    }
                }

                if (selectedRail == null)
                {
                    MessageBox.Show(
                        "Выделенный объект не является дин-рейкой.",
                        "Неверный выбор",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true;
                }

                if (deviceNames.Count == 0)
                {
                    MessageBox.Show(
                        "На выделенной рейке нет устройств.",
                        "Результат",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return true;
                }

                // Находим видовой экран, на котором находится рейка
                ViewPlacement targetViewport = null;
                if (railViewPart != null && viewPartMap.ContainsKey(railViewPart))
                {
                    targetViewport = viewPartMap[railViewPart];
                }

                if (targetViewport == null)
                {
                    MessageBox.Show(
                        "Рейка не найдена на текущей странице.\n\n" +
                        "Убедитесь, что на выбранной странице есть видовой экран с этой рейкой.",
                        "Не найдено",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return true;
                }

                // Убираем дубликаты и сортируем
                deviceNames = deviceNames.Distinct().OrderBy(n => n).ToList();

                // Формируем вертикальный список
                string calloutText = string.Join("\n", deviceNames);

                RailCalloutInteraction.CalloutText = calloutText;

                // Получаем масштаб видового экрана
                double scale = 1.0;
                try
                {
                    var scaleProp = targetViewport.Properties[36509]; // VIEW_SCALE
                    if (scaleProp != null && !scaleProp.IsEmpty)
                    {
                        // Явно преобразуем в строку, затем в double
                        string scaleStr = scaleProp.ToString();
                        if (!string.IsNullOrEmpty(scaleStr))
                        {
                            double.TryParse(scaleStr, NumberStyles.Any, CultureInfo.InvariantCulture, out scale);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка получения масштаба: {ex.Message}");
                }

                // Активируем страницу перед запуском интерактива
                CommandLineInterpreter cmdLine = new CommandLineInterpreter(); // Объявляем здесь!

                // Активируем страницу перед запуском интерактива
                SelectionSet selection2 = new SelectionSet();
                Page currentPage = selection2.GetSelectedPages().FirstOrDefault();


                // Настраиваем статические поля для Interaction (как запасной вариант)
                RailCalloutInteraction.SelectedViewPart = railViewPart;

                // СОЗДАЕМ КОНТЕКСТ для запуска Interaction
                ActionCallingContext actionCtx = new ActionCallingContext();

                // Устанавливаем имя Interaction через AddParameter
                actionCtx.AddParameter("Name", "RailCalloutInteraction");

                // Создаем InteractionContext и наполняем его параметрами через AddParameter
                InteractionContext interactionCtx = new InteractionContext();
                interactionCtx.AddParameter("Scale", scale.ToString(CultureInfo.InvariantCulture));
                interactionCtx.AddParameter("FontSize", "25");
                interactionCtx.AddParameter("Layer", "583");
                interactionCtx.AddParameter("Text", calloutText);

                // Сохраняем InteractionContext в строку и передаем как параметр
                string contextString = interactionCtx.ToString();
                actionCtx.AddParameter("Context", contextString);


                // Запускаем Interaction
                cmdLine.Execute("XGedStartInteractionAction", actionCtx);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Создает словарь ViewPart → ViewPlacement для указанной страницы
        /// </summary>
        private Dictionary<ViewPart, ViewPlacement> CreateViewPartMap(Project project, Page page)
        {
            var map = new Dictionary<ViewPart, ViewPlacement>();

            try
            {
                DMObjectsFinder finder = new DMObjectsFinder(project);
                PlacementsFilter filter = new PlacementsFilter { Page = page };
                Placement[] placements = finder.GetPlacements(filter);

                map = placements
                    .OfType<ViewPlacement>()
                    .SelectMany(vp => vp.SubPlacements
                        .OfType<ViewPart>()
                        .Select(vpPart => new { vpPart, vp }))
                    .ToDictionary(x => x.vpPart, x => x.vp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка CreateViewPartMap: {ex.Message}");
            }

            return map;
        }

        /// <summary>
        /// Ищет ViewPart для указанной рейки
        /// </summary>
        private ViewPart FindViewPartForRail(MountingRail rail, IEnumerable<ViewPart> viewParts)
        {
            foreach (var vp in viewParts)
            {
                try
                {
                    StorableObject source = vp.Source;
                    if (source == rail)
                        return vp;
                }
                catch { }
            }
            return null;
        }


        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties props) { }
    }

}
