﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Il2CppBigAmbitions.Characters;

namespace UberSideJobMod
{
    namespace UberSideJobMod
    {
        public class UberUIManager
        {
            private GameObject uberUICanvas;
            private GameObject uberPanel;
            private RectTransform uberPanelRect;
            public TMP_Text titleText; // Public for UberJobMod access
            public TMP_Text statusText;
            public TMP_Text driverStatsText;
            public TMP_Text passengerInfoText;
            public TMP_Text fareInfoText;
            public TMP_Text surgeText;

            private TMP_FontAsset rubikFont;

            public Button acceptButton;
            public Button declineButton;
            public Button cancelButton;
            private TMP_SpriteAnimator uberPanelAnimator;
            private bool uiInitialized = false;

            // New field for rounded sprite
            private Sprite roundedSprite;
            private Sprite buttonRoundedSprite;
            // Events for button clicks
            public Action OnAcceptClicked;
            public Action OnDeclineClicked;
            public Action OnCancelClicked;

            private void InitializeFont()
            {
                try
                {
                    rubikFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                        .FirstOrDefault(f => f.name == "Rubik-Medium SDF");

                    if (rubikFont != null)
                    {
                    }
                    else
                    {
                        rubikFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                            .FirstOrDefault(f => f.name.Contains("Rubik"));

                        if (rubikFont != null)
                        {
                        }
                        else
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error initializing font: {ex.Message}");
                }
            }

            private void ApplyRubikFont(TMP_Text textComponent)
            {
                if (rubikFont != null)
                {
                    textComponent.font = rubikFont;
                }
            }

            private void ApplyFontToAllTexts()
            {
                if (rubikFont == null) return;

                ApplyRubikFont(titleText);
                ApplyRubikFont(statusText);
                ApplyRubikFont(driverStatsText);
                ApplyRubikFont(passengerInfoText);
                ApplyRubikFont(fareInfoText);
                ApplyRubikFont(surgeText);

                TMP_Text acceptText = acceptButton.GetComponentInChildren<TMP_Text>();
                TMP_Text declineText = declineButton.GetComponentInChildren<TMP_Text>();
                TMP_Text cancelText = cancelButton.GetComponentInChildren<TMP_Text>();

                if (acceptText != null) ApplyRubikFont(acceptText);
                if (declineText != null) ApplyRubikFont(declineText);
                if (cancelText != null) ApplyRubikFont(cancelText);
            }

            public void InitializeUI()
            {
                try
                {
                    InitializeFont();
                    if (uberUICanvas != null)
                    {
                        MelonLogger.Warning("uberUICanvas already exists, destroying old instance...");
                        GameObject.Destroy(uberUICanvas);
                    }

                    CreateCanvas();
                    if (uberUICanvas == null) throw new Exception("CreateCanvas failed to set uberUICanvas");

                    roundedSprite = CreateRoundedRectangleSprite(256, 256, 8);
                    buttonRoundedSprite = CreateRoundedRectangleSprite(128, 128, 4);

                    CreateMainPanel();
                    CreateTitleBar();
                    GameObject contentPanel = CreateContentPanel();
                    CreateDriverStats(contentPanel);
                    CreateSurgePricing(contentPanel);
                    CreateDivider(contentPanel);
                    CreatePassengerInfo(contentPanel);
                    CreateStatusText(contentPanel);
                    CreateFareInfo(contentPanel);
                    CreateButtons(contentPanel);
                    SetupPanelTransitions();
                    ApplyFontToAllTexts();
                    if (uberUICanvas == null) throw new Exception("uberUICanvas became null during initialization");
                    uberUICanvas.SetActive(false); // Hidden initially
                    uiInitialized = true;
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to initialize Uber UI: {ex.Message}\nStack: {ex.StackTrace}");
                    uiInitialized = false;
                    uberUICanvas = null;
                }
            }

            public void SetUIVisible(bool visible)
            {
                if (uberUICanvas == null)
                {
                    InitializeUI();
                    if (uberUICanvas == null)
                    {
                        MelonLogger.Error("Reinitialization failed, UI cannot be shown");
                        return;
                    }
                }
                uberUICanvas.SetActive(visible);
            }

            public void UpdateUI(DriverStats driverStats, UberPassenger currentPassenger, bool isPeakHours, bool uberJobActive)
            {
                if (!uiInitialized || uberUICanvas == null || !uberUICanvas.activeSelf) return;

                try
                {
                    driverStatsText.text = $"Rating: {driverStats.averageRating:F1} ★ | Trips: {driverStats.totalRides} | Cancelled: {driverStats.cancelledRides}\n" +
                                           $"Top Area: {driverStats.mostVisitedNeighborhood} | Longest Ride: {FormatDistance(driverStats.longestRide)}\nEarnings: ${driverStats.totalEarnings:F2}\n";
                    surgeText.gameObject.SetActive(isPeakHours);
                    acceptButton.transform.parent.gameObject.SetActive(currentPassenger != null && !uberJobActive);
                    cancelButton.gameObject.SetActive(uberJobActive);

                    if (currentPassenger == null)
                    {
                        string tipStatsInfo = $"No active passenger requests at the moment.\nPress F3 to toggle Uber Job.\n\n" +
                                              $"Tip Statistics:\nTotal Tips: ${driverStats.totalTipsEarned:F2}\n" +
                                              $"Highest Tip: ${driverStats.highestTip:F2}\n" +
                                              $"Rides with Tips: {driverStats.ridesWithTips}/{driverStats.totalRides} " +
                                              $"({(driverStats.totalRides > 0 ? (driverStats.ridesWithTips * 100f / driverStats.totalRides).ToString("F0") : "0")}%)\n" +
                                              $"Avg Tip: {(driverStats.averageTipPercentage * 100):F0}%";

                        if (passengerInfoText.text != tipStatsInfo)
                        {
                            passengerInfoText.text = tipStatsInfo;
                            statusText.text = "";
                            fareInfoText.text = "";
                            LayoutRebuilder.ForceRebuildLayoutImmediate(uberPanelRect);
                        }
                        return;
                    }

                    if (!uberJobActive)
                    {
                        string newPassengerText = $"<b>Passenger:</b> {currentPassenger.passengerName}\n" +
                                                 $"<b>Pickup:</b> {currentPassenger.pickupAddress.DisplayName}, {currentPassenger.pickupAddress.neighborhood}\n" +
                                                 $"<b>Dropoff:</b> {currentPassenger.dropoffAddress.DisplayName}, {currentPassenger.dropoffAddress.neighborhood}";
                        if (passengerInfoText.text != newPassengerText)
                        {
                            passengerInfoText.text = newPassengerText;
                            statusText.text = "";
                            statusText.gameObject.SetActive(false);
                            fareInfoText.text = $"<b>Est. Fare:</b> ${currentPassenger.fare:F2}" + (isPeakHours ? " <color=#FF8800>(Surge)</color>" : "");
                            statusText.rectTransform.sizeDelta = new Vector2(0, 0);
                            LayoutRebuilder.ForceRebuildLayoutImmediate(uberPanelRect);
                        }
                    }
                    else
                    {
                        statusText.gameObject.SetActive(true);
                        string newPassengerText = $"<b>Passenger:</b> {currentPassenger.passengerName} ({currentPassenger.passengerType})";
                        string newStatusText = currentPassenger.isPickedUp ?
                            "<color=#00FF00>In progress - Drive to destination</color>" :
                            $"<color=#FFCC00>Waiting for pickup ({(int)(currentPassenger.waitTime)}s)</color>";
                        string newFareText = currentPassenger.isPickedUp ?
                            $"<b>Destination:</b> {currentPassenger.dropoffAddress.DisplayName}\n<b>Fare:</b> ${currentPassenger.fare:F2}" :
                            $"<b>Pickup:</b> {currentPassenger.pickupAddress.DisplayName}\n<b>Fare:</b> ${currentPassenger.fare:F2}";

                        if (passengerInfoText.text != newPassengerText || statusText.text != newStatusText || fareInfoText.text != newFareText)
                        {
                            passengerInfoText.text = newPassengerText;
                            statusText.text = newStatusText;
                            fareInfoText.text = newFareText;
                            LayoutRebuilder.ForceRebuildLayoutImmediate(uberPanelRect);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error updating Uber UI: {ex.Message}");
                }
            }
            private void CreateCanvas()
            {
                uberUICanvas = new GameObject("UberUICanvas");
                Canvas canvas = uberUICanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                CanvasScaler scaler = uberUICanvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                uberUICanvas.AddComponent<GraphicRaycaster>();
                if (uberUICanvas.GetComponent<RectTransform>() == null)
                {
                    MelonLogger.Error("Canvas missing RectTransform after creation!");
                }
            }

            private void CreateMainPanel()
            {
                uberPanel = new GameObject("UberPanel");
                uberPanel.transform.SetParent(uberUICanvas.transform, false);
                Image panelImage = uberPanel.AddComponent<Image>();
                if (panelImage == null) MelonLogger.Error("Failed to add Image component");

                panelImage.sprite = roundedSprite;
                panelImage.type = Image.Type.Sliced;
                panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
                
                // Add Mask to clip content to rounded shape
                uberPanel.AddComponent<Mask>().showMaskGraphic = true;

                uberPanelRect = uberPanel.GetComponent<RectTransform>();
                if (uberPanelRect == null) MelonLogger.Error("Failed to get RectTransform component");
                uberPanelRect.anchorMin = new Vector2(0, 1);
                uberPanelRect.anchorMax = new Vector2(0, 1);
                uberPanelRect.pivot = new Vector2(0, 1);
                uberPanelRect.sizeDelta = new Vector2(390, 320);
                uberPanelRect.anchoredPosition = new Vector2(50, -710);

                uberPanelAnimator = uberPanel.AddComponent<TMP_SpriteAnimator>();
            }

            private void CreateTitleBar()
            {
                GameObject titleBar = CreateUIElement("TitleBar", uberPanel.transform);
                RectTransform titleRect = titleBar.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 1);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.sizeDelta = new Vector2(0, 40);
                titleRect.anchoredPosition = Vector2.zero;

                // Modern gradient title bar
                Image titleBg = titleBar.AddComponent<Image>();
                titleBg.color = new Color(0f, 0f, 0f, 0.7906f);

                GameObject titleTextObj = CreateUIElement("TitleText", titleBar.transform);
                titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
                titleText.text = "UBER";
                titleText.fontSize = 14;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = Color.white;
                titleText.alignment = TextAlignmentOptions.Center;
                RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
                titleTextRect.anchorMin = Vector2.zero;
                titleTextRect.anchorMax = Vector2.one;
                titleTextRect.sizeDelta = Vector2.zero;

                // Add Uber's signature green accent
                GameObject uberStrip = CreateUIElement("UberColorStrip", titleBar.transform);
                Image stripImage = uberStrip.AddComponent<Image>();
                stripImage.color = new Color(0.2f, 0.6f, 1f, 1f);
                RectTransform stripRect = uberStrip.GetComponent<RectTransform>();
                stripRect.anchorMin = new Vector2(0, 0);
                stripRect.anchorMax = new Vector2(1, 0);
                stripRect.pivot = new Vector2(0.5f, 0);
                stripRect.sizeDelta = new Vector2(0, 3);
                stripRect.anchoredPosition = Vector2.zero;

            }

            private GameObject CreateContentPanel()
            {
                GameObject contentPanel = CreateUIElement("ContentPanel", uberPanel.transform);
                RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.offsetMin = new Vector2(15, 15);
                contentRect.offsetMax = new Vector2(-15, -45);
                VerticalLayoutGroup verticalLayout = contentPanel.AddComponent<VerticalLayoutGroup>();
                verticalLayout.padding = new RectOffset(5, 5, 5, 5);
                verticalLayout.spacing = 10;
                verticalLayout.childAlignment = TextAnchor.UpperLeft;
                verticalLayout.childControlHeight = true;
                verticalLayout.childForceExpandHeight = false;
                verticalLayout.childForceExpandWidth = true;
                return contentPanel;
            }

            private void CreateDriverStats(GameObject parent)
            {
                GameObject driverStatsObj = CreateUIElement("DriverStats", parent.transform);
                driverStatsText = driverStatsObj.AddComponent<TextMeshProUGUI>();
                driverStatsText.fontSize = 13;
                driverStatsText.alignment = TextAlignmentOptions.TopLeft;
                driverStatsText.fontStyle = FontStyles.Bold;
                driverStatsText.fontWeight = FontWeight.Regular;
                driverStatsText.color = new Color(0.8f, 0.8f, 0.8f); // Softer white for better readability
                driverStatsText.enableWordWrapping = true;
                RectTransform driverStatsRect = driverStatsObj.GetComponent<RectTransform>();
                driverStatsRect.sizeDelta = new Vector2(0, 50);
            }

            private void CreateSurgePricing(GameObject parent)
            {
                GameObject surgeObj = CreateUIElement("SurgeText", parent.transform);
                surgeText = surgeObj.AddComponent<TextMeshProUGUI>();
                surgeText.text = "SURGE PRICING ACTIVE - Higher fares!";
                surgeText.fontSize = 14;

                // More modern orange color for surge
                surgeText.color = new Color(1f, 0.56f, 0.13f, 1);

                surgeText.fontStyle = FontStyles.Bold;
                surgeText.alignment = TextAlignmentOptions.Top;

                // Add pulsing effect using a simple background
                GameObject surgeBg = CreateUIElement("SurgeBg", surgeObj.transform);
                surgeBg.transform.SetAsFirstSibling(); // Put background behind text
                Image surgeBgImage = surgeBg.AddComponent<Image>();
                surgeBgImage.color = new Color(1f, 0.56f, 0.13f, 0.07f);
                RectTransform surgeBgRect = surgeBg.GetComponent<RectTransform>();
                surgeBgRect.anchorMin = Vector2.zero;
                surgeBgRect.anchorMax = Vector2.one;
                surgeBgRect.sizeDelta = Vector2.zero;

                RectTransform surgeRect = surgeObj.GetComponent<RectTransform>();
                surgeRect.sizeDelta = new Vector2(0, 25);
            }

            private void CreateDivider(GameObject parent)
            {
                GameObject divider = CreateUIElement("Divider", parent.transform);
                Image dividerImage = divider.AddComponent<Image>();

                // Subtle divider that fits with modern UI
                dividerImage.color = new Color(1f, 1f, 1f, 0.1f);

                LayoutElement layoutElement = divider.AddComponent<LayoutElement>();
                layoutElement.minHeight = 1;
                layoutElement.flexibleHeight = 0;
            }

            private void CreatePassengerInfo(GameObject parent)
            {
                GameObject passengerInfoObj = CreateUIElement("PassengerInfo", parent.transform);
                passengerInfoText = passengerInfoObj.AddComponent<TextMeshProUGUI>();
                passengerInfoText.fontSize = 14;

                // Improved text color for better readability
                passengerInfoText.color = new Color(0.92f, 0.92f, 0.92f);

                passengerInfoText.alignment = TextAlignmentOptions.Left;
                passengerInfoText.enableWordWrapping = true;
                RectTransform passengerInfoRect = passengerInfoObj.GetComponent<RectTransform>();
                passengerInfoRect.sizeDelta = new Vector2(0, 50);
            }

            private void CreateStatusText(GameObject parent)
            {
                GameObject statusObj = CreateUIElement("StatusText", parent.transform);
                statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.fontSize = 14;
                statusText.color = Color.white;
                statusText.alignment = TextAlignmentOptions.Left;
                statusText.enableWordWrapping = true;
                LayoutElement layoutElement = statusObj.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = false;
                layoutElement.minHeight = 25;
                layoutElement.flexibleHeight = 0;
                RectTransform statusRect = statusObj.GetComponent<RectTransform>();
                statusRect.sizeDelta = new Vector2(0, 25);
            }

            private void CreateFareInfo(GameObject parent)
            {
                GameObject fareInfoObj = CreateUIElement("FareInfo", parent.transform);
                fareInfoText = fareInfoObj.AddComponent<TextMeshProUGUI>();
                fareInfoText.fontSize = 14;
                fareInfoText.fontStyle = FontStyles.Bold;

                // Brighter white for fare info to stand out
                fareInfoText.color = new Color(1f, 1f, 1f);

                fareInfoText.alignment = TextAlignmentOptions.Left;
                fareInfoText.enableWordWrapping = true;
            }

            private void CreateButtons(GameObject parent)
            {
                GameObject buttonContainer = CreateUIElement("ButtonContainer", parent.transform);
                RectTransform buttonContainerRect = buttonContainer.GetComponent<RectTransform>();
                buttonContainerRect.sizeDelta = new Vector2(0, 45);
                buttonContainerRect.anchoredPosition = new Vector2(175, -210);
                GridLayoutGroup buttonLayout = buttonContainer.AddComponent<GridLayoutGroup>();
                buttonLayout.cellSize = new Vector2(150, 40);
                buttonLayout.spacing = new Vector2(10, 0);
                buttonLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                buttonLayout.constraintCount = 2;
                buttonLayout.childAlignment = TextAnchor.MiddleCenter;
                CreateAcceptButton(buttonContainer);
                CreateDeclineButton(buttonContainer);
                CreateCancelButton(parent);
            }

            private void CreateAcceptButton(GameObject parent)
            {
                GameObject acceptButtonObj = CreateUIElement("AcceptButton", parent.transform);
                Image image = acceptButtonObj.AddComponent<Image>();
                Color normal = new Color(0.44f, 0.81f, 0.49f, 0.4f);
                Color hover = new Color(0.46f, 0.86f, 0.53f);

                // Apply rounded sprite
                image.sprite = buttonRoundedSprite;
                image.type = Image.Type.Sliced;
                image.color = normal;

                acceptButtonObj.AddComponent<Mask>().showMaskGraphic = true;

                acceptButton = acceptButtonObj.AddComponent<Button>();
                acceptButton.transition = Selectable.Transition.None;
                AddHoverEvents(acceptButtonObj, image, normal, hover);

                GameObject textObj = CreateUIElement("AcceptText", acceptButtonObj.transform);
                TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = "ACCEPT RIDE";
                text.fontSize = 16;
                text.fontStyle = FontStyles.Bold;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Center;
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                acceptButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnAcceptClicked?.Invoke()));
            }

