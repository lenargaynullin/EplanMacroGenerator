using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.HEServices;
using LenarSoft.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Класс Action (Точка входа) Вставить макрос с подстановкой значений из словаря
namespace LenarSoft.EplanActions
{
    public class InsertTerminalBlockAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Priority)
        {
            Name = "InsertTerminalBlockAction"; // Имя экшена
            Priority = 0;                 // Приоритет (0 — стандарт)
            return true;
        }
        public bool Execute(ActionCallingContext ctx)
        {

            // 1. Вызываем логику
            Project project = new ProjectManager().CurrentProject;
            if (project == null) { MessageBox.Show("Нет проекта!"); return false; }

            SelectionSet selection = new SelectionSet();
            Page currentPage = selection.GetSelectedPages().FirstOrDefault();
            if (currentPage == null) { MessageBox.Show("Выделите страницу!"); return false; }

            string macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\GPA_Macro\401_PLC_Control_IO_Module\101_01_NIS-DAI07\101_01_NIS-DAI07.ema";

            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>
            {
                ["TERMINAL_BLOCK_1"] = new Dictionary<string, string>
                {
                    ["TERMINAL_NAME"] = "XXX777",
                    ["TERMINAL_NUMBER"] = "111",
                    ["CABLE_NAME"] = "ИМЯ КАБЕЛЯ",
                    ["CABLE_TYPE"] = "ВВГНГ",
                    ["SCHEME"] = "NIS-XXXX",
                },
                ["TERMINAL_BLOCK_2"] = new Dictionary<string, string>
                {
                    ["TERMINAL_NAME"] = "XXX777",
                    ["TERMINAL_NUMBER"] = "111",
                    ["CABLE_NAME"] = "ИМЯ КАБЕЛЯ",
                    ["CABLE_TYPE"] = "ВВГНГ",
                    ["SCHEME"] = "NIS-XXXX",
                },
            };

            double x = 0;
            double y = 0;
            PointD insertPoint = new PointD(x, y);

            var insert = new MacroGenerator(project);

            for (int i = 0; i < 8; i++)
                {
                    insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary, insertPoint);
                    y = y - 24;
                    insertPoint = new PointD(x, y);
                }
            return true;
        }
        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties props) { }
    }

}
