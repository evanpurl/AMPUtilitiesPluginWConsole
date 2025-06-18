using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AMPUtilitiesPurlsWay.Utils
{
    [XmlRoot("ModList")]
    public class Config
    {
        [XmlArray("Mods")]
        [XmlArrayItem("Mod")]
        public List<string> Mods { get; set; } = new();

        [XmlArray("Service")]
        [XmlArrayItem("ServiceName")]
        public List<string> Service { get; set; } = new();
    }

}
