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
    public class InsertMacroAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Priority)
        {
            Name = "InsertMacroAction"; // Имя экшена
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

            string macroPath = @"C:\Users\Public\EPLAN\Data\Макросы\Company name\GPA_Macro\401_PLC_Control_IO_Module\001_TP-AIso\TP-AIso_1-8.ema";

            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>
            {
                ["CONTROL_CABINET"] = new Dictionary<string, string>
                {
                    ["FUNCTION"] = "M0-85-050",
                    ["PLACE"] = "JD01-CM1001"
                },
                ["MODULE_CPU_1"] = new Dictionary<string, string>
                {
                    ["MODULE_NAME"] = "2A1"
                },
                ["CABLE_1"] = new Dictionary<string, string>
                {
                    ["CABLE_NAME"] = "M0-XXXXXXXXX"
                },
                ["TERMINAL_BOARD_1"] = new Dictionary<string, string>
                {
                    ["TERMINAL_BOARD_NAME"] = "FTAXXX"
                },
                ["SIGNAL_1"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_2"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_3"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_4"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_5"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_6"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_7"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["SIGNAL_8"] = new Dictionary<string, string>
                {
                    ["ADDRESS"] = "M0-XXXXXXXXX",
                    ["SCHEME"] = "NIS-XXX77"
                },
                ["POINT_1"] = new Dictionary<string, string>
                {
                    ["POINT_NAME"] = "1"
                },
                ["POINT_2"] = new Dictionary<string, string>
                {
                    ["POINT_NAME"] = "2"
                },
            };

            PointD insertPoint = new PointD(0, 0);

            var insert = new MacroGenerator(project);
            insert.InsertMacroWithPlaceholders(currentPage, macroPath, dictionary, insertPoint);

            return true;
        }
        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties props) { }
    }

}
