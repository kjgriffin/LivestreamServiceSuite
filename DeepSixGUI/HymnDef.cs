namespace DeepSixGUI
{
    public class HymnDef
    {
        public int ID { get; set; }
        public bool Use { get; set; }
        public string Number { get; set; }

        public static HymnDef Default(int id)
        {
            return new HymnDef { ID = id, Number = "###", Use = false };
        }
    }
}
