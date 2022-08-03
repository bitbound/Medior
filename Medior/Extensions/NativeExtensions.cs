using PInvoke;

namespace Medior.Extensions
{
    internal static class NativeExtensions
    {
        internal static int Width(this RECT rect)
        {
            return rect.right - rect.left;
        }

        internal static int Height(this RECT rect)
        {
            return rect.bottom - rect.top;
        }

        internal static bool IsEmpty(this RECT rect)
        {
            return rect.Width() == 0 && rect.Height() == 0;
        }

        internal static bool IsOver(this POINT point, RECT rect)
        {
            return rect.left < point.x && rect.top < point.y && rect.right > point.x && rect.bottom > point.y;
        }
    }
}
