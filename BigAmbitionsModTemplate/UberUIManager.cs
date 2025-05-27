using System;
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

namespace RideshareSideJobMod
{
    namespace RideshareSideJobMod
    {
        public class RideshareUIManager
        {
            private List<GameObject> passengerEntryPool;
            private GameObject poolContainer;
            private int maxEntries = 3;
            public Action<RidesharePassenger> OnAcceptPassenger;
            private GameObject uberUICanvas;
            private GameObject uberPanel;
            private RectTransform uberPanelRect;
            public TMP_Text titleText;
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

            private Sprite roundedSprite;
            private Sprite buttonRoundedSprite;

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

                if (titleText != null) ApplyRubikFont(titleText);
                if (statusText != null) ApplyRubikFont(statusText);
                if (driverStatsText != null) ApplyRubikFont(driverStatsText);
                if (passengerInfoText != null) ApplyRubikFont(passengerInfoText);
                if (fareInfoText != null) ApplyRubikFont(fareInfoText);
                if (surgeText != null) ApplyRubikFont(surgeText);

                TMP_Text cancelText = cancelButton?.GetComponentInChildren<TMP_Text>();
                if (cancelText != null) ApplyRubikFont(cancelText);
            }

            public void InitializeUI()
            {
                if (uiInitialized && uberUICanvas != null)
                {
                    GameObject.Destroy(uberUICanvas);
                    uberUICanvas = null;
                    uiInitialized = false;
                }

                try
                {
                    InitializeFont();
                    if (uberUICanvas != null)
                    {
                        GameObject.Destroy(uberUICanvas);
                    }

                    CreateCanvas();
                    if (uberUICanvas == null) throw new Exception("CreateCanvas failed to set uberUICanvas");

                    roundedSprite = CreateRoundedRectangleSprite(256, 256, 8);
                    buttonRoundedSprite = CreateRoundedRectangleSprite(128, 128, 4);

                    CreateMainPanel();
                    CreateTitleBar();
                    GameObject contentPanel = CreateContentPanel();
                    if (contentPanel == null) throw new Exception("CreateContentPanel returned null");

                    CreateDriverStats(contentPanel);
                    CreateSurgePricing(contentPanel);
                    CreateDivider(contentPanel);
                    CreatePassengerInfo(contentPanel);
                    CreateStatusText(contentPanel);
                    CreateFareInfo(contentPanel);
                    CreateButtons(contentPanel);
                    CreatePoolContainer(contentPanel);
                    ApplyFontToAllTexts();

                    if (uberUICanvas == null) throw new Exception("uberUICanvas became null during initialization");

                    if (acceptButton == null) throw new Exception("acceptButton is null after CreateButtons");
                    if (cancelButton == null) throw new Exception("cancelButton is null after CreateButtons");

                    uberUICanvas.SetActive(false);
                    uiInitialized = true;
                }
                catch (Exception ex)
                {
                    uiInitialized = false;
                    uberUICanvas = null;
                    acceptButton = null;
                    cancelButton = null;
                }
            }

            public void SetUIVisible(bool visible)
            {
                if (uberUICanvas == null || !uiInitialized)
                {
                    DestroyUI();
                    InitializeUI();
                    if (uberUICanvas == null || !uiInitialized)
                    {
                        return;
                    }
                }
                uberUICanvas.SetActive(visible);
            }

