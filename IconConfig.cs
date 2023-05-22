using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconGenerator
{
    public class IconConfig
    {
        public float TextSize { get; set; } = 8;

        public string TextColor { get; set; } = "#ffffff";

        public string FontFamily { get; set; } = "Microsoft YaHei";

        public string VeteranIcon { get; set; } = "";

        public List<IconObject> Template { get; set; } = new List<IconObject>();

        public OutPutType OutPutType { get; set; }
    }

    public class IconObject
    {
        public IconObjectType Type { get; set; }

        public float X { get; set; } = 0;

        public float Y { get; set; } = 0;

        public string Src { get; set; }

        public string TextColor { get; set; } 

    }

    public enum IconObjectType
    {
        Image,
        Text,
        Icon,
        Veteran
    }

    public enum OutPutType
    {
        png,
        pcx
    }

}
