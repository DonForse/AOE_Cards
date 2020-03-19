using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Unit Card",menuName ="Cards/Unit")]
public class UnitCardData : ScriptableObject
{
    public string cardName;
    public string archetype;
    public string effect;
    public int effectPower;
    public int power;
    public Sprite artwork;

    public IList<string> GetArchetypes() {
        return archetype.Split('|');
    }
}