            public void UpdateUI(DriverStats driverStats, RidesharePassenger currentPassenger, Queue<RidesharePassenger> passengerPool, bool isPeakHours, bool uberJobActive)
            {
                if (!uiInitialized || uberUICanvas == null || !uberUICanvas.activeSelf)
                {
                    MelonLogger.Msg("UpdateUI: Skipping because UI not initialized or canvas inactive");
                    return;
                }

                try
                {
                    if (!EnsureContentPanelActive())
                    {
                        MelonLogger.Error("UpdateUI: RideshareContentPanel unavailable, skipping update.");
                        return;
                    }
                    if (driverStatsText == null || driverStatsText.gameObject == null)
                    {
                        MelonLogger.Error("driverStatsText or its gameObject is null! Reinitializing UI...");
                        InitializeUI();
                        if (driverStatsText == null)
                        {
                            MelonLogger.Error("Failed to reinitialize driverStatsText. Disabling UI updates until next toggle.");
                            uiInitialized = false;
                            return;
                        }
                    }

                    driverStatsText.text =
                        $"Earnings: <color=yellow>${driverStats.totalEarnings:F2}</color> • " +
                        $"Trips: <color=yellow>{driverStats.totalRides}</color> • " +
                        $"Cancelled: <color=yellow>{driverStats.cancelledRides}</color>\n" +
                        $"Top Area: <color=yellow>{driverStats.mostVisitedNeighborhood}</color> • " +
                        $"Longest Ride: <color=yellow>{FormatDistance(driverStats.longestRide)}</color>\n" +
                        $"Rating: <color=yellow>{driverStats.averageRating:F1}</color> ★";

                    if (surgeText == null || surgeText.gameObject == null)
                    {
                        MelonLogger.Error("surgeText or its gameObject is null! Reinitializing UI...");
                        InitializeUI();
                        if (surgeText == null)
                        {
                            MelonLogger.Error("Failed to reinitialize surgeText. Disabling UI updates until next toggle.");
                            uiInitialized = false;
                            return;
                        }
                    }
                    surgeText.gameObject.SetActive(isPeakHours);

                    if (!uberJobActive && currentPassenger == null && passengerPool.Count == 0)
                    {
                        poolContainer.SetActive(false);
                        cancelButton.gameObject.SetActive(false);
                        passengerInfoText.gameObject.SetActive(true);
                        statusText.gameObject.SetActive(false);
                        fareInfoText.gameObject.SetActive(false);

                        string tipStatsInfo =
                            $"<color=white>No active passenger requests at the moment.</color>\n" +
                            $"<color=#8d8f8f>Press F3 to toggle Rideshare Job.</color>\n\n" +
                            $"<b><color=white>Tip Statistics:</color></b>\n" +
                            $"Total Tips: <color=yellow><b>${driverStats.totalTipsEarned:F2}</b></color>\n" +
                            $"Highest Tip: <color=yellow><b>${driverStats.highestTip:F2}</b></color>\n" +
                            $"Rides with Tips: <color=yellow><b>{driverStats.ridesWithTips}</b></color>/<color=white>{driverStats.totalRides}</color> " +
                            $"(<color=yellow>{(driverStats.totalRides > 0 ? (driverStats.ridesWithTips * 100f / driverStats.totalRides).ToString("F0") : "0")}%</color>)\n" +
                            $"Avg Tip: <color=yellow><b>{(driverStats.averageTipPercentage * 100):F0}%</b></color>";

                        if (passengerInfoText.text != tipStatsInfo)
                        {
                            passengerInfoText.text = tipStatsInfo;
                            statusText.text = "";
                            fareInfoText.text = "";
                            LayoutRebuilder.ForceRebuildLayoutImmediate(uberPanelRect);
                        }
                        return;
                    }

                    if (!uberJobActive && passengerPool.Count > 0)
                    {
                        poolContainer.SetActive(true);
                        acceptButton.gameObject.SetActive(false);
                        cancelButton.gameObject.SetActive(false);
                        UpdatePassengerPoolUI(passengerPool);

                        passengerInfoText.gameObject.SetActive(false);
                        statusText.gameObject.SetActive(false);
                        fareInfoText.gameObject.SetActive(false);
                    }
                    else
                    {
                        poolContainer.SetActive(false);

                        if (passengerInfoText == null || passengerInfoText.gameObject == null)
                        {
                            InitializeUI();
                            if (passengerInfoText == null)
                            {
                                uiInitialized = false;
                                return;
                            }
                        }
                        passengerInfoText.gameObject.SetActive(true);

                        if (statusText == null || statusText.gameObject == null)
                        {
                            InitializeUI();
                            if (statusText == null)
                            {
                                uiInitialized = false;
                                return;
                            }
                        }
                        statusText.gameObject.SetActive(true);

                        if (fareInfoText == null || fareInfoText.gameObject == null)
                        {
                            InitializeUI();
                            if (fareInfoText == null)
                            {
                                uiInitialized = false;
                                return;
                            }
                        }
                        fareInfoText.gameObject.SetActive(true);

                        bool shouldShowButtonPanel = (currentPassenger != null && !uberJobActive) || uberJobActive;
                        acceptButton.transform.parent.gameObject.SetActive(shouldShowButtonPanel);

                        acceptButton.gameObject.SetActive(currentPassenger != null && !uberJobActive);
                        cancelButton.gameObject.SetActive(uberJobActive);

                        if (!uberJobActive)
                        {
                            string newPassengerText = $"<b>Passenger:</b> {currentPassenger.passengerName}\n" +
                                                     $"<b>Pickup:</b> {currentPassenger.pickupAddress.DisplayName}, {currentPassenger.pickupAddress.neighborhood}\n" +
                                                     $"<b>Dropoff:</b> {currentPassenger.dropoffAddress.DisplayName}, {currentPassenger.dropoffAddress.neighborhood}";
                            if (passengerInfoText.text != newPassengerText)
                            {
                                passengerInfoText.text = newPassengerText;
                                statusText.text = "";
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
                                $"<color=yellow>Waiting for pickup ({(int)(currentPassenger.waitTime)}s)</color>";
                            string newFareText = currentPassenger.isPickedUp ?
                                $"Destination: {currentPassenger.dropoffAddress.DisplayName} <color=yellow>({currentPassenger.dropoffAddress.neighborhood})</color>\nFare: <color=yellow>${currentPassenger.fare:F2}</color>" :
                                $"Pickup: {currentPassenger.pickupAddress.DisplayName} " +
                                $"<color=yellow>({currentPassenger.pickupAddress.neighborhood})</color>\n" +
                                $"Fare: <color=yellow>${currentPassenger.fare:F2}</color>";

                            if (passengerInfoText.text != newPassengerText || statusText.text != newStatusText || fareInfoText.text != newFareText)
                            {
                                passengerInfoText.text = newPassengerText;
                                statusText.text = newStatusText;
                                fareInfoText.text = newFareText;
                                LayoutRebuilder.ForceRebuildLayoutImmediate(uberPanelRect);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error updating Rideshare UI: {ex.Message}\nStack: {ex.StackTrace}");
                }
            }

            private void UpdatePassengerPoolUI(Queue<RidesharePassenger> passengerPool)
            {
                foreach (var entry in passengerEntryPool)
                {
                    entry.SetActive(false);
                }

                for (int i = 0; i < maxEntries - 1; i++)
                {
                    var divider = poolContainer.transform.Find($"Divider_{i}")?.gameObject;
                    if (divider != null) divider.SetActive(false);
                }

                var passengersToShow = passengerPool.Take(maxEntries).ToList();
                for (int i = 0; i < passengerEntryPool.Count; i++)
                {
                    GameObject entry = passengerEntryPool[i];
                    if (i < passengersToShow.Count)
                    {
                        RidesharePassenger passenger = passengersToShow[i];
                        entry.SetActive(true);
                        entry.name = $"PassengerEntry_{passenger.passengerName}";

                        TextMeshProUGUI text = entry.transform.Find("PassengerText").GetComponent<TextMeshProUGUI>();
                        text.text = $"<color=yellow>{passenger.passengerName}</color>, " +
                                    $"Fare:  <b><color=yellow>${passenger.fare:F2}</color></b>\n" +
                                    $"from <b><color=white>{passenger.pickupAddress.DisplayName}</color></b>" +
                                    $"<color=yellow> ({passenger.pickupAddress.neighborhood})</color>\n" +
                                    $"to <b><color=white>{passenger.dropoffAddress.DisplayName}</color></b>" +
                                    $"<color=yellow> ({passenger.dropoffAddress.neighborhood})</color>\n";

                        Button button = entry.transform.Find("AcceptButton").GetComponent<Button>();
                        button.interactable = true;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                        {
                            OnAcceptPassenger?.Invoke(passenger);
                        }));

                        TextMeshProUGUI buttonText = button.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>();
                        buttonText.text = "Accept";

                        if (passengersToShow.Count == 1 && i == 0)
                        {
                            var divider = poolContainer.transform.Find("Divider_0")?.gameObject;
                            if (divider != null) divider.SetActive(true);
                        }
                        else if (passengersToShow.Count == 2 && i < 2)
                        {
                            var divider = poolContainer.transform.Find($"Divider_{i}")?.gameObject;
                            if (divider != null) divider.SetActive(true);
                        }
                        else if (passengersToShow.Count == 3 && i < 2)
                        {
                            var divider = poolContainer.transform.Find($"Divider_{i}")?.gameObject;
                            if (divider != null) divider.SetActive(true);
                        }
                    }
                    else
                    {
                        entry.SetActive(false);
                    }
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(poolContainer.GetComponent<RectTransform>());

                passengerInfoText.gameObject.SetActive(false);
                statusText.gameObject.SetActive(false);
                fareInfoText.gameObject.SetActive(false);
            }

            private void CreatePoolContainer(GameObject parent)
            {
                passengerEntryPool = new List<GameObject>();
                poolContainer = CreateUIElement("PassengerPoolContainer", parent.transform);
                RectTransform poolRect = poolContainer.GetComponent<RectTransform>();
                poolRect.anchorMin = new Vector2(0, 0);
                poolRect.anchorMax = new Vector2(1, 1);
                poolRect.offsetMin = new Vector2(15, 15);
                poolRect.offsetMax = new Vector2(-15, -100);
                VerticalLayoutGroup layout = poolContainer.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 5;
                layout.padding = new RectOffset(5, 5, 5, 5);
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;

                for (int i = 0; i < maxEntries; i++)
                {
                    GameObject entry = CreatePassengerEntry(null);
                    entry.transform.SetParent(poolContainer.transform, false);
                    entry.SetActive(false);
                    passengerEntryPool.Add(entry);

                    if (i < maxEntries - 1)
                    {
                        GameObject dividerObj = CreateUIElement($"Divider_{i}", poolContainer.transform);
                        Image dividerImage = dividerObj.AddComponent<Image>();
                        dividerImage.color = new Color(1f, 1f, 1f, 0.2f);

                        LayoutElement layoutElement = dividerObj.AddComponent<LayoutElement>();
                        layoutElement.minHeight = 1;
                        layoutElement.preferredHeight = 1;
                        layoutElement.flexibleHeight = 0;

                        dividerObj.SetActive(false);
                    }
                }

            }
            private GameObject CreatePassengerEntry(RidesharePassenger passenger)
            {
                GameObject entry = CreateUIElement("PassengerEntry", poolContainer.transform);
                RectTransform rt = entry.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 60);

                HorizontalLayoutGroup layout = entry.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.padding = new RectOffset(5, 5, 5, 5);
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = false;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;

                GameObject textObj = CreateUIElement("PassengerText", entry.transform);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 12;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Left;
                text.enableWordWrapping = true;
                text.text = passenger != null && passenger.dropoffAddress != null
                    ? $"{passenger.passengerName} to {passenger.dropoffAddress.DisplayName}"
                    : "No Passenger";
                if (rubikFont != null) ApplyRubikFont(text);

                RectTransform textRT = textObj.GetComponent<RectTransform>();
                textRT.sizeDelta = new Vector2(250, 50);

                LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
                textLayout.preferredWidth = 250;
                textLayout.flexibleWidth = 1;

                GameObject buttonObj = CreateUIElement("AcceptButton", entry.transform);
                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.sprite = buttonRoundedSprite;
                buttonImage.color = new Color(0.204f, 0.54f, 0.886f, 0.4f);
                buttonImage.raycastTarget = true;
                buttonImage.type = Image.Type.Sliced;

                Button button = buttonObj.AddComponent<Button>();
                button.targetGraphic = buttonImage;
                button.interactable = passenger != null;

                RectTransform buttonRT = buttonObj.GetComponent<RectTransform>();
                buttonRT.sizeDelta = new Vector2(80, 30);

                LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
                buttonLayout.preferredWidth = 80;
                buttonLayout.preferredHeight = 30;
                buttonLayout.flexibleWidth = 0;
                buttonLayout.flexibleHeight = 0;

                GameObject buttonTextObj = CreateUIElement("ButtonText", buttonObj.transform);
                TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "Accept";
                buttonText.fontSize = 14;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
                if (rubikFont != null) ApplyRubikFont(buttonText);

                RectTransform buttonTextRT = buttonTextObj.GetComponent<RectTransform>();
                buttonTextRT.anchorMin = Vector2.zero;
                buttonTextRT.anchorMax = Vector2.one;
                buttonTextRT.sizeDelta = Vector2.zero;

                Color normalColor = new Color(0.204f, 0.54f, 0.886f, 0.4f);
                Color hoverColor = new Color(0.227f, 0.6f, 0.984f, 1f);
                AddHoverEvents(buttonObj, buttonImage, normalColor, hoverColor);

                if (passenger != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                    {
                        OnAcceptPassenger?.Invoke(passenger);
                    }));
                }

                entry.name = passenger != null ? $"PassengerEntry_{passenger.passengerName}" : "PassengerEntry_Empty";

                LayoutElement layoutElement = entry.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 60;
                layoutElement.minHeight = 60;

                return entry;
            }
            private void CreateCanvas()
            {
                uberUICanvas = new GameObject("RideshareUICanvas");
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
                }
            }

