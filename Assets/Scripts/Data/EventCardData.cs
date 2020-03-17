using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Event Card",menuName ="Cards/Event")]
public class EventCardData : ScriptableObject
{
    public string cardName;
    public string archetype;
    public string effect;
    public Sprite artwork;

    public IList<string> GetArchetypes() {
        return archetype.Split('|');
    }
}
