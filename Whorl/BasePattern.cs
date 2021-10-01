using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whorl
{
    public abstract class BasePattern: WhorlBaseObject
    {
        private PointF _center = new PointF(0, 0);  //(float.MinValue, float.MinValue);
        //public PointF? PrevCenter { get; set; } = null;
        public PointF Center
        {
            get { return _center; }
            set
            {
                _center = value;
                //if (_center != value)
                //{
                //    _center = value;
                //    OnCenterChanged();
                //}
                //PrevCenter = _center;
            }
        }
        private static long currentPatternID = 0;
        public long PatternID { get; }  //Unique ID for patterns in current session.
        public long SharedPatternID { get; set; }  //ID for pattern or its copies.

        public BasePattern()
        {
            this.PatternID = ++currentPatternID;
            this.SharedPatternID = this.PatternID;  //Overwritten by CopyProperties if this pattern is a copy.
            //this.Center = new PointF(0, 0);
        }
        public abstract void DrawOutline(Graphics g, Color? color = null);
        //protected virtual void OnCenterChanged() { }
    }
}