            private void CreateDeclineButton(GameObject parent)
            {
                GameObject declineButtonObj = CreateUIElement("DeclineButton", parent.transform);
                Image image = declineButtonObj.AddComponent<Image>();
                Color normal = new Color(0.9f, 0.624f, 0.279f, 0.4f);
                Color hover = new Color(1f, 0.694f, 0.31f, 1f);

                // Apply rounded sprite
                image.sprite = buttonRoundedSprite;
                image.type = Image.Type.Sliced;
                image.color = normal;

                declineButtonObj.AddComponent<Mask>().showMaskGraphic = true;

                declineButton = declineButtonObj.AddComponent<Button>();
                declineButton.transition = Selectable.Transition.None;
                AddHoverEvents(declineButtonObj, image, normal, hover);

                GameObject textObj = CreateUIElement("DeclineText", declineButtonObj.transform);
                TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = "DECLINE";
                text.fontSize = 16;
                text.fontStyle = FontStyles.Bold;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Center;
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                declineButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnDeclineClicked?.Invoke()));
            }

            private void CreateCancelButton(GameObject parent)
            {
                GameObject cancelButtonObj = CreateUIElement("CancelButton", parent.transform);
                LayoutElement layout = cancelButtonObj.AddComponent<LayoutElement>();
                layout.preferredWidth = 340;
                layout.preferredHeight = 40;
                layout.minWidth = 340;
                layout.minHeight = 40;
                layout.flexibleWidth = 0;
                layout.flexibleHeight = 0;

                Image image = cancelButtonObj.AddComponent<Image>();
                Color normal = new Color(0.204f, 0.54f, 0.886f, 0.4f);
                Color hover = new Color(0.227f, 0.6f, 0.984f, 1f);

                // Apply rounded sprite
                image.sprite = buttonRoundedSprite;
                image.type = Image.Type.Sliced;
                image.color = normal;

                cancelButtonObj.AddComponent<Mask>().showMaskGraphic = true;

                cancelButton = cancelButtonObj.AddComponent<Button>();
                cancelButton.transition = Selectable.Transition.None;
                AddHoverEvents(cancelButtonObj, image, normal, hover);

                RectTransform rect = cancelButtonObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(340, 40);
                rect.anchoredPosition = new Vector2(175, -210);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);

