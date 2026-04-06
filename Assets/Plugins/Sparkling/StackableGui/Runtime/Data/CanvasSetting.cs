using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkling.StackableGui
{
    [Serializable]
    public struct CanvasSetting
    {
        /// <summary>Render mode of the canvas (ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace).</summary>
        public RenderMode CanvasRenderMode;

        /// <summary>Scaling strategy applied by the CanvasScaler.</summary>
        public CanvasScaler.ScaleMode ScaleMode;

        /// <summary>Screen match mode used when scaling with screen size.</summary>
        public CanvasScaler.ScreenMatchMode MatchMode;

        /// <summary>The resolution the UI is designed for.</summary>
        public Vector2 ReferenceResolution;

        /// <summary>Blend between width (0) and height (1) when matching screen size.</summary>
        public float Match;

        /// <summary>Pixels per unit used as reference by the scaler.</summary>
        public float ReferencePixelsPerUnit;

        /// <summary>Sorting order assigned to the parent canvas.</summary>
        public int BaseOrder;

        /// <summary>Sorting order increment between each canvas layer. Must be greater than zero.</summary>
        public int OrderStep;

        /// <summary>Default preset for PC and console (1920x1080, match 0.5).</summary>
        public static readonly CanvasSetting Default = new CanvasSetting
        {
            CanvasRenderMode = RenderMode.ScreenSpaceOverlay,
            ScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize,
            MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight,
            ReferenceResolution = new Vector2(1920f, 1080f),
            Match = 0.5f,
            ReferencePixelsPerUnit = 100f,
            BaseOrder = 0,
            OrderStep = 10
        };

        /// <summary>Preset for portrait mobile screens (1080x1920, match width).</summary>
        public static readonly CanvasSetting Mobile = new CanvasSetting
        {
            CanvasRenderMode = RenderMode.ScreenSpaceOverlay,
            ScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize,
            MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight,
            ReferenceResolution = new Vector2(1080f, 1920f),
            Match = 0f,
            ReferencePixelsPerUnit = 100f,
            BaseOrder = 0,
            OrderStep = 10
        };

        /// <summary>Preset for high DPI screens (2560x1440, match 0.5, 200 pixels per unit).</summary>
        public static readonly CanvasSetting HighDpi = new CanvasSetting
        {
            CanvasRenderMode = RenderMode.ScreenSpaceOverlay,
            ScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize,
            MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight,
            ReferenceResolution = new Vector2(2560f, 1440f),
            Match = 0.5f,
            ReferencePixelsPerUnit = 200f,
            BaseOrder = 0,
            OrderStep = 10
        };
    }
}