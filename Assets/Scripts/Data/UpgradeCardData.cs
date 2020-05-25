﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Upgrade Card",menuName ="Cards/Upgrade")]
public class UpgradeCardData : CardData
{
    public string effect;
    public string powerEffect;
    public Sprite artwork;
    public AudioClip revealClip;
}
