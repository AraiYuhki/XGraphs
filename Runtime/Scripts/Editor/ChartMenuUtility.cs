#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Xeon.XGraph.View;

namespace Xeon.XGraph.Editor
{
    internal static class ChartMenuUtility
    {
        internal const string MenuRoot = "GameObject/Create/UI/XGraphs/";
        internal const int MenuPriorityRoot = 10;

        private const float DefaultWidth = 400f;
        private const float DefaultHeight = 300f;

        internal static GameObject CreateChart<T>(string objectName, MenuCommand menuCommand) where T : ChartBase
        {
            var chartObject = CreateUIElementRoot(objectName, menuCommand);
            Undo.AddComponent<T>(chartObject);
            Selection.activeGameObject = chartObject;
            return chartObject;
        }

        private static GameObject CreateUIElementRoot(string name, MenuCommand menuCommand)
        {
            var parent = GetOrCreateParent(menuCommand);
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            StageUtility.PlaceGameObjectInCurrentStage(go);
            GameObjectUtility.SetParentAndAlign(go, parent);

            var uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                go.layer = uiLayer;
            }

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(DefaultWidth, DefaultHeight);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            return go;
        }

        private static GameObject GetOrCreateParent(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var currentStage = StageUtility.GetCurrentStageHandle();

            if (parent != null && StageUtility.GetStageHandle(parent) != currentStage)
            {
                parent = null;
            }

            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponentInParent<Canvas>() != null)
                {
                    var candidate = Selection.activeGameObject;
                    if (StageUtility.GetStageHandle(candidate) == currentStage)
                    {
                        parent = candidate;
                    }
                }
            }

            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            return parent;
        }

        private static GameObject GetOrCreateCanvasGameObject()
        {
            var canvas = FindCanvas();
            if (canvas != null && canvas.gameObject.activeInHierarchy)
            {
                return canvas.gameObject;
            }

            var canvasGameObject = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGameObject, "Create Canvas");
            canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();
            StageUtility.PlaceGameObjectInCurrentStage(canvasGameObject);

            var uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                canvasGameObject.layer = uiLayer;
            }

            EnsureEventSystem();

            return canvasGameObject;
        }

        private static void EnsureEventSystem()
        {
            if (FindEventSystem() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
        }

        private static Canvas FindCanvas()
        {
            var canvases = Object.FindObjectsOfType<Canvas>();
            var currentStage = StageUtility.GetCurrentStageHandle();
            foreach (var canvas in canvases)
            {
                if (canvas == null)
                {
                    continue;
                }

                if (!canvas.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (StageUtility.GetStageHandle(canvas.gameObject) == currentStage)
                {
                    return canvas;
                }
            }

            return null;
        }

        private static EventSystem FindEventSystem()
        {
            var eventSystems = Object.FindObjectsOfType<EventSystem>();
            var currentStage = StageUtility.GetCurrentStageHandle();
            foreach (var eventSystem in eventSystems)
            {
                if (eventSystem == null)
                {
                    continue;
                }

                if (!eventSystem.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (StageUtility.GetStageHandle(eventSystem.gameObject) == currentStage)
                {
                    return eventSystem;
                }
            }

            return null;
        }
    }
}
#endif
