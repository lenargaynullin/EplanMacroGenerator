using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.Graphics;
using Eplan.EplApi.Gui;
using Eplan.EplApi.HEServices;
using LenarSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace LenarSoft.Generators
{
    // 
    internal class MacroGenerator
    {
        private readonly Project _project;
        private readonly Insert _insert;

        public MacroGenerator(Project project)
        {
            _project = project;
            _insert = new Insert();
        }

        /// <summary>
        /// Вставить макрос с подстановкой значений из словаря
        /// </summary>
        public void InsertMacroWithPlaceholders(
        Page targetPage,
        string macroPath,
        Dictionary<string, string> placeholderValues,
        PointD insertionPoint)
        {
            if (!System.IO.File.Exists(macroPath))
                throw new System.Exception($"Макрос не найден: {macroPath}");

            try
            {
                // 1. Вставляем макрос
                StorableObject[] macroObjects = _insert.WindowMacro(
                    macroPath,
                    0,
                    targetPage,
                    insertionPoint,
                    Insert.MoveKind.Absolute
                );
                new Edit().RedrawGed();

                // 2. Обрабатываем объекты напрямую из массива вставки

                if (macroObjects != null && macroObjects.Length > 0)

                foreach (StorableObject selObj in macroObjects)
                {
                    selObj.LockObject();
                }
                

                foreach (StorableObject selObj in macroObjects)
                {
                    // CONTROL_CABINET
                    if (selObj is PlaceHolder placeholder && placeholder.Name == "CONTROL_CABINET")
                    {
                        // 1. ЗАБЛОКИРОВАТЬ объект перед изменением
                        placeholder.LockObject();
                        placeholder = selObj as PlaceHolder;

                        // Ввести значения
                        MultiLangString oMultiLangString = new MultiLangString();
                        oMultiLangString.AddString(ISOCode.Language.L___, "M0-85-050");
                        placeholder.SetValue("Default", "FUNCTION", oMultiLangString);

                        oMultiLangString.AddString(ISOCode.Language.L___, "JD01-CM1001");
                        placeholder.SetValue("Default", "PLACE", oMultiLangString);
                        placeholder.ApplyRecord("Default", true);
                    }

                    // MODULE_CPU_1
                    if (selObj is PlaceHolder placeholder2 && placeholder2.Name == "MODULE_CPU_1")
                    {
                        // 1. ЗАБЛОКИРОВАТЬ объект перед изменением
                        placeholder2.LockObject();
                        placeholder2 = selObj as PlaceHolder;

                        // Ввести значения
                        MultiLangString oMultiLangString = new MultiLangString();
                        oMultiLangString.AddString(ISOCode.Language.L___, "2A1");
                        placeholder2.SetValue("Default", "MODULE_NAME", oMultiLangString);
                        placeholder2.ApplyRecord("Default", true);
                    }

                    // SIGNAL
                    if (selObj is PlaceHolder placeholder3 && placeholder3.Name == "SIGNAL_1")
                    {
                        // 1. ЗАБЛОКИРОВАТЬ объект перед изменением
                        placeholder3.LockObject();
                        placeholder3 = selObj as PlaceHolder;

                        // Ввести значения
                        MultiLangString oMultiLangString = new MultiLangString();
                        oMultiLangString.AddString(ISOCode.Language.L___, "M0-XXXXXXXXX");
                        placeholder3.SetValue("Default", "ADDRESS", oMultiLangString);
                        placeholder3.ApplyRecord("Default", true);

                        oMultiLangString.AddString(ISOCode.Language.L___, "NIS-XXX77");
                        placeholder3.SetValue("Default", "SCHEME", oMultiLangString);
                        placeholder3.ApplyRecord("Default", true);
                    }

                    // CABLE
                    if (selObj is PlaceHolder placeholder4 && placeholder4.Name == "CABLE_1")
                    {
                        // 1. ЗАБЛОКИРОВАТЬ объект перед изменением
                        placeholder4.LockObject();
                        placeholder4 = selObj as PlaceHolder;

                        // Ввести значения
                        MultiLangString oMultiLangString = new MultiLangString();
                        oMultiLangString.AddString(ISOCode.Language.L___, "M0-XXXXXXXXX");
                        placeholder4.SetValue("Default", "CABLE_NAME", oMultiLangString);
                        placeholder4.ApplyRecord("Default", true);
                    }

                    // TERMINAL
                    if (selObj is PlaceHolder placeholder5 && placeholder5.Name == "TERMINAL_BOARD_1")
                    {
                        // 1. ЗАБЛОКИРОВАТЬ объект перед изменением
                        placeholder5.LockObject();
                        placeholder5 = selObj as PlaceHolder;

                        // Ввести значения
                        MultiLangString oMultiLangString = new MultiLangString();
                        oMultiLangString.AddString(ISOCode.Language.L___, "FTAXXX");
                        placeholder5.SetValue("Default", "TERMINAL_BOARD_NAME", oMultiLangString);
                        placeholder5.ApplyRecord("Default", true);
                    }
                }
                // Обновление графики
                new Edit().RedrawGed();
            }
            catch (System.Exception ex)
            {
                // Логируем ошибку, если нужно
                MessageBox.Show($"Ошибка при вставке макроса: {ex.Message}");
            }

        }
    }
}