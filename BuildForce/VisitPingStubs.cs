#if !ANDROID
namespace BuildForce;

// [VISPING1] Non-Android stub so SchedulePage compiles on every target.
// iOS keeps the existing in-app dispatcher-timer pings for now; a
// CLLocationManager background pass (SiteWatchStubs pattern) can follow.
public static class VisitPingService
{
    public static void Track(int visitId) { }
    public static void Untrack(int visitId) { }
    public static void Stop() { }
}
#endif
