using static Emgu.CV.VideoCapture;

namespace iFRAT.Extensions;
public static class EXTENDED_CMT
{
    /// <summary>
    /// Optimized, Hard Coded CaptureModuleType ToString() method.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ToStringOp(this CaptureModuleType type)
    {
        if (type is CaptureModuleType.Camera)
            return "Camera";
        else if (type is CaptureModuleType.Highgui)
            return "Highgui";
        else
            return "Unknown";
    }
}