using ChartAndGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartAndGraph
{
    [Serializable]
    public class ChartMainDivisionInfo : ChartDivisionInfo
    {
        public DivisionMessure Messure
        {
            get { return spacingType; }
            set
            {
                spacingType = value;
                RaiseOnChanged();
            }
        }
        public DivisionMessure SpacingType
        {
            get { return spacingType; }
            set
            {
                spacingType = value;
                RaiseOnChanged();
            }
        }
        public float UnitsPerDivision
        {
            get { return unitsPerDivision; }
            set
            {
                unitsPerDivision = value;
                RaiseOnChanged();
            }
        }

    }
}
