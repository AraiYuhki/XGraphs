using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Xeon.XGraph.Manager
{
    [Serializable]
    public class ScaleMarkerManager
    {
        public delegate float ValueToHeightDelegate(float value, out bool inRange);
        
        [SerializeField] private Transform parent;
        [SerializeField] private ScaleMarker markerPrefab;
        [SerializeField, HideInInspector] private List<ScaleMarker> markers = new();
        [SerializeField] private List<ScaleMarkerData> markerDataList = new();
        
        [SerializeField, Tooltip("マーカー間の最小距離（ピクセル）。この距離以下の場合、最小値・最大値を優先して中間値を非表示にします")]
        private float minMarkerDistance = 30f;
        
        [SerializeField, Tooltip("近接マーカーのフィルタリングを有効にする")]
        private bool enableProximityFiltering = true;

        private ValueToHeightDelegate valueToHeight;

        public void Initialize(ValueToHeightDelegate valueToHeight)
        {
            this.valueToHeight = valueToHeight;
        }

        public void SetMarkers(List<ScaleMarkerData> newData)
        {
            if (ReferenceEquals(markerDataList, newData))
                return;
            
            markerDataList = newData;
            RefreshMarkers();
        }
        
        public void AddMarker(string label, float value)
        {
            markerDataList.Add(new ScaleMarkerData(label, value));
            RefreshMarkers();
        }

        public void RemoveMarker(string label, float value)
        {
            var target = markerDataList.FirstOrDefault(data => data.Label == label && Mathf.Approximately(data.Value, value));
            if (target != null)
                markerDataList.Remove(target);
            RefreshMarkers();
        }

        public void RemoveMarker(int index)
        {
            markerDataList.RemoveAt(index);
            RefreshMarkers();
        }
        
        public void UpdateMarkers()
        {
            // 位置を更新
            foreach (var marker in markers)
            {
                if (marker == null || marker.Data == null)
                    continue;
                var position = marker.transform.localPosition;
                position.y = valueToHeight(marker.Data.Value, out var inRange);
                marker.transform.localPosition = position;
                marker.gameObject.SetActive(inRange);
            }
            
            // 近接フィルタリングを適用
            if (enableProximityFiltering)
            {
                ApplyProximityFiltering();
            }
        }

        public void ClearMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker == null)
                    continue;
                if (Application.isPlaying)
                    GameObject.Destroy(marker.gameObject);
                else
                    GameObject.DestroyImmediate(marker.gameObject);
            }

            markers.Clear();
        }

        public void RefreshMarkers()
        {
            ClearMarkers();
            if (markerPrefab == null)
                return;

            bool hasDelegate = valueToHeight != null;

            foreach (var data in markerDataList)
            {
                var marker = GameObject.Instantiate(markerPrefab, parent);
                marker.Data = data;

                var position = marker.transform.localPosition;
                bool inRange = true;
                if (hasDelegate)
                {
                    position.y = valueToHeight(data.Value, out inRange);
                }
                else
                {
                    position.y = 0f;
                    inRange = true;
                }
                marker.transform.localPosition = position;
                marker.gameObject.SetActive(inRange);
                markers.Add(marker);
            }
            
            // 近接フィルタリングを適用
            if (enableProximityFiltering && hasDelegate)
            {
                ApplyProximityFiltering();
            }
        }
        
        /// <summary>
        /// 近接マーカーのフィルタリングを適用
        /// 最小値・最大値を優先し、中間値は距離が十分ある場合のみ表示
        /// </summary>
        private void ApplyProximityFiltering()
        {
            if (markers.Count <= 2)
                return; // 2個以下ならフィルタリング不要
            
            // アクティブなマーカーのみを対象にし、値でソート
            var activeMarkers = markers
                .Where(m => m != null && m.gameObject.activeSelf)
                .OrderBy(m => m.Data.Value)
                .ToList();
            
            if (activeMarkers.Count <= 2)
                return;
            
            // 最小値と最大値のインデックス
            int minIndex = 0;
            int maxIndex = activeMarkers.Count - 1;
            
            // 中間のマーカーをチェック
            for (int i = 1; i < activeMarkers.Count - 1; i++)
            {
                var currentMarker = activeMarkers[i];
                float currentY = currentMarker.transform.localPosition.y;
                
                // 前のアクティブなマーカーを探す
                float prevY = float.NegativeInfinity;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (activeMarkers[j].gameObject.activeSelf)
                    {
                        prevY = activeMarkers[j].transform.localPosition.y;
                        break;
                    }
                }
                
                // 次のアクティブなマーカーを探す
                float nextY = float.PositiveInfinity;
                for (int j = i + 1; j < activeMarkers.Count; j++)
                {
                    if (activeMarkers[j].gameObject.activeSelf)
                    {
                        nextY = activeMarkers[j].transform.localPosition.y;
                        break;
                    }
                }
                
                // 前後のマーカーとの距離をチェック
                float distToPrev = Mathf.Abs(currentY - prevY);
                float distToNext = Mathf.Abs(nextY - currentY);
                
                // どちらかの距離が最小距離以下なら非表示
                if (distToPrev < minMarkerDistance || distToNext < minMarkerDistance)
                {
                    currentMarker.gameObject.SetActive(false);
                }
            }
        }
        
