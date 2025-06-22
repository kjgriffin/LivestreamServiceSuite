using Concord;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSixGUI
{
    internal class GravePlot
    {
        public string ServiceName { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceTime { get; set; }
        public string DeceasedName { get; set; }
        public string Lifespan { get; set; }
        public HymnDef[] Hymns { get; set; } = new HymnDef[4];
        public string ServicePath { get; set; } = "...";
        public ReadingDef[] Readings { get; set; } = new ReadingDef[4];
        public string Translation { get; set; } = "NIV";

        public static GravePlot Default
        {
            get
            {
                return new GravePlot()
                {
                    ServiceName = "Christian Funeral Service",
                    ServiceDate = DateTime.Now,
                    ServiceTime = "11:00 am",
                    DeceasedName = "NAME",
                    Lifespan = "BORN mmm DD YYYY -- DIED mmm DD YYY",
                    Hymns =
                    [
                        HymnDef.Default(1),
                        HymnDef.Default(2),
                        HymnDef.Default(3),
                        HymnDef.Default(4),
                    ],
                    Readings =
                    [
                        ReadingDef.Default(1),
                        ReadingDef.Default(2),
                        ReadingDef.Default(3),
                        ReadingDef.Default(4),
                    ],
                    Translation = "NIV",
                };
            }
        }
    }

    internal class HymnDef
    {
        public int ID { get; set; }
        public bool Use { get; set; }
        public string Number { get; set; }

        public static HymnDef Default(int id)
        {
            return new HymnDef { ID = id, Number = "###", Use = false };
        }
    }

    internal class ReadingDef
    {
        public int ID { get; set; }
        public bool Use { get; set; }
        public string Reference { get; set; }
        public static ReadingDef Default(int id)
        {
            return new ReadingDef { ID = id, Reference = "ref.", Use = false };
        }
    }

    internal class GraveDigger
    {
        public GravePlot Config { get; set; }




    }
}
