using UnityEngine;
using System.Reflection;

namespace QuickChat
{
    public class QuickChatWheel : MonoBehaviour
    {
        private bool isWheelActive = false;
        private int selectedIndex = -1; 
        
        private readonly float deadzoneRadius = 30f;
        private MethodInfo chatResetMethod;
        private readonly float wheelRadius = 150f;
        
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

            // Dark, highly transparent gradient
            Color centerBg = new Color(0f, 0f, 0f, 0.1f);
            Color edgeBg = new Color(0f, 0f, 0f, 0.7f);
            Color lineColor = new Color(0f, 0f, 0f, 0.9f);

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

                        // Outer border
                        if (dist > texRadius - 3f) 
                        {
                            bgTexture.SetPixel(x, y, lineColor);
                        }
                        else if (dist < 35f) 
                        {
                            if (dist > 32f) bgTexture.SetPixel(x, y, lineColor);
                            else bgTexture.SetPixel(x, y, Color.clear);
                        }
                        else
                        {
                            // Background gradient
                            float t = dist / texRadius;
                            Color bgColor = Color.Lerp(centerBg, edgeBg, t);

                            // Slice dividers (every 60 degrees starting at 30)
                            float modAngle = (angle - 30f) % 60f;
                            if (modAngle < 0) modAngle += 60f;
                            
                            // Calculate distance to the nearest divider line
                            float deltaAngle = Mathf.Min(modAngle, 60f - modAngle);
                            float arcLength = (deltaAngle * Mathf.Deg2Rad) * dist;

                            if (arcLength < 1.5f)
                            {
                                bgTexture.SetPixel(x, y, lineColor);
                            }
                            else
                            {
                                bgTexture.SetPixel(x, y, bgColor);
                            }
                        }

                        float highlightAngle = angle;
                        if (highlightAngle > 180f) highlightAngle -= 360f;
                        
                        float modForHighlight = (angle - 30f) % 60f;
                        if (modForHighlight < 0) modForHighlight += 60f;
                        float hlDeltaAngle = Mathf.Min(modForHighlight, 60f - modForHighlight);
                        float hlArcLength = (hlDeltaAngle * Mathf.Deg2Rad) * dist;

                        if (dist < texRadius - 3f && dist > 35f && highlightAngle > -30f && highlightAngle < 30f && hlArcLength >= 1.5f)
                        {
                            // Soft radial fade
                            float glowAlpha = 0.25f * (1f - (dist / texRadius));
                            highlightTexture.SetPixel(x, y, new Color(1f, 1f, 1f, glowAlpha));
                        }
                        else
                        {
                            highlightTexture.SetPixel(x, y, Color.clear);
                        }
                    }
                    else
                    {
                        bgTexture.SetPixel(x, y, Color.clear);
                        highlightTexture.SetPixel(x, y, Color.clear);
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

                selectedIndex = Mathf.FloorToInt((angle + 30f) / 60f) % 6;
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
                
                float drawAngle = selectedIndex * 60f;
                
                GUIUtility.RotateAroundPivot(drawAngle, center);
                GUI.DrawTexture(bgRect, highlightTexture);
                GUI.matrix = oldMatrix;
            }

            string[] options = new string[] 
            {
                QuickChatPlugin.ConfigTop.Value,
                QuickChatPlugin.ConfigTopRight.Value,
                QuickChatPlugin.ConfigBottomRight.Value,
                QuickChatPlugin.ConfigBottom.Value,
                QuickChatPlugin.ConfigBottomLeft.Value,
                QuickChatPlugin.ConfigTopLeft.Value
            };

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.fontSize = 17;
            textStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 1f);

            GUIStyle selectedStyle = new GUIStyle(textStyle);
            selectedStyle.normal.textColor = Color.white;
            selectedStyle.fontSize = 19; 

            float boxWidth = 200f;
            float boxHeight = 50f;

            for (int i = 0; i < 6; i++)
            {
                float drawAngle = -90f + (i * 60f);
                float rad = drawAngle * Mathf.Deg2Rad;

                float x = center.x + Mathf.Cos(rad) * textRadius - (boxWidth / 2f);
                float y = center.y + Mathf.Sin(rad) * textRadius - (boxHeight / 2f);

                Rect rect = new Rect(x, y, boxWidth, boxHeight);
                GUIStyle styleToUse = (i == selectedIndex) ? selectedStyle : textStyle;

                Rect shadowRect = new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height);
                GUIStyle shadowStyle = new GUIStyle(styleToUse);
                shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.9f);
                GUI.Label(shadowRect, options[i], shadowStyle);

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
                case 2: message = QuickChatPlugin.ConfigBottomRight.Value; break;
                case 3: message = QuickChatPlugin.ConfigBottom.Value; break;
                case 4: message = QuickChatPlugin.ConfigBottomLeft.Value; break;
                case 5: message = QuickChatPlugin.ConfigTopLeft.Value; break;
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
