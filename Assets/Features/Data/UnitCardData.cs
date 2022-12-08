using UnityEngine;

namespace Features.Data
{
    [CreateAssetMenu(fileName ="Unit Card",menuName ="Cards/Unit")]
    public class UnitCardData : CardData
    {
        public string effect;
        public int effectPower;
        public int power;
        public Sprite artwork;
        public Sprite background;
    }
}
