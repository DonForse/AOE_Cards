using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Upgrade Card",menuName ="Cards/Upgrade")]
public class UpgradeCardData : CardData
{
    public string effect;
    public int powerEffect;
    public Sprite artwork;
}
