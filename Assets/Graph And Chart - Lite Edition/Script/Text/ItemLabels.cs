using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    public class ItemLabels : AlignedItemLabels
    {
        /// <summary>
        /// the number of fraction digits in the text labels
        /// </summary>
        [Range(0, 7)]
        [SerializeField]
        [Category(InspectorCategories.Labels, 3)]
        private int fractionDigits = 2;

        /// <summary>
        /// the number of fraction digits in the text labels
        /// </summary>
        public int FractionDigits
        {
            get { return fractionDigits; }
            set { fractionDigits = value; RaiseOnUpdate(); }
        }
        protected override Action<IInternalUse, bool> Assign
        {
            get
            {
                return (x, clear) =>
                {
                    if (clear)
                    {
                        if (x.ItemLabels == this)
                            x.ItemLabels = null;
                    }
                    else
                    {
                        if (x.ItemLabels != this)
                            x.ItemLabels = this;
                    }
                };
            }
        }
    }
}