            private void CreateMainPanel()
            {
                uberPanel = new GameObject("RidesharePanel");
                uberPanel.transform.SetParent(uberUICanvas.transform, false);
                Image panelImage = uberPanel.AddComponent<Image>();
                panelImage.sprite = roundedSprite;
                panelImage.type = Image.Type.Sliced;
                panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

                Shadow shadow = uberPanel.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.3f);
                shadow.effectDistance = new Vector2(2, -2);

                uberPanel.AddComponent<Mask>().showMaskGraphic = true;

                uberPanelRect = uberPanel.GetComponent<RectTransform>();
                uberPanelRect.anchorMin = new Vector2(0, 1);
                uberPanelRect.anchorMax = new Vector2(0, 1);
                uberPanelRect.pivot = new Vector2(0, 1);
                uberPanelRect.sizeDelta = new Vector2(390, 370);
                uberPanelRect.anchoredPosition = new Vector2(50, -660);

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

                Image titleBg = titleBar.AddComponent<Image>();
                titleBg.color = new Color(0f, 0f, 0f, 0.7906f);

                GameObject titleTextObj = CreateUIElement("TitleText", titleBar.transform);
                titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
                if (titleText == null)
                {
                }
                titleText.text = "Rideshare";
                titleText.fontSize = 14;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = Color.white;
                titleText.alignment = TextAlignmentOptions.Center;
                RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
                titleTextRect.anchorMin = Vector2.zero;
                titleTextRect.anchorMax = Vector2.one;
                titleTextRect.sizeDelta = Vector2.zero;

