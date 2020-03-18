using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Upgrade Card",menuName ="Cards/Upgrade")]
public class UpgradeCardData : ScriptableObject
{
    public string cardName;
    public string archetype;
    public string effect;
    public Sprite artwork;

    public IList<string> GetArchetypes() {
        return archetype.Split('|');
    }
}
