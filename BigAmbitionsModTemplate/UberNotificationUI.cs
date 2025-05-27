using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using Il2CppUI.Notification;

namespace RideshareSideJobMod
{
    public enum NotificationType
    {
        Info,
        Success,
        Error,
        Warning
    }

    public class RideshareNotificationUI
    {
        private static GameObject notificationCanvas;
        private static Transform notificationContainer;
        private static List<GameObject> activeNotifications = new List<GameObject>();
        private static bool isInitialized = false;
        private static bool isInitializing = false;

        private static readonly Color INFO_COLOR = new Color(0.2f, 0.6f, 1f);
        private static readonly Color SUCCESS_COLOR = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color ERROR_COLOR = new Color(0.9f, 0.2f, 0.2f);
        private static readonly Color WARNING_COLOR = new Color(1f, 0.8f, 0.2f);
        private static readonly Color BG_COLOR = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        public bool Initialize()
        {
            isInitializing = true;

            try
            {
                MelonCoroutines.Start(InitializeWhenReady());
                return true;
            }
            catch (Exception)
            {
                isInitializing = false;
                return false;
            }
        }

        private IEnumerator InitializeWhenReady()
        {
            yield return null;
            yield return null;

            bool success = false;

            try
            {
                if (notificationCanvas == null)
                {
                    notificationCanvas = new GameObject("RideshareSideJobNotificationCanvas");
                    GameObject.DontDestroyOnLoad(notificationCanvas);
                }
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            try
            {
                Canvas canvas = notificationCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            try
            {
                CanvasScaler scaler = notificationCanvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            try
            {
                notificationCanvas.AddComponent<GraphicRaycaster>();
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            GameObject container = null;
            try
            {
                container = new GameObject("RideshareSideJobNotificationContainer");
                container.transform.SetParent(notificationCanvas.transform, false);
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            try
            {
                RectTransform containerRT = container.AddComponent<RectTransform>();
                containerRT.anchorMin = new Vector2(0.5f, 1);
                containerRT.anchorMax = new Vector2(0.5f, 1);
                containerRT.pivot = new Vector2(0.5f, 1);
                containerRT.anchoredPosition = new Vector2(0, -85);
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            try
            {
                VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.spacing = 15;
                layout.childControlHeight = false;
                layout.childControlWidth = false;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = false;
                layout.padding = new RectOffset(10, 10, 10, 10);
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            yield return null;

            try
            {
                ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                notificationContainer = container.transform;
                success = true;

                RectTransform containerRT = container.GetComponent<RectTransform>();
            }
            catch (Exception)
            {
                isInitializing = false;
                yield break;
            }

            isInitialized = success;
            isInitializing = false;

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
        }

        private Il2CppUI.Notification.NotificationType MapToGameNotificationType(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Info:
                    return Il2CppUI.Notification.NotificationType.Info; // 3
                case NotificationType.Success:
                    return Il2CppUI.Notification.NotificationType.Success; // 0
                case NotificationType.Error:
                    return Il2CppUI.Notification.NotificationType.Error; // 2
                case NotificationType.Warning:
                    return Il2CppUI.Notification.NotificationType.Warning; // 1
                default:
                    return Il2CppUI.Notification.NotificationType.Info; // 3
            }
        }

        public void ShowNotification(string message, NotificationType type, float duration)
        {
            if (!isInitialized && !isInitializing)
            {
                bool initStarted = Initialize();
                if (!initStarted)
                {
                    return;
                }
            }

            try
            {
                Il2CppUI.Notification.NotificationType gameNotificationType = MapToGameNotificationType(type);
                NotificationsUI.PlayNotificationSound(gameNotificationType);
            }
            catch (Exception)
            {
                // Silently fail
            }

            MelonCoroutines.Start(DisplayNotification(message, type, duration));
        }

        private List<(GameObject notification, Text text, RectTransform notifRT)> activeNotificationsWithText = new List<(GameObject, Text, RectTransform)>();

        private IEnumerator DisplayNotification(string message, NotificationType type, float duration)
        {
            while (isInitializing)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (!isInitialized)
            {
                bool initStarted = Initialize();
                if (!initStarted)
                {
                    yield break;
                }

                while (isInitializing)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                if (!isInitialized)
                {
                    yield break;
                }
            }

            GameObject notification = new GameObject("RideshareSideJobNotification");
            notification.transform.SetParent(notificationContainer, false);
            activeNotifications.Add(notification);

            RectTransform notifRT = notification.AddComponent<RectTransform>();
            notifRT.sizeDelta = new Vector2(0, 0);

            Image bg = notification.AddComponent<Image>();
            bg.color = BG_COLOR;
            bg.type = Image.Type.Sliced;

            Shadow shadow = notification.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);

            GameObject indicator = new GameObject("TypeIndicator");
            indicator.transform.SetParent(notification.transform, false);

            RectTransform indicatorRT = indicator.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0, 0);
            indicatorRT.anchorMax = new Vector2(0, 1);
            indicatorRT.pivot = new Vector2(0, 0.5f);
            indicatorRT.sizeDelta = new Vector2(6, 0);
            indicatorRT.anchoredPosition = new Vector2(0, 0);

            Image indicatorImg = indicator.AddComponent<Image>();
            switch (type)
            {
                case NotificationType.Success:
                    indicatorImg.color = SUCCESS_COLOR;
                    break;
                case NotificationType.Error:
                    indicatorImg.color = ERROR_COLOR;
                    break;
                case NotificationType.Warning:
                    indicatorImg.color = WARNING_COLOR;
                    break;
                default:
                    indicatorImg.color = INFO_COLOR;
                    break;
            }

            LayoutElement indicatorLayout = indicator.AddComponent<LayoutElement>();
            indicatorLayout.ignoreLayout = true;

            VerticalLayoutGroup notifLayout = notification.AddComponent<VerticalLayoutGroup>();
            notifLayout.padding = new RectOffset(32, 20, 15, 15);
            notifLayout.spacing = 8;
            notifLayout.childAlignment = TextAnchor.UpperLeft;
            notifLayout.childControlHeight = false;
            notifLayout.childControlWidth = false;
            notifLayout.childForceExpandHeight = false;
            notifLayout.childForceExpandWidth = false;

            GameObject textObj = new GameObject("NotificationText");
            textObj.transform.SetParent(notification.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0.5f);
            textRT.anchorMax = new Vector2(0, 0.5f);
            textRT.pivot = new Vector2(0, 0.5f);
            textRT.anchoredPosition = new Vector2(0, 0);
            textRT.sizeDelta = new Vector2(0, 0);

            Text text = textObj.AddComponent<Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.supportRichText = true;
            text.resizeTextForBestFit = false;

            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            ContentSizeFitter textFitter = textObj.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ContentSizeFitter notifFitter = notification.AddComponent<ContentSizeFitter>();
            notifFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            notifFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            activeNotificationsWithText.Add((notification, text, notifRT));

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(notifRT);

            CanvasGroup canvasGroup = notification.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            float fadeTime = 0.3f;
            float elapsedTime = 0;

            while (elapsedTime < fadeTime)
            {
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 1;

            yield return new WaitForSeconds(duration - fadeTime);

            elapsedTime = 0;
            while (elapsedTime < fadeTime)
            {
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            activeNotificationsWithText.RemoveAll(n => n.notification == notification);
            CleanupNotification(notification);
        }

        private void CleanupNotification(GameObject notification)
        {
            if (notification != null)
            {
                activeNotifications.Remove(notification);
                GameObject.Destroy(notification);
            }
        }
    }
}