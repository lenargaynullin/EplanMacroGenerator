using Eplan.EplApi.DataModel;
using LenarSoft.Models;  // ← добавляем
using System;

namespace LenarSoft.Readers
{
        /// Генерация страниц в EPLAN
        /// </summary>
        public class PageGenerator
        {
            private readonly Project _project;

            public PageGenerator(Project project)
            {
                _project = project;
            }

            /// <summary>
            /// Создать страницу по имени
            /// </summary>
            public Page CreatePage(string pageName, string description = "")
            {
                try
                {
                    // Проверяем, существует ли уже такая страница
                    var existingPages = _project.Pages.SearchPages(pageName);
                    if (existingPages.Length > 0)
                    {
                        return existingPages[0];
                    }

                    // Создаем новую страницу
                    var page = _project.Pages.CreatePage(
                        pageName,
                        Page.PageType.SchematicMulti,
                        true // открыть после создания
                    );

                    if (!string.IsNullOrEmpty(description))
                    {
                        page.Properties.PAGE_DESCRIPTION = description;
                    }

                    return page;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка создания страницы {pageName}: {ex.Message}");
                }
            }

            /// <summary>
            /// Создать страницу для шкафа
            /// </summary>
            public Page CreateCabinetMainPage(CabinetData cabinet)
            {
                return CreatePage(
                    cabinet.GetMainPageName(),
                    $"Шкаф {cabinet.Title}"
                );
            }

            /// <summary>
            /// Создать страницу для клеммника
            /// </summary>
            public Page CreateTerminalBlockPage(CabinetData cabinet, TerminalBlock block, int pageNumber)
            {
                return CreatePage(
                    cabinet.GetTerminalBlockPageName(block, pageNumber),
                    $"Клеммник {block.Name} ({block.Type})"
                );
            }

            /// <summary>
            /// Создать страницу для сетевых подключений
            /// </summary>
            public Page CreateNetworkPage(string cabinetTitle, string type)
            {
                return CreatePage(
                    $"={cabinetTitle}+{type}/1.1",
                    $"Сетевые подключения {type}"
                );
            }

            /// <summary>
            /// Создать страницу для питания
            /// </summary>
            public Page CreatePowerPage(string cabinetTitle)
            {
                return CreatePage(
                    $"={cabinetTitle}+PWR/1.1",
                    "Цепи питания"
                );
            }
        }

        // Рабочее решение 13.02.26
        /*public void InsertPage() // Вставить новую страницу
        {
            // Получаем текущий открытый проект
            Project project = new ProjectManager().CurrentProject;

            // 1. Создаем объект выбора
            SelectionSet selection = new SelectionSet();

            // 2. Получаем первую выделенную страницу
            Page currentPage = selection.GetSelectedPages().FirstOrDefault();

            // 1. Получаем текущую структуру (Установка и Место)
            string plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT]; // CA1
            string location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION]; // EAA
            int nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1; // 2

            // Ищем последнюю страницу
            while (project.Pages.Any(p => (int)p.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] == nextNum))
            {
                nextNum++;
            }

            // 2. Создаем набор свойств и ЗАПОЛНЯЕМ его вручную
            PagePropertyList nextPageProps = new PagePropertyList();

            // Установка (==)
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant; // CA1

            // Место установки (+)
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location; // EAA

            // Номер страницы (/)
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum; // 2

            // 3. Создаем страницу (3 аргумента: проект, тип, свойства)
            Page nextPage = new Page();
            nextPage.Create(project, currentPage.PageType, nextPageProps);

            MessageBox.Show("Вставили новую страницу");
        }
        */
}
