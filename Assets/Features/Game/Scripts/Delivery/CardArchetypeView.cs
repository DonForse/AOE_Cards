using System.Collections.Generic;
using System.Linq;
using Features.Game.Scripts.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Game.Scripts.Delivery
{
    public class CardArchetypeView : MonoBehaviour
    {
        [SerializeField] GameObject CardArchetypeIconGo;
        [SerializeField] string pathToIcons;
        public Sprite sprite;

        public void SetCard(IList<Archetype> archetypes)
        {
            SetArchetypes(archetypes);
        }

        private void SetArchetypes(IList<Archetype> archetypes)
        {
            foreach (var archetype in archetypes) {
                var go = Instantiate(CardArchetypeIconGo, this.transform);
                sprite = GetSprite(archetype);
                var image = go.GetComponentsInChildren<Image>().LastOrDefault();
                if (image != null)
                    image.sprite = sprite;
            }
        }

        private Sprite GetSprite(Archetype archetype)
        {
            return Resources.Load<Sprite>(string.Format("{0}/{1}", pathToIcons, archetype.ToString()));
        }
    }
}
