using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    [Serializable]
    public class BaseScrollableCategoryData
    {
        public string Name;
        public bool Enabled = true;
        [HideInInspector]
        public int ViewOrder = -1;
        [NonSerialized]
        public double? MaxX, MaxY, MinX, MinY, MaxRadius;
    }
}