#if UNITY_EDITOR
        [UnityEditor.CustomPropertyDrawer(typeof(ScaleMarkerManager))]
        private class MarkerManagerEditor : UnityEditor.PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                // 折りたたみ時は1行のみ
                if (!property.isExpanded)
                    return EditorGUIUtility.singleLineHeight;

                var parentProp = property.FindPropertyRelative("parent");
                var prefabProp = property.FindPropertyRelative("markerPrefab");
                var listProp = property.FindPropertyRelative("markerDataList");
                var enableFilteringProp = property.FindPropertyRelative("enableProximityFiltering");
                var minDistanceProp = property.FindPropertyRelative("minMarkerDistance");

                float h = 0f;
                h += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Foldoutヘッダ
                h += EditorGUI.GetPropertyHeight(parentProp) + EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUI.GetPropertyHeight(prefabProp) + EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Header
                h += EditorGUI.GetPropertyHeight(enableFilteringProp) + EditorGUIUtility.standardVerticalSpacing;
                if (enableFilteringProp.boolValue)
                {
                    h += EditorGUI.GetPropertyHeight(minDistanceProp) + EditorGUIUtility.standardVerticalSpacing;
                }
                h += EditorGUI.GetPropertyHeight(listProp) + EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUIUtility.singleLineHeight; // buttons
                return h;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                float line = EditorGUIUtility.singleLineHeight;
                float pad = EditorGUIUtility.standardVerticalSpacing;

                // Foldoutヘッダ
                Rect headerRect = new Rect(position.x, position.y, position.width, line);
                property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

                if (!property.isExpanded)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                float y = headerRect.y + line + pad;

                var parentProp = property.FindPropertyRelative("parent");
                var prefabProp = property.FindPropertyRelative("markerPrefab");
                var listProp = property.FindPropertyRelative("markerDataList");
                var enableFilteringProp = property.FindPropertyRelative("enableProximityFiltering");
                var minDistanceProp = property.FindPropertyRelative("minMarkerDistance");

                // parent
                Rect rParent = new Rect(position.x, y, position.width, line);
                EditorGUI.PropertyField(rParent, parentProp);
                y += line + pad;

                // prefab
                Rect rPrefab = new Rect(position.x, y, position.width, line);
                EditorGUI.PropertyField(rPrefab, prefabProp);
                y += line + pad;

                // Header for proximity filtering
                Rect rHeader = new Rect(position.x, y, position.width, line);
                EditorGUI.LabelField(rHeader, "マーカー重複制御", EditorStyles.boldLabel);
                y += line + pad;

                // Enable proximity filtering
                Rect rEnableFiltering = new Rect(position.x, y, position.width, line);
                EditorGUI.PropertyField(rEnableFiltering, enableFilteringProp);
                y += line + pad;

                // Min distance (only if filtering is enabled)
                if (enableFilteringProp.boolValue)
                {
                    Rect rMinDistance = new Rect(position.x, y, position.width, line);
                    EditorGUI.PropertyField(rMinDistance, minDistanceProp);
                    y += line + pad;
                }

                // marker data list
                float listHeight = EditorGUI.GetPropertyHeight(listProp);
                Rect rList = new Rect(position.x, y, position.width, listHeight);
                EditorGUI.PropertyField(rList, listProp, true);
                y += listHeight + pad;

                // buttons
                Rect rButtons = new Rect(position.x, y, position.width, line);
                float half = (rButtons.width - 4f) * 0.5f;
                Rect rCreate = new Rect(rButtons.x, rButtons.y, half, line);
                Rect rClear = new Rect(rButtons.x + half + 4f, rButtons.y, half, line);

                var owner = property.serializedObject.targetObject;
                var markerManager = fieldInfo.GetValue(owner) as ScaleMarkerManager;

                using (new EditorGUI.DisabledScope(markerManager == null))
                {
                    if (GUI.Button(rCreate, "マーカー生成 / 更新"))
                    {
                        markerManager?.RefreshMarkers();
                        EditorUtility.SetDirty(owner);
                    }
                    if (GUI.Button(rClear, "全マーカー削除"))
                    {
                        markerManager?.ClearMarkers();
                        EditorUtility.SetDirty(owner);
                    }
                }

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}
