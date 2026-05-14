using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.E3D;
using Eplan.EplApi.DataModel.Graphics;
using Eplan.EplApi.HEServices;
using Eplan.EplApi.Scripting;
using System;
using System.Windows.Forms;

namespace LenarSoft.EplanActions
{
    /// Простой Action для создания одного видового экрана
    /// </summary>
    public class CreateSimpleViewportAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = "CreateSimpleViewport";
            Ordinal = 20;
            return true;
        }

        public bool Execute(ActionCallingContext ctx)
        {
            try
            {
                // 1. Получаем текущий проект
                var selectionSet = new SelectionSet();
                var project = selectionSet.GetCurrentProject(false);

                if (project == null)
                {
                    MessageBox.Show("Откройте проект EPLAN!");
                    return false;
                }

                // 2. Создаем новую страницу для видового экрана
                Page page = CreateViewportPage(project);

                // 3. Создаем простой видовой экран
                ViewPlacement viewport = CreateSimpleViewport(project, page);

                // 4. Обновляем и показываем сообщение
                viewport.Update();

                MessageBox.Show(
                    $"Видовой экран успешно создан!\n" +
                    $"Страница: {page.Name}\n" +
                    $"Имя вида: {GetViewportName(viewport)}",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool GetActionProperties(ref ActionProperties properties)
        {
            return true;
        }

        /// <summary>
        /// Создание страницы для видового экрана
        /// </summary>
        private Page CreateViewportPage(Project project)
        {
            // Имя страницы: =ProjectName+3D/1.1
            string pageName = "=TestProject+3D/1.1";

            // Проверяем, существует ли уже такая страница
            Page[] existingPages = project.Pages.SearchPages(pageName);
            if (existingPages.Length > 0)
            {
                return existingPages[0];
            }

            // Создаем новую страницу
            Page page = project.CreatePage(
                pageName,
                Page.PageType.SchematicMulti,
                true // открыть после создания
            );

            // Устанавливаем описание страницы
            SetPageProperty(page, "PAGE_DESCRIPTION", "3D виды");

            return page;
        }

        /// <summary>
        /// Создание простого видового экрана
        /// </summary>
        private ViewPlacement CreateSimpleViewport(Project project, Page page)
        {
            // Создаем видовой экран
            ViewPlacement viewport = new ViewPlacement();

            // Важно: для Create нужно передать проект
            viewport.Create(project, null); // null = корень проекта

            // Размещаем на странице
            viewport.Page = page;

            // Устанавливаем область отображения (в мм)
            // X, Y - левый верхний угол, Width, Height - размер
            viewport.Area = new RectangleD(
                new PointD(10, 10),    // левый верхний угол
                new PointD(200, 150)    // правый нижний угол
            );

            // Устанавливаем имя вида
            SetViewportProperty(viewport, "VIEW_NAME", "Тестовый вид");

            // Устанавливаем масштаб
            SetViewportProperty(viewport, "VIEW_SCALE", 1.0);

            // Устанавливаем тип отображения (1 = ортогональная проекция)
            SetViewportProperty(viewport, "VIEW_REPRESENTATION_TYPE", 1);

            // Устанавливаем угол обзора (изометрия)
            SetViewportProperty(viewport, "VIEW_ANGLE1", 45.0);
            SetViewportProperty(viewport, "VIEW_ANGLE2", 35.264);

            // Включаем отображение элементов
            SetViewportProperty(viewport, "VIEW_SHOW_CONNECTIONS", 1);
            SetViewportProperty(viewport, "VIEW_SHOW_DEVICE_TAGS", 1);
            SetViewportProperty(viewport, "VIEW_SHOW_MOUNTINGRAILS", 1);
            SetViewportProperty(viewport, "VIEW_SHOW_CABLEDUCTS", 1);

            return viewport;
        }

        /// <summary>
        /// Безопасная установка свойства для страницы
        /// </summary>
        private void SetPageProperty(Page page, string propertyName, object value)
        {
            try
            {
                // Используем индексатор UniversalPropertyList
                page.Properties[propertyName] = value;
            }
            catch
            {
                // Если свойство не найдено, игнорируем
            }
        }

        /// <summary>
        /// Безопасная установка свойства для видового экрана
        /// </summary>
        private void SetViewportProperty(ViewPlacement viewport, string propertyName, object value)
        {
            try
            {
                // Используем индексатор UniversalPropertyList
                viewport.Properties[propertyName] = value;
            }
            catch
            {
                // Если свойство не найдено, игнорируем
            }
        }

        /// <summary>
        /// Безопасное получение имени видового экрана
        /// </summary>
        private string GetViewportName(ViewPlacement viewport)
        {
            try
            {
                return viewport.Properties["VIEW_NAME"]?.ToString() ?? "Без имени";
            }
            catch
            {
                return "Без имени";
            }
        }
    }
}
