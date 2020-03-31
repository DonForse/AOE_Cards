using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class UpgradeIconView : MonoBehaviour
    {
        [SerializeField] Image image;
        public void SetImage(Sprite upgradeViewImage)
        {
            image.sprite = upgradeViewImage;
        }
    }
}