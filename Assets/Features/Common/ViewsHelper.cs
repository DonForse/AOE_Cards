using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public static class ViewsHelper
    {
        public static void RefreshView(RectTransform container)
        {
            LayoutRebuilder.MarkLayoutForRebuild(container);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            Canvas.ForceUpdateCanvases();
        }

        public static bool IsOverlapped(RectTransform area, Vector3 position)
        {
            var corners = new Vector3[4];
            area.GetWorldCorners(corners);

            if (position.x - corners[0].x < 0 || position.y - corners[0].y < 0)
                return false;
            if (position.x - corners[1].x < 0 || position.y - corners[1].y > 0)
                return false;
            if (position.x - corners[2].x > 0 || position.y - corners[2].y > 0)
                return false;
            if (position.x - corners[3].x > 0 || position.y - corners[3].y < 0)
                return false;
            return true;
        }
    }
}
