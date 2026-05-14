using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.Graphics;
using Eplan.EplApi.Base;
using LenarSoft.Models;  // ← добавляем
using System;
using System.Collections.Generic;
using System.Linq;

namespace LenarSoft.Generators
{
    internal class TerminalGenerator
    {
        private readonly Project _project;
        private readonly MacroGenerator _macroGen;

        public TerminalGenerator(Project project)
        {
            _project = project;
            _macroGen = new MacroGenerator(project);
        }

        /// <summary>
        /// Создать клеммник на странице
        /// </summary>
        public void GenerateTerminalBlock(Page page, TerminalBlock block, string cabinetTitle)
        {
            // Сортируем клеммы по номеру
            var terminals = block.Terminals.OrderBy(t => t.Number).ToList();

            // Создаем словарь данных для макроса
            var blockData = new Dictionary<string, string>();

            for (int i = 0; i < terminals.Count; i++)
            {
                var term = terminals[i];
                var props = term.GetMacroProperties();

                foreach (var kvp in props)
                {
                    blockData[$"Term{i + 1}_{kvp.Key}"] = kvp.Value;
                }
            }

            // Добавляем общую информацию
            blockData["BlockName"] = block.Name;
            blockData["BlockType"] = block.Type;
            blockData["Cabinet"] = cabinetTitle;

            // Вставляем макрос клеммника
            string macroPath = GetTerminalBlockMacroPath(block.Type, terminals.Count);

            _macroGen.InsertMacroWithPlaceholders(
                page,
                macroPath,
                blockData,
                new PointD(50, 50, 0) // Начальная позиция
            );
        }

        /// <summary>
        /// Создать отдельную клемму (если нужно)
        /// </summary>
        public Function CreateTerminal(Page page, Terminal terminal, string terminalBlockName, PointD position)
        {
            try
            {
                // Ищем артикул клеммы в базе EPLAN
                var article = FindTerminalArticle(terminal.Potential);

                if (article == null)
                {
                    throw new Exception($"Не найден артикул для клеммы с потенциалом {terminal.Potential}");
                }

                // Создаем клемму
                var function = page.InsertTerminal(
                    article,
                    position,
                    $"+{terminalBlockName}:{terminal.Number}"
                );

                // Устанавливаем свойства
                function.Properties.FUNC_DESCRIPTION = terminal.Destination;
                function.Properties.FUNC_POTENTIAL_NAME = terminal.Potential;

                if (!string.IsNullOrEmpty(terminal.Remarks))
                {
                    function.Properties.FUNC_SUPPLEMENTARYFIELD = terminal.Remarks;
                }

                return function;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания клеммы {terminalBlockName}:{terminal.Number}: {ex.Message}");
            }
        }

        private Article FindTerminalArticle(string potential)
        {
            // Здесь нужно настроить поиск артикулов в вашей базе EPLAN
            // Это пример - замените на реальные артикулы из вашей БД

            var articleManager = new ArticleManager();

            string searchPattern = potential switch
            {
                "+24V" => "UK 3 N *24V*",
                "0V" => "UK 3 N *0V*",
                _ => "UK 3 N"
            };

            return articleManager.GetArticles(searchPattern).FirstOrDefault();
        }

        private string GetTerminalBlockMacroPath(string type, int terminalCount)
        {
            // Определяем путь к макросу в зависимости от типа клеммника
            return terminalCount switch
            {
                <= 10 => @"$(MD_MACROS)\TerminalBlocks\TB_10.ema",
                <= 20 => @"$(MD_MACROS)\TerminalBlocks\TB_20.ema",
                <= 30 => @"$(MD_MACROS)\TerminalBlocks\TB_30.ema",
                _ => @"$(MD_MACROS)\TerminalBlocks\TB_40.ema"
            };
        }

        // old
        /*public void InsertTerminal() // Вставить клемму
        {
            Project project = new ProjectManager().CurrentProject;
            if (project == null) { MessageBox.Show("Нет проекта!"); return; }

            SelectionSet selection = new SelectionSet();
            Page currentPage = selection.GetSelectedPages().FirstOrDefault();
            if (currentPage == null) { MessageBox.Show("Выделите страницу!"); return; }

            using (UndoStep undo = new UndoManager().CreateUndoStep())
            {
                try
                {
                    // 1. ПОЛУЧАЕМ СИМВОЛ
                    SymbolLibrary oLib = new SymbolLibrary(project, "GA_symbol_mikerov");
                    Symbol oSymbol = new Symbol(oLib, "klemma_2level_knife");
                    SymbolVariant symbolVariant = new SymbolVariant(oSymbol, 1);

                    // 2. ДОКУМЕНТИРОВАННЫЙ МЕТОД: СОЗДАЁМ SymbolReference
                    SymbolReference symbolRef = new SymbolReference();
                    symbolRef.Create(currentPage, symbolVariant);

                    // 3. ЗАДАЁМ ПОЗИЦИЮ ОТДЕЛЬНО
                    symbolRef.Location = new PointD(100, 200);

                    // 4.НАХОДИМ ЛОГИЧЕСКУЮ КЛЕММУ(TERMINAL) ПО КООРДИНАТАМ
                    // В 2025 году ищем через коллекцию функций страницы
                    Terminal terminal = currentPage.Functions.OfType<Terminal>().FirstOrDefault(t =>
                        t.IsPlaced &&
                        t.Location.X == symbolRef.Location.X &&
                        t.Location.Y == symbolRef.Location.Y);

                    if (terminal == null)
                    {
                        // Если по координатам не нашли, берем последнюю созданную клемму на странице
                        terminal = currentPage.Functions.OfType<Terminal>().LastOrDefault();
                    }
                    MessageBox.Show("11");
                    if (terminal != null)
                    {
                        // 5. ЗАДАЁМ ОБОЗНАЧЕНИЕ
                        terminal.LockObject(); // Снимаем защиту на время записи
                        MessageBox.Show("333");

                        // 1. БЕРЕМ СТРУКТУРУ ИЗ ТЕКУЩЕЙ СТРАНИЦЫ (Чтобы не вбивать вручную)
                        string currentPlant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                        string currentLocation = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                        int num = 1;
                        string str = "-X1:";


                        terminal.Name = currentPlant + currentLocation + str + num.ToString();
                        MessageBox.Show(terminal.Name);

                        terminal.VisibleName = "=" + currentPlant + "+" + currentLocation + str + num.ToString();
                        MessageBox.Show(terminal.VisibleName);

                        // Номер клеммы
                        MessageBox.Show("22");

                        // 7. АРТИКУЛ И ПРОЧЕЕ
                        terminal.AddArticleReference("3044102");
                        MessageBox.Show("36");
                        terminal.IsMainFunction = true;
                        MessageBox.Show("37");

                    }
                    else
                    {
                        MessageBox.Show("Ошибка: Не удалось найти логическую функцию клеммы после вставки символа.");
                    }
                    MessageBox.Show("44");


                    undo.SetUndoDescription("Вставка клеммы -X1:10");
                    MessageBox.Show("Вставка клеммы -X1:10");

                }
                catch (InvalidCastException)
                {
                    MessageBox.Show("Ошибка: Символ 'klemma_2level_knife' не является клеммой! Проверьте тип символа.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        */
    }
}
