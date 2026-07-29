#pragma warning disable CA1416
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BuildForce.Services;

// [OFF2] Durable punch queue. Construction sites lose signal constantly, so a
// punch is written to DISK FIRST and transmitted later. Each punch carries a
// GUID the server treats as an idempotency key, so a retry after a timeout can
// never create a duplicate timesheet. Selfies are stored as files and
// referenced by path - holding them as base64 inside the queue would bloat the
// file fast and risk losing every pending punch to one oversized write.
public enum PunchKind { ClockIn, ClockOut }

public class PendingPunch
{
    public string ClientPunchId { get; set; } = Guid.NewGuid().ToString("N");
    public PunchKind Kind { get; set; }

    // Device clock at the moment the worker actually punched.
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Description { get; set; }
    public string? PhotoPath { get; set; }

    // Clock-out only. TimesheetId is 0 when the clock-in never reached the
    // server, in which case ClockInClientPunchId locates the row instead.
    public int TimesheetId { get; set; }
    public string? ClockInClientPunchId { get; set; }
    public bool InjuryReported { get; set; }
    public string? InjuryDetails { get; set; }
    public bool AutoClockOut { get; set; }

    public int AttemptCount { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string? LastError { get; set; }
}

public static class PunchQueue
{
    public const string ActivePunchIdKey = "pending_clockin_punch_id";

    private static readonly object _gate = new object();
    private static string QueuePath => Path.Combine(FileSystem.AppDataDirectory, "punch-queue.json");
    private static string PhotoDir => Path.Combine(FileSystem.AppDataDirectory, "punch-photos");

    public static List<PendingPunch> All()
    {
        lock (_gate)
        {
            try
            {
                if (!File.Exists(QueuePath)) return new List<PendingPunch>();
                var json = File.ReadAllText(QueuePath);
                if (string.IsNullOrWhiteSpace(json)) return new List<PendingPunch>();
                return JsonSerializer.Deserialize<List<PendingPunch>>(json) ?? new List<PendingPunch>();
            }
            catch
            {
                return new List<PendingPunch>();
            }
        }
    }

    public static int Count => All().Count;

    // Write to a temp file then swap, so a crash mid-write cannot leave a
    // truncated queue - that would lose a worker's whole day.
    private static void Save(List<PendingPunch> items)
    {
        lock (_gate)
        {
            try
            {
                var tmp = QueuePath + ".tmp";
                File.WriteAllText(tmp, JsonSerializer.Serialize(items));
                if (File.Exists(QueuePath)) File.Delete(QueuePath);
                File.Move(tmp, QueuePath);
            }
            catch { }
        }
    }

    public static void Enqueue(PendingPunch punch)
    {
        var items = All();
        items.Add(punch);
        Save(items);
    }

    public static PendingPunch? First()
    {
        return All().OrderBy(p => p.OccurredAtUtc).FirstOrDefault();
    }

    public static void Remove(string clientPunchId)
    {
        var items = All();
        var hit = items.FirstOrDefault(p => p.ClientPunchId == clientPunchId);
        if (hit == null) return;
        DeletePhoto(hit.PhotoPath);
        items.Remove(hit);
        Save(items);
    }

    public static void MarkAttempt(string clientPunchId, string? error)
    {
        var items = All();
        var hit = items.FirstOrDefault(p => p.ClientPunchId == clientPunchId);
        if (hit == null) return;
        hit.AttemptCount++;
        hit.LastAttemptUtc = DateTime.UtcNow;
        hit.LastError = error;
        Save(items);
    }

    public static string? SavePhoto(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        try
        {
            var raw = base64;
            if (raw.StartsWith("data:"))
            {
                var comma = raw.IndexOf(',');
                if (comma > 0) raw = raw.Substring(comma + 1);
            }
            Directory.CreateDirectory(PhotoDir);
            var path = Path.Combine(PhotoDir, Guid.NewGuid().ToString("N") + ".jpg");
            File.WriteAllBytes(path, Convert.FromBase64String(raw));
            return path;
        }
        catch
        {
            return null;
        }
    }

    public static string? LoadPhoto(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        try
        {
            return File.Exists(path) ? Convert.ToBase64String(File.ReadAllBytes(path)) : null;
        }
        catch
        {
            return null;
        }
    }

    public static void DeletePhoto(string? path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
        }
        catch { }
    }
}