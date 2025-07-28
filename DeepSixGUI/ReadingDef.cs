namespace DeepSixGUI
{
    public class ReadingDef
    {
        public int ID { get; set; }
        public bool Use { get; set; }
        public string Reference { get; set; }
        public static ReadingDef Default(int id)
        {
            return new ReadingDef { ID = id, Reference = "ref.", Use = false };
        }
    }
}
