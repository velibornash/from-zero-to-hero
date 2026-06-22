using UnityEngine;

namespace FromZeroToHero.Core
{
    public sealed class HUDController : MonoBehaviour
    {
        private GameState state;
        private string objective = string.Empty;
        private string eventFeed = string.Empty;
        private string hint =
            "Strelice pomeraju heroja po skrivenoj mreži. E kupuje/aktivira objekat na lokaciji. Klikom na HUD ne upravljaš igrom.";

        private GUIStyle topStyle;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle hintStyle;

        public void Build()
        {
            topStyle = CreateStyle(new Color(0.05f, 0.08f, 0.12f, 0.80f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
            panelStyle = CreateStyle(new Color(0.05f, 0.08f, 0.12f, 0.78f), 16, FontStyle.Normal, TextAnchor.UpperLeft);
            titleStyle = CreateTextStyle(18, FontStyle.Bold, new Color(0.92f, 0.95f, 0.99f, 1f), TextAnchor.MiddleLeft);
            bodyStyle = CreateTextStyle(15, FontStyle.Normal, new Color(0.92f, 0.95f, 0.99f, 1f), TextAnchor.UpperLeft);
            hintStyle = CreateTextStyle(13, FontStyle.Normal, new Color(0.80f, 0.86f, 0.93f, 1f), TextAnchor.MiddleLeft);
            hintStyle.wordWrap = true;
        }

        public void UpdateValues(GameState gameState, string objectiveText, string eventText)
        {
            state = gameState;
            objective = objectiveText ?? string.Empty;
            eventFeed = eventText ?? string.Empty;
        }

        private void OnGUI()
        {
            if (state == null)
                return;

            DrawTopBar();
            DrawLeftPanel();
            DrawRightPanel();
            DrawBottomBar();
        }

        private void DrawTopBar()
        {
            GUI.Box(new Rect(16, 16, Screen.width - 32, 78), GUIContent.none, topStyle);
            GUILayout.BeginArea(new Rect(32, 26, Screen.width - 64, 58));
            GUILayout.Label($"Wood {state.Wood}   Food {state.Food}   Gold {state.Gold}   Morale {state.Morale}   Day {state.Day}   Base {state.BaseHp}", titleStyle);
            GUILayout.EndArea();
        }

        private void DrawLeftPanel()
        {
            GUI.Box(new Rect(16, 110, 320, Screen.height - 196), GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(30, 124, 292, Screen.height - 226));
            GUILayout.Label("Objectives", titleStyle);
            GUILayout.Space(8);
            GUILayout.Label(objective, bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawRightPanel()
        {
            GUI.Box(new Rect(Screen.width - 336, 110, 320, Screen.height - 196), GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(Screen.width - 322, 124, 292, Screen.height - 226));
            GUILayout.Label("Event Feed", titleStyle);
            GUILayout.Space(8);
            GUILayout.Label(eventFeed, bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawBottomBar()
        {
            GUI.Box(new Rect(16, Screen.height - 94, Screen.width - 32, 78), GUIContent.none, topStyle);
            GUILayout.BeginArea(new Rect(30, Screen.height - 84, Screen.width - 60, 60));
            GUILayout.Label(hint, hintStyle);
            GUILayout.EndArea();
        }

        private static GUIStyle CreateStyle(Color background, int size, FontStyle fontStyle, TextAnchor anchor)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = size,
                fontStyle = fontStyle,
                alignment = anchor,
                padding = new RectOffset(14, 14, 12, 12),
                border = new RectOffset(8, 8, 8, 8),
                normal = { textColor = new Color(0.92f, 0.95f, 0.99f, 1f) }
            };

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, background);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }

        private static GUIStyle CreateTextStyle(int size, FontStyle fontStyle, Color color, TextAnchor anchor)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = fontStyle,
                alignment = anchor,
                wordWrap = true,
                normal = { textColor = color }
            };
        }
    }
}
