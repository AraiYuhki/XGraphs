using System.Collections.Generic;
using UnityEngine;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.Controller
{
    public class LegendListView : MonoBehaviour
    {
        [SerializeField]
        private LegendView prefab;
        [SerializeField]
        private Transform container;

        [SerializeField, HideInInspector]
        private List<LegendView> legends = new();

        public void Setup(IEnumerable<Legend> legends)
        {
            foreach (var legend in this.legends)
            {
                if (Application.isPlaying)
                    Destroy(legend.gameObject);
                else
                    DestroyImmediate(legend.gameObject);
            }
            this.legends.Clear();
            foreach (var legend in legends)
            {
                var view = Instantiate(prefab, container);
                view.Setup(legend);
                this.legends.Add(view);
            }
        }
    }
}
