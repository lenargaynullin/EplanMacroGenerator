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
            // === 01 Вставить Макрос 01_Ввод_силовой (Работает) 14.05.26

            Project project = new ProjectManager().CurrentProject;
            if (project == null) { MessageBox.Show("Нет проекта!"); return false; }

            SelectionSet selection = new SelectionSet();
            Page currentPage = selection.GetSelectedPages().FirstOrDefault();
            if (currentPage == null) { MessageBox.Show("Выделите страницу!"); return false; }

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

            
            
            // Создать страницу
            string plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
            string location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
            string functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
            string designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
            string docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
            int nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

            PagePropertyList nextPageProps = new PagePropertyList();

            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
            nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

            Page nextPage = new Page();
            nextPage.Create(project, currentPage.PageType, nextPageProps);

            Edit editService = new Edit();
            editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
            CommandLineInterpreter interpreter = new CommandLineInterpreter();
            interpreter.Execute("XGedSelectPageAction");
            interpreter.Execute("XEsSyncPDDsAction");
            interpreter.Execute("XGedEscapeAction");
            currentPage = nextPage;

            // === 02 Вставить Макрос 02_Кросс_модуль (Работает) 14.05.26

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

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel (*.xlsx)|*.xlsx";
                dialog.Title = "Выберите опросный лист";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return true;

                var excel = new Excel.Application();
                var workbook = excel.Workbooks.Open(dialog.FileName);
                var sheet = workbook.Sheets[1];

                string pchv = sheet.Cells[9, 2].Value?.ToString();
                string moschnostPV = sheet.Cells[8, 2].Value?.ToString();
                string modelPCHV = sheet.Cells[10, 2].Value?.ToString();
                string nasos = sheet.Cells[13, 2].Value?.ToString();
                string uvlazhnitel = sheet.Cells[16, 2].Value?.ToString();
                string kkb = sheet.Cells[19, 2].Value?.ToString();
                string tipDatchikov = sheet.Cells[21, 2].Value?.ToString();
                string modbus = sheet.Cells[24, 2].Value?.ToString();
                string tipPlc = sheet.Cells[26, 2].Value?.ToString();
                

                workbook.Close(false);
                excel.Quit();

                // === 03 Вставить макрос 02_ПЧВ_Двигатель
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
                else // === 04 Вставить макрос 02_Прямой_пуск.ema
                {
                    macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\02_Прямой_пуск.ema";

                    Dictionary<string, Dictionary<string, string>> dictionary4 = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["CONTROL_CABINET"] = new Dictionary<string, string>
                        {
                            ["FUNCTION"] = "M0-85-050",
                            ["PLACE"] = "JD01-CM1001"
                        },
                    };

                    insertPoint = new PointD(0, 0);

                    insert = new MacroGenerator(project);
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary4, insertPoint);
                }


                // === 05 Вставить макрос Макрос 03 — Насос калорифера
                if (nasos == "Да")
                {
                    macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\03_Насос_калорифера.ema";

                    Dictionary<string, Dictionary<string, string>> dictionary5 = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["CONTROL_CABINET"] = new Dictionary<string, string>
                        {
                            ["FUNCTION"] = "M0-85-050",
                            ["PLACE"] = "JD01-CM1001"
                        },
                    };

                    insertPoint = new PointD(0, 0);

                    insert = new MacroGenerator(project);
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary5, insertPoint);
                }

                // === 06 Вставить макрос Макрос 04 — Насос увлажнителя
                if (uvlazhnitel == "Да")
                {
                    macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\04_Насос_увлажнителя.ema";

                    Dictionary<string, Dictionary<string, string>> dictionary6 = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["CONTROL_CABINET"] = new Dictionary<string, string>
                        {
                            ["FUNCTION"] = "M0-85-050",
                            ["PLACE"] = "JD01-CM1001"
                        },
                    };

                    insertPoint = new PointD(0, 0);

                    insert = new MacroGenerator(project);
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary6, insertPoint);
                }

                // === 07 Вставить макрос Макрос 05 — Лампа сеть
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\05_Лампа_Сеть.ema";
                Dictionary<string, Dictionary<string, string>> dictionary7 = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary7, insertPoint);

                // === 08 Вставить макрос Макрос 06 — Питание 24 В
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_Питание_24В.ema";
                Dictionary<string, Dictionary<string, string>> dictionary8 = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 09 Вставить макрос Макрос 06_Питание_24В_2
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_Питание_24В_2.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 10 Вставить макрос Макрос 07_Цепи_управления
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\07_Цепи_управления.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 11 Вставить макрос Макрос 06_ПЛК_и_модули_1
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_ПЛК_и_модули_1.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 12 Вставить макрос Макрос 06_ПЛК_и_модули_2
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_ПЛК_и_модули_2.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 13 Вставить макрос Макрос 06_ПЛК_и_модули_3
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_ПЛК_и_модули_3.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 14 Вставить макрос Макрос 06_ПЛК_и_модули_4
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_ПЛК_и_модули_4.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                // Создать страницу
                plant = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT];
                location = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION];
                functionalAssigment = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT];
                designationUserDefined = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED];
                docType = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE];
                nextNum = currentPage.Properties[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] + 1;

                nextPageProps = new PagePropertyList();

                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_PLANT] = plant;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_LOCATION] = location;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_FUNCTIONALASSIGNMENT] = functionalAssigment;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_USERDEFINED] = designationUserDefined;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.DESIGNATION_DOCTYPE] = docType;
                nextPageProps[Eplan.EplApi.DataModel.Properties.Page.PAGE_COUNTER] = nextNum;

                nextPage = new Page();
                nextPage.Create(project, currentPage.PageType, nextPageProps);

                editService = new Edit();
                editService.OpenPageWithName(nextPage.Project.ProjectLinkFilePath, nextPage.IdentifyingName);
                interpreter = new CommandLineInterpreter();
                interpreter.Execute("XGedSelectPageAction");
                interpreter.Execute("XEsSyncPDDsAction");
                interpreter.Execute("XGedEscapeAction");
                currentPage = nextPage;

                // === 15 Вставить макрос Макрос 06_ПЛК_и_модули_5
                macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\PVU_Generator\06_ПЛК_и_модули_5.ema";
                dictionary = new Dictionary<string, Dictionary<string, string>>
                {
                    ["CONTROL_CABINET"] = new Dictionary<string, string>
                    {
                        ["FUNCTION"] = "M0-85-050",
                        ["PLACE"] = "JD01-CM1001"
                    },
                };
                insertPoint = new PointD(0, 0);
                insert = new MacroGenerator(project);
                insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary8, insertPoint);

                MessageBox.Show("Схема сгенерирована");
            }
            // 
            return true;
        }

        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties properties) { }
    }
}