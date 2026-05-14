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
    /// <summary>
    /// Генератор для вставки макросов с обработкой плейсхолдеров
    /// </summary>
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
            Dictionary<string, Dictionary<string, string>> placeholderValues, // Словарь из Action
            PointD insertionPoint)
        {
            if (!System.IO.File.Exists(macroPath))
                throw new System.Exception($"Макрос не найден: {macroPath}");

            try
            {
                // 1. Вставляем макрос
                StorableObject[] macroObjects = InsertMacro(targetPage, macroPath, insertionPoint);

                if (macroObjects == null || macroObjects.Length == 0)
                    return;

                // 2. Блокируем все объекты для изменений
                LockAllObjects(macroObjects);

                // 3. Обрабатываем все плейсхолдеры, используя переданный словарь
                ProcessAllPlaceholders(macroObjects, placeholderValues);

                // 4. Обновляем графику
                RefreshGraphics();
            }
            catch (System.Exception ex)
            {
                HandleError("Ошибка при вставке макроса", ex);
            }
        }

        #region Private Methods

        /// <summary>
        /// Вставка макроса на страницу
        /// </summary>
        private StorableObject[] InsertMacro(Page targetPage, string macroPath, PointD insertionPoint)
        {
            var objects = _insert.WindowMacro(
                macroPath,
                0,
                targetPage,
                insertionPoint,
                Insert.MoveKind.Absolute
            );
            return objects;
        }

        /// <summary>
        /// Блокировка всех объектов для редактирования
        /// </summary>
        private void LockAllObjects(StorableObject[] objects)
        {
            foreach (StorableObject obj in objects)
            {
                try
                {
                    obj.LockObject();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Не удалось заблокировать объект: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Обработка всех плейсхолдеров в массиве объектов
        /// </summary>
        private void ProcessAllPlaceholders(
            StorableObject[] objects,
            Dictionary<string, Dictionary<string, string>> placeholderValues)
        {
            foreach (StorableObject selObj in objects)
            {
                ProcessSingleObject(selObj, placeholderValues);
            }
        }

        /// <summary>
        /// Обработка одного объекта (если это плейсхолдер)
        /// </summary>
        private void ProcessSingleObject(
            StorableObject selObj,
            Dictionary<string, Dictionary<string, string>> placeholderValues)
        {
            if (!(selObj is PlaceHolder placeholder))
                return;

            // Проверяем, есть ли значения для этого плейсхолдера в переданном словаре
            if (placeholderValues.TryGetValue(placeholder.Name, out var values))
            {
                UpdatePlaceholderWithValues(placeholder, values);
            }
        }

        /// <summary>
        /// Обновление плейсхолдера с заданными значениями
        /// </summary>
        private void UpdatePlaceholderWithValues(PlaceHolder placeholder, Dictionary<string, string> values)
        {
            try
            {
                // Обновляем ссылку после блокировки (на всякий случай)
                placeholder = placeholder as PlaceHolder;

                // Применяем все значения из словаря
                foreach (var kvp in values)
                {
                    SetPlaceholderValue(placeholder, kvp.Key, kvp.Value);
                }

                // Сохраняем изменения
                placeholder.ApplyRecord("Default", true);
                placeholder.Remove();
            }
            catch (Exception ex)
            {
                HandleError($"Ошибка при обновлении плейсхолдера {placeholder.Name}", ex);
            }
        }

        /// <summary>
        /// Установка значения для конкретной переменной плейсхолдера
        /// </summary>
        private void SetPlaceholderValue(PlaceHolder placeholder, string variableName, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            var multiLangString = CreateMultiLangString(value);
            placeholder.SetValue("Default", variableName, multiLangString);
        }

        /// <summary>
        /// Создание мультиязычной строки
        /// </summary>
        private MultiLangString CreateMultiLangString(string value)
        {
            var mls = new MultiLangString();
            mls.AddString(ISOCode.Language.L___, value);
            return mls;
        }

        /// <summary>
        /// Обновление графики
        /// </summary>
        private void RefreshGraphics()
        {
            new Edit().RedrawGed();
        }

        /// <summary>
        /// Обработка ошибок
        /// </summary>
        private void HandleError(string message, Exception ex)
        {
            string fullMessage = $"{message}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(fullMessage);
            MessageBox.Show(fullMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion
    }
}