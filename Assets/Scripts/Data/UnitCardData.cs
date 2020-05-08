using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Unit Card",menuName ="Cards/Unit")]
public class UnitCardData : CardData
{
    public string effect;
    public int effectPower;
    public int power;
    public Sprite artwork;
    public Sprite background;

}