                GameObject textObj = CreateUIElement("CancelText", cancelButtonObj.transform);
                TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = "CANCEL RIDE";
                text.fontSize = 16;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Center;
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                cancelButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnCancelClicked?.Invoke()));
            }

            private void AddHoverEvents(GameObject obj, Image image, Color normalColor, Color hoverColor)
            {
                EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
                EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enterEntry.callback = new EventTrigger.TriggerEvent();
                enterEntry.callback.AddListener((UnityAction<BaseEventData>)(data =>
                {
                    image.color = hoverColor;
                }));
                trigger.triggers.Add(enterEntry);

                EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exitEntry.callback = new EventTrigger.TriggerEvent();
                exitEntry.callback.AddListener((UnityAction<BaseEventData>)(data =>
                {
                    image.color = normalColor;
                }));
                trigger.triggers.Add(exitEntry);
            }

            private void SetupPanelTransitions()
            {
            }

            public void ShowPanel()
            {
                Vector2 targetPosition = new Vector2(50, -50);
                MelonCoroutines.Start(AnimatePanelPosition(targetPosition, 0.3f));
            }

            public void HidePanel()
            {
                Vector2 targetPosition = new Vector2(50, -500);
                MelonCoroutines.Start(AnimatePanelPosition(targetPosition, 0.3f));
            }

            private IEnumerator AnimatePanelPosition(Vector2 targetPosition, float duration)
            {
                if (uberPanelRect == null)
                {
                    MelonLogger.Error("Cannot animate panel - RectTransform is null");
                    yield break;
                }
                float elapsed = 0f;
                Vector2 startPosition = uberPanelRect.anchoredPosition;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float smoothT = Mathf.SmoothStep(0, 1, t);
                    uberPanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, smoothT);
                    yield return null;
                }
                uberPanelRect.anchoredPosition = targetPosition;
            }

            private GameObject CreateUIElement(string name, Transform parent)
            {
                GameObject element = new GameObject(name);
                element.AddComponent<RectTransform>();
                element.transform.SetParent(parent, false);
                return element;
            }

            private Sprite CreateRoundedRectangleSprite(int width, int height, int radius)
            {
                // Higher texture resolution for better quality
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                // Use multi-sampling (4x4) for better anti-aliasing
                const int samples = 4;
                const float sampleSize = 1f / samples;

                Color[] colors = new Color[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Multi-sampling: Take multiple samples per pixel
                        float totalAlpha = 0f;
                        for (int sy = 0; sy < samples; sy++)
                        {
                            for (int sx = 0; sx < samples; sx++)
                            {
                                float sampleX = x + (sx + 0.5f) * sampleSize;
                                float sampleY = y + (sy + 0.5f) * sampleSize;
                                totalAlpha += GetRoundedCornerAlpha((int)sampleX, (int)sampleY, width, height, radius);
                            }
                        }

                        // Average the samples
                        float alpha = totalAlpha / (samples * samples);
                        int index = y * width + x;
                        colors[index] = new Color(1f, 1f, 1f, alpha);
                    }
                }

                texture.SetPixels(colors);
                texture.Apply();

                // Use tight sprite mesh for better edge appearance
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, new Vector4(radius, radius, radius, radius));

                texture.wrapMode = TextureWrapMode.Clamp;
                texture.anisoLevel = 4;
                texture.filterMode = FilterMode.Bilinear;

                return sprite;
            }

            private float GetRoundedCornerAlpha(int x, int y, int width, int height, int radius)
            {
                // Center coordinates of each corner
                int topLeftX = radius;
                int topLeftY = radius;
                int topRightX = width - radius - 1;
                int topRightY = radius;
                int bottomLeftX = radius;
                int bottomLeftY = height - radius - 1;
                int bottomRightX = width - radius - 1;
                int bottomRightY = height - radius - 1;

                // Main body areas (always visible)
                if (x >= radius && x < width - radius) return 1f;
                if (y >= radius && y < height - radius) return 1f;

                // Find which corner we're in and calculate distance to corner center
                float dist;
                if (x < radius && y < radius)
                { // Top-left corner
                    dist = Mathf.Sqrt((x - topLeftX) * (x - topLeftX) + (y - topLeftY) * (y - topLeftY));
                }
                else if (x >= width - radius && y < radius)
                { // Top-right corner
                    dist = Mathf.Sqrt((x - topRightX) * (x - topRightX) + (y - topRightY) * (y - topRightY));
                }
                else if (x < radius && y >= height - radius)
                { // Bottom-left corner
                    dist = Mathf.Sqrt((x - bottomLeftX) * (x - bottomLeftX) + (y - bottomLeftY) * (y - bottomLeftY));
                }
                else
                { // Bottom-right corner
                    dist = Mathf.Sqrt((x - bottomRightX) * (x - bottomRightX) + (y - bottomRightY) * (y - bottomRightY));
                }

                // Improved super-sampling anti-aliasing
                float edge = radius - 0.5f;

                // Completely inside the corner radius
                if (dist <= edge - 1.5f) return 1f;

                // Completely outside the corner radius
                if (dist >= edge + 1.5f) return 0f;

                // Smooth transition over several pixels for better anti-aliasing
                return Mathf.SmoothStep(1f, 0f, (dist - (edge - 1.5f)) / 3f);
            }

            private static string FormatDistance(float meters)
            {
                return meters >= 1000f ? $"{(meters / 1000f):0.0}km" : $"{meters:F0}m";
            }

            public void DestroyUI()
            {
                if (uberUICanvas != null)
                {
                    GameObject.Destroy(uberUICanvas);
                    uberUICanvas = null;
                    uiInitialized = false;
                }
            }
        }
    }
}