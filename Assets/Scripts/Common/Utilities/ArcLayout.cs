using UnityEngine;
using UnityEngine.UI;

namespace Common.Utilities
{
    [ExecuteAlways]
    public class ArcLayout : LayoutGroup
    {
        [SerializeField] private Vector2 pivot;
        [SerializeField] private float angle;
        //[SerializeField] private float arc;
        //[SerializeField] private bool automaticAngle;
        [SerializeField] private float spreadWidth;

        protected ArcLayout()
        { }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
            var halfAngle = angle / 2;
            var angleDif = angle / (rectChildren.Count - 1);
            var halfChilds = (rectChildren.Count - 1) / 2f;
            if (rectChildren.Count == 1)
            {
                rectChildren[0].position = new Vector3(pivot.x, this.rectTransform.position.y, this.rectTransform.position.z);
                rectChildren[0].eulerAngles = new Vector3(0f, 0f, 0f);
            }
            else
            {
                for (int i = 0; i < rectChildren.Count; i++)
                {
                    rectChildren[i].position = new Vector3(((halfChilds - i) * spreadWidth) + pivot.x, this.rectTransform.position.y, this.rectTransform.position.z);
                    rectChildren[i].eulerAngles = new Vector3(0f, 0f, -halfAngle + (angleDif * i));

                }
            }
        }

        public override void SetLayoutVertical()
        {
            var halfChilds = (rectChildren.Count - 1) / 2f;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                float height = -3 * Mathf.Pow((halfChilds - i) / rectChildren.Count, 2); //height calculation formula. Try to find something that makes more sense...
                rectChildren[i].position = new Vector3(rectChildren[i].position.x, this.transform.position.y + pivot.y + height, rectChildren[i].position.z);
            }
        }
    }
}