                GameObject uberStrip = CreateUIElement("RideshareColorStrip", titleBar.transform);
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
                GameObject contentPanel = CreateUIElement("RideshareContentPanel", uberPanel.transform);
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
                driverStatsText.fontWeight = FontWeight.Regular;
                driverStatsText.color = new Color(0.8f, 0.8f, 0.8f);
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

                passengerInfoText.alignment = TextAlignmentOptions.Right;
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
                statusText.alignment = TextAlignmentOptions.Right;
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

                fareInfoText.color = new Color(1f, 1f, 1f);

                fareInfoText.alignment = TextAlignmentOptions.Right;
                fareInfoText.enableWordWrapping = true;
            }

            private void CreateButtons(GameObject parent)
            {
                GameObject acceptButtonObj = CreateUIElement("AcceptButton", parent.transform);
                Image acceptImage = acceptButtonObj.AddComponent<Image>();
                acceptImage.sprite = buttonRoundedSprite;
                acceptImage.color = new Color(0.2f, 0.8f, 0.4f, 0.8f);
                acceptImage.raycastTarget = true;
                acceptImage.type = Image.Type.Sliced;

                acceptButton = acceptButtonObj.AddComponent<Button>();
                acceptButton.targetGraphic = acceptImage;
                acceptButton.interactable = true;

                RectTransform acceptRT = acceptButtonObj.GetComponent<RectTransform>();
                acceptRT.sizeDelta = new Vector2(100, 30);
                acceptRT.anchorMin = new Vector2(0.5f, 0);
                acceptRT.anchorMax = new Vector2(0.5f, 0);
                acceptRT.pivot = new Vector2(0.5f, 0.5f);
                acceptRT.anchoredPosition = new Vector2(0, 25);

                GameObject acceptTextObj = CreateUIElement("AcceptText", acceptButtonObj.transform);
                TextMeshProUGUI acceptText = acceptTextObj.AddComponent<TextMeshProUGUI>();
                acceptText.text = "Accept";
                acceptText.fontSize = 12;
                acceptText.color = Color.white;
                acceptText.alignment = TextAlignmentOptions.Center;
                acceptText.enableWordWrapping = false;
                if (rubikFont != null) ApplyRubikFont(acceptText);

                RectTransform acceptTextRT = acceptTextObj.GetComponent<RectTransform>();
                acceptTextRT.anchorMin = Vector2.zero;
                acceptTextRT.anchorMax = Vector2.one;
                acceptTextRT.sizeDelta = Vector2.zero;

                Color normalColor = new Color(0.2f, 0.8f, 0.4f, 0.8f);
                Color hoverColor = new Color(0.3f, 0.9f, 0.5f, 1f);
                AddHoverEvents(acceptButtonObj, acceptImage, normalColor, hoverColor);

                GameObject cancelButtonObj = CreateUIElement("CancelButton", parent.transform);
                LayoutElement layoutElement = cancelButtonObj.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 340;
                layoutElement.preferredHeight = 40;
                layoutElement.minWidth = 340;
                layoutElement.minHeight = 40;
                layoutElement.flexibleWidth = 0;
                layoutElement.flexibleHeight = 0;

                Image cancelImage = cancelButtonObj.AddComponent<Image>();
                normalColor = new Color(0.76f, 0.29f, 0.29f, 0.4f);
                hoverColor = new Color(0.76f, 0.29f, 0.29f, 0.7f);

                cancelImage.sprite = buttonRoundedSprite;
                cancelImage.type = Image.Type.Sliced;
                cancelImage.color = normalColor;

                cancelButtonObj.AddComponent<Mask>().showMaskGraphic = true;

                cancelButton = cancelButtonObj.AddComponent<Button>();
                cancelButton.transition = Selectable.Transition.None;
                cancelButton.interactable = true;

                AddHoverEvents(cancelButtonObj, cancelImage, normalColor, hoverColor);

                RectTransform cancelRT = cancelButtonObj.GetComponent<RectTransform>();
                cancelRT.sizeDelta = new Vector2(340, 40);
                cancelRT.anchoredPosition = new Vector2(195, -260);
                cancelRT.anchorMin = new Vector2(0, 1);
                cancelRT.anchorMax = new Vector2(0, 1);
                cancelRT.pivot = new Vector2(0.5f, 0.5f);

                GameObject cancelTextObj = CreateUIElement("CancelText", cancelButtonObj.transform);
                TextMeshProUGUI cancelText = cancelTextObj.AddComponent<TextMeshProUGUI>();
                cancelText.text = "CANCEL RIDE";
                cancelText.fontSize = 16;
                cancelText.color = Color.white;
                cancelText.alignment = TextAlignmentOptions.Center;
                if (rubikFont != null) ApplyRubikFont(cancelText);

                RectTransform cancelTextRT = cancelTextObj.GetComponent<RectTransform>();
                cancelTextRT.anchorMin = Vector2.zero;
                cancelTextRT.anchorMax = Vector2.one;
                cancelTextRT.sizeDelta = Vector2.zero;

                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnCancelClicked?.Invoke()));
                acceptButton.gameObject.SetActive(false);
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

            public bool EnsureContentPanelActive()
            {
                GameObject contentPanel = uberPanel?.transform.Find("RideshareContentPanel")?.gameObject;
                if (contentPanel == null)
                {
                    return false;
                }
                if (!contentPanel.activeSelf)
                {
                    contentPanel.SetActive(true);
                }
                return contentPanel.activeSelf;
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
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

                const int samples = 4;
                const float sampleSize = 1f / samples;

                Color[] colors = new Color[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
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

                        float alpha = totalAlpha / (samples * samples);
                        int index = y * width + x;
                        colors[index] = new Color(1f, 1f, 1f, alpha);
                    }
                }

                texture.SetPixels(colors);
                texture.Apply();

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, new Vector4(radius, radius, radius, radius));

                texture.wrapMode = TextureWrapMode.Clamp;
                texture.anisoLevel = 4;
                texture.filterMode = FilterMode.Bilinear;

                return sprite;
            }

            private float GetRoundedCornerAlpha(int x, int y, int width, int height, int radius)
            {
                int topLeftX = radius;
                int topLeftY = radius;
                int topRightX = width - radius - 1;
                int topRightY = radius;
                int bottomLeftX = radius;
                int bottomLeftY = height - radius - 1;
                int bottomRightX = width - radius - 1;
                int bottomRightY = height - radius - 1;

                if (x >= radius && x < width - radius) return 1f;
                if (y >= radius && y < height - radius) return 1f;

                float dist;
                if (x < radius && y < radius)
                {
                    dist = Mathf.Sqrt((x - topLeftX) * (x - topLeftX) + (y - topLeftY) * (y - topLeftY));
                }
                else if (x >= width - radius && y < radius)
                {
                    dist = Mathf.Sqrt((x - topRightX) * (x - topRightX) + (y - topRightY) * (y - topRightY));
                }
                else if (x < radius && y >= height - radius)
                {
                    dist = Mathf.Sqrt((x - bottomLeftX) * (x - bottomLeftX) + (y - bottomLeftY) * (y - bottomLeftY));
                }
                else
                {
                    dist = Mathf.Sqrt((x - bottomRightX) * (x - bottomRightX) + (y - bottomRightY) * (y - bottomRightY));
                }

                float edge = radius - 0.5f;
                if (dist <= edge - 1.5f) return 1f;
                if (dist >= edge + 1.5f) return 0f;

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