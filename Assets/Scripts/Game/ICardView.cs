using System.Collections;
using UnityEngine;

namespace Game
{
    internal interface ICardView
    {
        string CardName { get; }
        CardType CardType { get; }
        IEnumerator MoveToPoint(Vector3 newPosition, float duration);
        void FlipCard(bool show, float duration);
        void ShowCardBack();
    }
}