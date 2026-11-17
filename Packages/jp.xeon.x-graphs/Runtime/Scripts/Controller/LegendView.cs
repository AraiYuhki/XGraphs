using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.Controller
{
    public class LegendView : MonoBehaviour
    {
        [SerializeField]
        private Image colorSample;
        [SerializeField]
        private TMP_Text label;

        public void Setup(Legend legend)
        {
            colorSample.color = legend.Color;
            label.text = legend.Name;
        }
    }
}
