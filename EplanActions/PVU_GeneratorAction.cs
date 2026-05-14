using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.HEServices;
using LenarSoft.Generators;
using LenarSoft.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace LenarSoft.EplanActions
{
    public class PVU_GeneratorAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Priority)
        {
            Name = "PVU_GeneratorAction";
            Priority = 20;
            return true;
        }

        public bool Execute(ActionCallingContext ctx)
        {
            // === Условие - Мы находимся на 1-ой странице
            // === 01 Вставить Макрос 01_Ввод_силовой (Работает) 14.05.26

            Project project = new ProjectManager().CurrentProject;
            if (project == null) { MessageBox.Show("Нет проекта!"); return false; }

            SelectionSet selection = new SelectionSet();
            Page currentPage = selection.GetSelectedPages().FirstOrDefault();
            if (currentPage == null) { MessageBox.Show("Выделите страницу!"); return false; }

            // Вставить макрос
            string macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\01_Ввод_силовой.ema";

            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>
            {
                ["INPUT_PVU"] = new Dictionary<string, string>
                {
                    ["PRODUCT_NUMBER_QS1"] = "DEK.000",
                    ["PRODUCT_NUMBER_QS1"] = "DEK.111"
                },
            };

            PointD insertPoint = new PointD(0, 0);

            var insert = new MacroGenerator(project);
            insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary, insertPoint);

            MessageBox.Show("Макрос 01_Ввод_силовой вставлен! на 1-ую страницу");
                        

            // 1. Получаем ВСЕ свойства текущей страницы
            string plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
            string location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
            string functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
            string designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
            string docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
            int nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

            // 2. Передаем свойства текущей страницы новой странице
            PagePropertyList nextPageProps = new PagePropertyList();

            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

            // 3. Создаём страницу
            Page nextPage = new Page();
            nextPage.Create(project, currentPage.PageType, nextPageProps);

            MessageBox.Show($"Страница создана: {functionalAssigment}/{designationUserDefined}/{docType}/{nextNum}");

            // === Переключить фокус на новую страницу
            Edit editService = new Edit();
            editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
            CommandLineInterpreter interpreter = new CommandLineInterpreter();
            interpreter.Execute("XGedSelectPageAction");
            interpreter.Execute("XEsSyncPDDsAction");
            interpreter.Execute("XGedEscapeAction");

            currentPage = nextPage;

            // === Условие - Мы находимся на 2-ой странице
            // === 02 Вставить Макрос 02_Кросс_модуль (Работает) 14.05.26

            // Вставить макрос
            macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\02_Кросс_модуль.ema";

            Dictionary<string, Dictionary<string, string>> dictionary2 = new Dictionary<string, Dictionary<string, string>>
            {
                ["INPUT_PVU"] = new Dictionary<string, string>
                {
                    ["PRODUCT_NUMBER_QS1"] = "DEK.000",
                    ["PRODUCT_NUMBER_QS1"] = "DEK.111"
                },
            };

            insertPoint = new PointD(0, 0);

            insert = new MacroGenerator(project);
            insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary2, insertPoint);

            MessageBox.Show("Макрос 02_Кросс_модуль вставлен! на 2-ую страницу");


            // === 03 Вставка макроса (Тип пуска — ПЧВ/Прямой пуск)

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel (*.xlsx)|*.xlsx";
                dialog.Title = "Выберите опросный лист";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return true;

                // 2. Прочитать Excel
                var excel = new Excel.Application();
                var workbook = excel.Workbooks.Open(dialog.FileName);
                var sheet = workbook.Sheets[1];

                // Читаем ВСЕ нужные ячейки за один раз
                string pchv = sheet.Cells[9, 2].Value?.ToString();   // B9  — Тип пуска
                string nasos = sheet.Cells[13, 2].Value?.ToString();  // B13 — Насос калорифера
                string uvlazhnitel = sheet.Cells[15, 2].Value?.ToString();  // B15 — Увлажнитель
                string kkb = sheet.Cells[17, 2].Value?.ToString();  // B17 — ККБ
                string modbus = sheet.Cells[21, 2].Value?.ToString();  // B21 — Modbus
                string tipPlc = sheet.Cells[23, 2].Value?.ToString();  // B23 — Тип ПЛК
                string tipDatchikov = sheet.Cells[11, 2].Value?.ToString();  // B11 — Тип датчиков
                string moschnostPV = sheet.Cells[7, 2].Value?.ToString();   // B7  — Мощность ПВ
                string modelPCHV = sheet.Cells[8, 2].Value?.ToString();   // B8  — Модель ПЧВ

                workbook.Close(false);
                excel.Quit();

                // Макрос 02 — ПЧВ или Прямой пуск
                if (pchv == "ПЧВ")
                {
                    macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\02_ПЧВ_Двигатель.ema";

                    Dictionary<string, Dictionary<string, string>> dictionary3 = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["CONTROL_CABINET"] = new Dictionary<string, string>
                        {
                            ["FUNCTION"] = "M0-85-050",
                            ["PLACE"] = "JD01-CM1001"
                        },
                    };

                    insertPoint = new PointD(0, 0);

                    insert = new MacroGenerator(project);
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary3, insertPoint);
                }
                else // 02_Прямой_пуск.ema
                {
                    macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\02_Прямой_пуск.ema";

                    Dictionary<string, Dictionary<string, string>> dictionary3 = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["CONTROL_CABINET"] = new Dictionary<string, string>
                        {
                            ["FUNCTION"] = "M0-85-050",
                            ["PLACE"] = "JD01-CM1001"
                        },
                    };

                    insertPoint = new PointD(0, 0);

                    insert = new MacroGenerator(project);
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary3, insertPoint);
                }


                // Макрос 03 — Насос калорифера
                if (nasos == "Да")
                {
                    macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\03_Насос_калорифера.ema";

                    Dictionary<string, Dictionary<string, string>> dictionary3 = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["CONTROL_CABINET"] = new Dictionary<string, string>
                        {
                            ["FUNCTION"] = "M0-85-050",
                            ["PLACE"] = "JD01-CM1001"
                        },
                    };

                    insertPoint = new PointD(0, 0);

                    insert = new MacroGenerator(project);
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary3, insertPoint);
                }

            }
            return true;
        }

        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties properties) { }
    }
}