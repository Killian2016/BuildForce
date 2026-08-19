#pragma warning disable CA1416
using BuildForce.Services;

namespace BuildForce.Views;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    public DashboardPage(ApiService api, AuthService auth)
    {
        InitializeComponent();
        try { VersionLabel.Text = "v" + AppInfo.VersionString; } catch { }
        _api = api;
        _auth = auth;
        LoadHeader();
        LoadLive();
        LoadAvatar(); // [PRF3b] ctor call - OnAppearing may not fire in dock
        LoadUnreadCount();   // [NOT3] unread badge
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAvatar();
        LoadHeader();
        LoadLive();
        LoadUnreadCount();   // [NOT3]
    }

    // [NOT3] unread badge + notifications page
    private async void LoadUnreadCount()
    {
        try
        {
            var count = await _api.GetUnreadNotificationCountAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NotifBadge.IsVisible = count > 0;
                NotifBadgeLabel.Text = count > 99 ? "99+" : count.ToString();
            });
        }
        catch { }
    }

    private async void OnNotifications(object sender, TappedEventArgs e)
    {
        try
        {
            var host = Application.Current?.MainPage;
            if (host != null)
                await host.Navigation.PushModalAsync(new NotificationsPage(_api));
        }
        catch (Exception ex)
        {
            var host2 = Application.Current?.MainPage;
            if (host2 != null) await host2.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void LoadHeader()
    {
        NewJobCard.IsVisible = AuthService.CanScheduleJobs; // [BFJOB1]
        var name = Preferences.Get("full_name", "");
        var email = Preferences.Get("email", "");
        var display = string.IsNullOrEmpty(name) ? email : name;
        if (string.IsNullOrEmpty(display)) display = "Mezano";

        UserNameLabel.Text = display;
        RoleLabel.Text = DateTime.Now.ToString("dddd, MMMM d");

        var parts = display.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string initials;
        if (parts.Length >= 2)
            initials = parts[0].Substring(0, 1) + parts[1].Substring(0, 1);
        else if (display.Length >= 2)
            initials = display.Substring(0, 2);
        else
            initials = "BF";
        AvatarLabel.Text = initials.ToUpper();
    }

    private async void LoadLive()
    {
        try
        {
            var dash = await _api.GetDashboardAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (dash != null)
                {
                    RoleLabel.Text = DateTime.Now.ToString("dddd, MMMM d") + "  |  " + dash.ActiveProjects + " active projects";
                    SyncLabel.Text = "Online";
                    SyncLabel.TextColor = Color.FromArgb("#10b981");
                }
                else
                {
                    SyncLabel.Text = "Offline";
                    SyncLabel.TextColor = Color.FromArgb("#f0a500");
                }
            });
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SyncLabel.Text = "Offline";
                SyncLabel.TextColor = Color.FromArgb("#f0a500");
            });
        }

        try
        {
            var active = await _api.GetActiveTimesheetAsync();
            var summary = await _api.GetTimesheetSummaryAsync();
            string hours = summary != null ? summary.TotalHours.ToString("F1") + "h this week" : "";
            bool onClock = active != null && active.Status == "Active";
            string text = onClock ? "You are on the clock" : "You are not clocked in";
            if (hours.Length > 0) text = text + "  |  " + hours;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CrewSub.Text = text;
                CrewSub.TextColor = onClock ? Color.FromArgb("#10b981") : Color.FromArgb("#7d8590");
            });
        }
        catch { }

        try
        {
            var expenses = await _api.GetExpensesAsync();
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var recent = expenses.Where(e => e.ExpenseDate >= monthStart).OrderByDescending(e => e.ExpenseDate).ToList();
            string text;
            if (recent.Count == 0)
            {
                text = "Nothing logged this month";
            }
            else
            {
                var last = recent[0];
                var who = string.IsNullOrEmpty(last.Vendor) ? last.Description : last.Vendor;
                text = recent.Count + " this month  |  last " + who;
            }
            MainThread.BeginInvokeOnMainThread(() => MaterialsSub.Text = text);
        }
        catch { }
    }

    private static Page? HostPage => Application.Current?.MainPage;

    private static async Task AlertAsync(string title, string message)
    {
        var host = HostPage;
        if (host != null)
            await host.DisplayAlert(title, message, "OK");
    }

    private static async Task ComingSoonAsync(string feature)
    {
        await AlertAsync(feature, feature + " needs a Mezano CM endpoint before it can go live.");
    }

    private async void OnTakePicture(object sender, EventArgs e)
    {
        try
        {
            await Application.Current!.MainPage!.Navigation.PushModalAsync(new ProjectPhotosPage(_api));
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Navigation error", ex.Message, "OK");
        }
    }

    private async void OnViewProjects(object sender, EventArgs e)
    {
        try
        {
            MainShellPage.Current?.GoToProjects();   // [NAV3]
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await AlertAsync("Navigation error", ex.Message);
        }
    }

    private async void OnDailyLog(object sender, EventArgs e)
    {
        await Application.Current!.MainPage!.Navigation.PushModalAsync(new SiteLogPage(_api));
    }

    private async void OnDailyLogTap(object sender, TappedEventArgs e)
    {
        await Application.Current!.MainPage!.Navigation.PushModalAsync(new SiteLogPage(_api));
    }

    private async void OnCrew(object sender, TappedEventArgs e)
    {
        await Application.Current!.MainPage!.Navigation.PushModalAsync(new CrewPage(_api));
    }

    private async void OnBlueprints(object sender, TappedEventArgs e)
    {
        try // [BLP3] Blueprints -> BlueprintsPage
        {
            var hostBp = Application.Current?.MainPage;
            if (hostBp != null)
                await hostBp.Navigation.PushModalAsync(new BlueprintsPage(_api));
        }
        catch (Exception bpEx)
        {
            var hostBp2 = Application.Current?.MainPage;
            if (hostBp2 != null) await hostBp2.DisplayAlert("Navigation error", bpEx.Message, "OK");
        }
    }

    // ============================================
    // SEARCH [SRCH1] in-app destinations + [SRCH2] server content.
    // Results render in PickerSheetPage so search looks like every other
    // dropdown in the app. Server hits DEGRADE QUIETLY: if /api/mobile/search
    // is not published yet SearchAsync returns null, and the sheet still
    // lists the in-app destinations instead of showing an error.
    // ============================================
    private sealed class SearchDest
    {
        public string Name = "";
        public string Keywords = "";
        public Func<Page> Build = null!;
    }

    private List<SearchDest> BuildDestinations()
    {
        var api = _api;
        return new List<SearchDest>
        {
            new SearchDest { Name = "Daily site log",     Keywords = "log logs daily site weather crew work completed delay delays materials notes", Build = () => new SiteLogPage(api) },
            new SearchDest { Name = "Project photos",     Keywords = "photo photos picture pictures camera image images media capture",              Build = () => new ProjectPhotosPage(api) },
            new SearchDest { Name = "Blueprints",         Keywords = "blueprint blueprints plan plans sheet sheets drawing drawings pdf",            Build = () => new BlueprintsPage(api) },
            new SearchDest { Name = "Submittals",         Keywords = "submittal submittals transmittal approval spec section shop drawing product data", Build = () => new SubmittalsPage(api) },
            new SearchDest { Name = "Safety inspection",  Keywords = "safety inspection inspections checklist hazard daily weekly",                  Build = () => new SafetyInspectionPage(api) },
            new SearchDest { Name = "Crew on the clock",  Keywords = "crew team worker workers who clocked hours",                                   Build = () => new CrewPage(api) },
            new SearchDest { Name = "Schedule",           Keywords = "schedule calendar assignment assignments upcoming",                            Build = () => new SchedulePage(api) },
            new SearchDest { Name = "Log an expense",     Keywords = "expense material materials received receipt delivery cost vendor scan",        Build = () => new ExpenseCreatePage(api) },
            new SearchDest { Name = "Expenses",           Keywords = "expenses spending costs receipts list",                                        Build = () => new ExpensesPage(api) },
            new SearchDest { Name = "Projects",           Keywords = "project projects job jobs",                                                    Build = () => new ProjectsPage(api) },
            new SearchDest { Name = "Account settings",   Keywords = "account settings profile password photo avatar me sign out",                   Build = () => new ProfilePage(api) },
        };
    }

    private static bool DestMatches(SearchDest d, string q)
    {
        return d.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
            || d.Keywords.Contains(q, StringComparison.OrdinalIgnoreCase);
    }

    private Page PageForHit(SearchHit h)
    {
        switch (h.Kind)
        {
            case "Project":   return new ProjectDetailPage(_api, h.Id);
            case "Site log":  return new SiteLogPage(_api);
            case "Photo":     return new ProjectPhotosPage(_api);
            case "Blueprint": return new BlueprintsPage(_api);
            case "Submittal": return new SubmittalsPage(_api);
            case "Safety":    return new SafetyInspectionPage(_api);
            case "Expense":   return new ExpensesPage(_api);
            default:          return new ProjectsPage(_api);
        }
    }

    private async void OnSearch(object sender, EventArgs e)
    {
        var q = (SearchEntry.Text ?? "").Trim();
        if (q.Length < 2)
        {
            await AlertAsync("Search", "Type at least 2 characters, then press enter.");
            return;
        }
        try { SearchEntry.Unfocus(); } catch { }

        var host = HostPage;
        if (host == null) return;

        var labels = new List<string>();
        var opens = new List<Func<Page>>();

        // in-app destinations first - instant, works with no signal
        foreach (var d in BuildDestinations())
        {
            if (!DestMatches(d, q)) continue;
            labels.Add("Open  -  " + d.Name);
            opens.Add(d.Build);
        }

        // then server content; null means the endpoint is unreachable
        List<SearchHit>? hits = null;
        try { hits = await _api.SearchAsync(q); } catch { }

        if (hits != null)
        {
            foreach (var h in hits)
            {
                var title = string.IsNullOrWhiteSpace(h.Title) ? "(untitled)" : h.Title;
                var line = (h.Kind ?? "Result") + "  -  " + title;
                if (!string.IsNullOrWhiteSpace(h.Subtitle)) line = line + "  (" + h.Subtitle + ")";
                labels.Add(line);
                var captured = h;
                opens.Add(() => PageForHit(captured));
            }
        }

        if (labels.Count == 0)
        {
            var none = hits == null
                ? "Nothing in the app matches \"" + q + "\", and content search is unavailable right now."
                : "No matches for \"" + q + "\".";
            await AlertAsync("Search", none);
            return;
        }

        var subtitle = hits == null
            ? labels.Count + " in-app match(es) - content search unavailable"
            : labels.Count + " match(es)";

        int pick = await PickerSheetPage.PickIndexAsync(host.Navigation, "Results for \"" + q + "\"", labels, -1, subtitle);
        if (pick < 0 || pick >= opens.Count) return;

        try { await host.Navigation.PushModalAsync(opens[pick]()); }
        catch (Exception ex) { await AlertAsync("Navigation error", ex.Message); }
    }

    private async void OnMaterialsReceived(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage;
            if (host != null)
                await host.Navigation.PushModalAsync(new ExpenseCreatePage(_api));
        }
        catch (Exception ex)
        {
            await AlertAsync("Navigation error", ex.Message);
        }
    }

    private async void OnSafetyForms(object sender, TappedEventArgs e)
    {
        await Application.Current!.MainPage!.Navigation.PushModalAsync(new SafetyInspectionPage(_api));
    }

    private async void OnSubmittals(object sender, TappedEventArgs e)
    {
        try // [SUB3b] Submittals -> SubmittalsPage
        {
            var hostSub = Application.Current?.MainPage;
            if (hostSub != null)
                await hostSub.Navigation.PushModalAsync(new SubmittalsPage(_api));
        }
        catch (Exception ex)
        {
            var hostSub2 = Application.Current?.MainPage;
            if (hostSub2 != null) await hostSub2.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnAccountSettings(object sender, TappedEventArgs e)
    {
        try // [PRF2] Account settings -> ProfilePage
        {
            var hostPg = Application.Current?.MainPage;
            if (hostPg != null)
                await hostPg.Navigation.PushModalAsync(new ProfilePage(
_api
));
        }
        catch (Exception navEx)
        {
            var hostPg2 = Application.Current?.MainPage;
            if (hostPg2 != null) await hostPg2.DisplayAlert("Navigation error", navEx.Message, "OK");
        }
    }

    private async void OnSignOut(object sender, TappedEventArgs e)
    {
        var host = HostPage;
        if (host == null) return;

        bool confirm = await host.DisplayAlert("Sign out", "Sign out of Mezano?", "Sign out", "Cancel");
        if (!confirm) return;

        Preferences.Clear();
        Application.Current!.MainPage = new LoginPage(_auth);
    }

    private async void LoadAvatar() // [PRF3] dashboard avatar photo
    {
        try
        {
            var bytes = await _api.GetProfilePhotoImageAsync();
            if (bytes == null || bytes.Length == 0) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvatarImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                AvatarImage.IsVisible = true;
                AvatarLabel.IsVisible = false;
            });
        }
        catch { }
    }

    private async void OnAvatarTap(object sender, TappedEventArgs e)
    {
        try
        {
            var host = Application.Current?.MainPage;
            if (host != null) await host.Navigation.PushModalAsync(new ProfilePage(_api));
        }
        catch { }
    }

    // [BFJOB1] office-only quick job (Jobber-style New job sheet)
    private async void OnNewJob(object sender, TappedEventArgs e)
    {
        try
        {
            var host = HostPage;
            if (host != null) await host.Navigation.PushModalAsync(new NewJobPage(_api));
        }
        catch (Exception ex) { await AlertAsync("Navigation error", ex.Message); }
    }
}
