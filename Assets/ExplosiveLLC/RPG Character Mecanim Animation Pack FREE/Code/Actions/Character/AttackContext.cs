using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Actions
{
    public class AttackContext
    {
        public string type;
        public Side Side;
        public int number;
        public AttackLevel level;

        public AttackContext(string type, Side side, int number = -1, AttackLevel level = AttackLevel.Light)
        {
            this.type = type;
            Side = side;
            this.number = number;
            this.level = level;
        }
    }
}