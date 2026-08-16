using UnityEngine;
using System.Reflection;

namespace QuickChat
{
    public class QuickChatWheel : MonoBehaviour
    {
        private bool isWheelActive = false;
        private int selectedIndex = -1; 
        
        private MethodInfo chatResetMethod;
        
        private Texture2D bgTexture;
        private Texture2D highlightTexture;

        private readonly float bgSize = 450f; 
        private readonly float textRadius = 140f;

        private void Start()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            chatResetMethod = typeof(ChatManager).GetMethod("ChatReset", flags);

            if (chatResetMethod == null) QuickChatPlugin.Log.LogError("[QuickChatWheel] Could not find ChatReset method!");

            GenerateTextures();
        }

        private void GenerateTextures()
        {
            int texRadius = 256;
            int diameter = texRadius * 2;
            bgTexture = new Texture2D(diameter, diameter, TextureFormat.ARGB32, false);
            highlightTexture = new Texture2D(diameter, diameter, TextureFormat.ARGB32, false);

            Color clear = Color.clear;
            Color uiColor = new Color(1f, 1f, 1f, 0.8f);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - texRadius;
                    float dy = y - texRadius; 
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < texRadius)
                    {
                        float angle = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;

                        bool drawPixel = false;

                        if (dist > 35f && dist < 40f) drawPixel = true; 
                        if (dist > 44f && dist < 46f) drawPixel = true; 

                        float modAngle = (angle - 22.5f) % 45f;
                        if (modAngle < 0) modAngle += 45f;
                        float deltaAngle = Mathf.Min(modAngle, 45f - modAngle);
                        float arcLength = (deltaAngle * Mathf.Deg2Rad) * dist;

                        if (dist > 50f && arcLength < 1.0f) 
                        {
                            drawPixel = true;
                        }

                        if (drawPixel) bgTexture.SetPixel(x, y, uiColor);
                        else bgTexture.SetPixel(x, y, clear);

                        float highlightAngle = angle;
                        if (highlightAngle > 180f) highlightAngle -= 360f;
                        
                        float modForHighlight = (angle - 22.5f) % 45f;
                        if (modForHighlight < 0) modForHighlight += 45f;
                        float hlDeltaAngle = Mathf.Min(modForHighlight, 45f - modForHighlight);
                        float hlArcLength = (hlDeltaAngle * Mathf.Deg2Rad) * dist;

                        if (dist < texRadius - 3f && dist > 50f && highlightAngle > -22.5f && highlightAngle < 22.5f && hlArcLength >= 1.0f)
                        {
                            float glowAlpha = 0.25f * (1f - (dist / texRadius));
                            highlightTexture.SetPixel(x, y, new Color(1f, 1f, 1f, glowAlpha));
                        }
                        else
                        {
                            highlightTexture.SetPixel(x, y, clear);
                        }
                    }
                    else
                    {
                        bgTexture.SetPixel(x, y, clear);
                        highlightTexture.SetPixel(x, y, clear);
                    }
                }
            }

            bgTexture.Apply();
            highlightTexture.Apply();
        }

        private void Update()
        {
            try 
            {
                if (ChatManager.instance == null) return;

                bool isTyping = ChatManager.instance.chatActive;
                
                if (MenuManager.instance != null)
                {
                    isTyping = isTyping || MenuManager.instance.textInputActive;
                }

                bool vHeld = false;
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    vHeld = UnityEngine.InputSystem.Keyboard.current.vKey.isPressed;
                }

                if (vHeld && !isTyping)
                {
                    if (!isWheelActive)
                    {
                        isWheelActive = true;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = false;
                    }
                }
                else if (isWheelActive)
                {
                    isWheelActive = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    if (selectedIndex != -1 && !isTyping)
                    {
                        SendQuickChat(selectedIndex);
                    }
                }
            }
            catch (System.Exception ex)
            {
                QuickChatPlugin.Log.LogError($"[QuickChatWheel] Error in Update: {ex}");
            }
        }

        private void OnGUI()
        {
            if (!isWheelActive) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 mousePos = Event.current.mousePosition;

            float distance = Vector2.Distance(center, mousePos);
            
            float deadzone = (35f / 256f) * (bgSize / 2f);
            
            if (distance < deadzone)
            {
                selectedIndex = -1;
            }
            else
            {
                Vector2 dir = mousePos - center;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                
                angle += 90f;
                if (angle < 0) angle += 360f;

                selectedIndex = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
            }

            DrawWheel(center);
        }

        private void DrawWheel(Vector2 center)
        {
            Rect bgRect = new Rect(center.x - bgSize / 2f, center.y - bgSize / 2f, bgSize, bgSize);
            
            if (bgTexture != null)
            {
                GUI.DrawTexture(bgRect, bgTexture);
            }

            if (selectedIndex != -1 && highlightTexture != null)
            {
                Matrix4x4 oldMatrix = GUI.matrix;
                float drawAngle = selectedIndex * 45f;
                GUIUtility.RotateAroundPivot(drawAngle, center);
                GUI.DrawTexture(bgRect, highlightTexture);
                GUI.matrix = oldMatrix;
            }

            string[] options = new string[] 
            {
                QuickChatPlugin.ConfigTop.Value,
                QuickChatPlugin.ConfigTopRight.Value,
                QuickChatPlugin.ConfigRight.Value,
                QuickChatPlugin.ConfigBottomRight.Value,
                QuickChatPlugin.ConfigBottom.Value,
                QuickChatPlugin.ConfigBottomLeft.Value,
                QuickChatPlugin.ConfigLeft.Value,
                QuickChatPlugin.ConfigTopLeft.Value
            };

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.fontSize = 17;
            textStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);

            GUIStyle selectedStyle = new GUIStyle(textStyle);
            selectedStyle.normal.textColor = Color.white;
            selectedStyle.fontSize = 19; 

            for (int i = 0; i < 8; i++)
            {
                float drawAngle = -90f + (i * 45f);
                float rad = drawAngle * Mathf.Deg2Rad;

                bool isSelected = (i == selectedIndex);
                GUIStyle styleToUse = isSelected ? selectedStyle : textStyle;

                Vector2 textSize = styleToUse.CalcSize(new GUIContent(options[i]));
                float boxW = textSize.x + 20f;
                float boxH = textSize.y + 10f;

                float x = center.x + Mathf.Cos(rad) * textRadius - (boxW / 2f);
                float y = center.y + Mathf.Sin(rad) * textRadius - (boxH / 2f);

                Rect rect = new Rect(x, y, boxW, boxH);
                GUI.Label(rect, options[i], styleToUse);
            }
        }

        private void SendQuickChat(int index)
        {
            string message = "";
            switch (index)
            {
                case 0: message = QuickChatPlugin.ConfigTop.Value; break;
                case 1: message = QuickChatPlugin.ConfigTopRight.Value; break;
                case 2: message = QuickChatPlugin.ConfigRight.Value; break;
                case 3: message = QuickChatPlugin.ConfigBottomRight.Value; break;
                case 4: message = QuickChatPlugin.ConfigBottom.Value; break;
                case 5: message = QuickChatPlugin.ConfigBottomLeft.Value; break;
                case 6: message = QuickChatPlugin.ConfigLeft.Value; break;
                case 7: message = QuickChatPlugin.ConfigTopLeft.Value; break;
            }

            if (string.IsNullOrEmpty(message)) return;

            chatResetMethod?.Invoke(ChatManager.instance, null);
            
            foreach (char c in message)
            {
                ChatManager.instance.AddLetterToChat(c.ToString());
            }

            ChatManager.instance.ForceConfirmChat();
        }
    }
}
