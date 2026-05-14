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

namespace LenarSoft.EplanActions
{
    // Класс Interaction для интерактивной выноски
    public class RailCalloutInteraction : Interaction
    {
        public static ViewPart SelectedViewPart;
        public static string CalloutText;
        public static double ViewScale = 1.0;
        public static string LayerName = "Выноски";
        public static double FontSize = 2.5;

        private double _textsDistance;
        private PointD _cursorPosition;

        public override RequestCode OnStart(InteractionContext oContext)
        {
            // Используем статические поля для получения данных
            ViewScale = RailCalloutInteraction.ViewScale;
            FontSize = RailCalloutInteraction.FontSize;
            LayerName = RailCalloutInteraction.LayerName;
            CalloutText = RailCalloutInteraction.CalloutText;

            _textsDistance = FontSize * 1.5;
            base.Description = "Создание выноски для дин-рейки";

            return RequestCode.Point;
        }

        public override RequestCode OnPoint(Position oPosition)
        {
            // Сохраняем позицию клика
            _cursorPosition = oPosition.FinalPosition;
            return RequestCode.Success;
        }
                
        // 24.02.26 Текстовые выноски
        public override void OnSuccess(InteractionContext oContext)
        {
            try
            {
                if (SelectedViewPart == null || !SelectedViewPart.IsValid)
                {
                    new Decider().Decide(EnumDecisionType.eOkDecision,
                        "Объект для выноски не найден!", "Ошибка",
                        EnumDecisionReturn.eOK, EnumDecisionReturn.eOK);
                    return;
                }

                // Проверяем слой
                GraphicalLayer targetLayer = SelectedViewPart.Project.LayerTable.Layers
                    .FirstOrDefault(l => l.Name == LayerName);

                if (targetLayer == null)
                {
                    targetLayer = SelectedViewPart.Project.LayerTable.Layers
                        .FirstOrDefault(l => l.Name == "0");

                    if (targetLayer == null)
                    {
                        new Decider().Decide(EnumDecisionType.eOkDecision,
                            "Слой '0' не найден в проекте!", "Ошибка",
                            EnumDecisionReturn.eOK, EnumDecisionReturn.eOK);
                        return;
                    }
                }

                // ПОЛУЧАЕМ BOUNDING BOX РЕЙКИ
                PointD[] boundingBox = SelectedViewPart.GetBoundingBox();

                if (boundingBox == null || boundingBox.Length != 2)
                {
                    new Decider().Decide(EnumDecisionType.eOkDecision,
                        "Не удалось получить габариты рейки!", "Ошибка",
                        EnumDecisionReturn.eOK, EnumDecisionReturn.eOK);
                    return;
                }

                // ВЫЧИСЛЯЕМ ВСЕ УГЛЫ РЕЙКИ
                PointD lowerLeft = boundingBox[0];      // левый нижний
                PointD upperRight = boundingBox[1];     // правый верхний
                PointD upperLeft = new PointD(boundingBox[0].X, boundingBox[1].Y); // левый верхний
                PointD lowerRight = new PointD(boundingBox[1].X, boundingBox[0].Y); // правый нижний

                // Центр рейки
                PointD railCenter = new PointD(
                    (boundingBox[0].X + boundingBox[1].X) / 2,
                    (boundingBox[0].Y + boundingBox[1].Y) / 2
                );

                // ПОЗИЦИЯ ТЕКСТА (клик пользователя)
                PointD textPosition = _cursorPosition;

                // РАЗБИВАЕМ ТЕКСТ НА СТРОКИ ДЛЯ ОПРЕДЕЛЕНИЯ ДЛИНЫ
                string[] textLines = CalloutText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                int lineCount = textLines.Length;

                // ВЫЧИСЛЯЕМ МАКСИМАЛЬНУЮ ДЛИНУ СТРОКИ
                int maxLineLength = 0;
                if (lineCount > 0)
                {
                    maxLineLength = textLines.Max(line => line.Length);
                }

                // ДЛИНА ТЕКСТА В ММ (примерно 0.7 * размер шрифта * количество символов)
                double textWidth = Math.Ceiling(maxLineLength * FontSize * 0.7);

                // ЕСЛИ КЛИК СЛЕВА ОТ РЕЙКИ, СМЕЩАЕМ ТЕКСТ ВЛЕВО
                if (textPosition.X < boundingBox[0].X)
                {
                    textPosition.X -= textWidth; // смещаем влево на ширину текста + небольшой отступ
                }

                // СОЗДАЕМ ТЕКСТ
                Text textObj = new Text();
                MultiLangString mls = new MultiLangString();
                mls.AddString(ISOCode.Language.L___, CalloutText);
                textObj.Create(SelectedViewPart.Page, mls, FontSize / ViewScale);
                textObj.Location = textPosition;
                textObj.Layer = targetLayer;

                // ОПРЕДЕЛЯЕМ ТОЧКУ ПРИВЯЗКИ В ЗАВИСИМОСТИ ОТ ПОЛОЖЕНИЯ ТЕКСТА
                PointD anchorPoint;
                bool isLeftSide;

                if (textPosition.X < boundingBox[0].X)
                {
                    // Текст слева от рейки → привязываемся к середине левой стороны
                    anchorPoint = new PointD(
                        boundingBox[0].X,
                        (boundingBox[0].Y + boundingBox[1].Y) / 2
                    );
                    isLeftSide = true;
                }
                else if (textPosition.X > boundingBox[1].X)
                {
                    // Текст справа от рейки → привязываемся к середине правой стороны
                    anchorPoint = new PointD(
                        boundingBox[1].X,
                        (boundingBox[0].Y + boundingBox[1].Y) / 2
                    );
                    isLeftSide = false;
                }
                else
                {
                    // Текст над или под рейкой → привязываемся к центру
                    anchorPoint = railCenter;
                    isLeftSide = textPosition.X < anchorPoint.X;
                }

                // СОЗДАЕМ ЛИНИЮ С ПОЛОЧКОЙ
                PolyLine polyline = new PolyLine();
                polyline.Create(SelectedViewPart.Page);

                // Три точки для полилинии
                PointD[] linePoints = new PointD[3];

                if (isLeftSide)
                {
                    // Полочка слева от текста
                    linePoints[0].X = textPosition.X;
                    linePoints[0].Y = textPosition.Y;
                    linePoints[1].X = textPosition.X + textWidth;
                    linePoints[1].Y = textPosition.Y;
                }
                else
                {
                    // Полочка справа от текста
                    linePoints[0].X = textPosition.X + textWidth;
                    linePoints[0].Y = textPosition.Y;
                    linePoints[1].X = textPosition.X;
                    linePoints[1].Y = textPosition.Y;
                }

                // Третья точка - к рейке
                linePoints[2].X = anchorPoint.X;
                linePoints[2].Y = anchorPoint.Y;

                // Добавляем точки в полилинию
                polyline.SetPointAt(0, ref linePoints[0]);
                polyline.SetPointAt(1, ref linePoints[1]);
                polyline.SetPointAt(2, ref linePoints[2]);

                polyline.Layer = targetLayer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка OnSuccess: {ex.Message}");
                new Decider().Decide(EnumDecisionType.eOkDecision,
                    $"Ошибка создания выноски: {ex.Message}", "Ошибка",
                    EnumDecisionReturn.eOK, EnumDecisionReturn.eOK);
            }

        }
    }
}