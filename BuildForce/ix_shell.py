

































































import os

ROOT = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

SHELL = os.path.join(ROOT, "AppShell.xaml")
SHELL_TEXT = """<?xml version="1.0" encoding="utf-8" ?>
<Shell
    x:Class="BuildForce.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:BuildForce.Views"
    Shell.NavBarIsVisible="False"
    Shell.TabBarIsVisible="False">
    <TabBar>
        <Tab Title="Dashboard">
            <ShellContent ContentTemplate="{DataTemplate views:DashboardPage}" Route="DashboardPage"/>
        </Tab>
        <Tab Title="Projects">
            <ShellContent ContentTemplate="{DataTemplate views:ProjectsPage}" Route="ProjectsPage"/>
        </Tab>
        <Tab Title="Invoices">
            <ShellContent ContentTemplate="{DataTemplate views:InvoicesPage}" Route="InvoicesPage"/>
        </Tab>
        <Tab Title="Expenses">
            <ShellContent ContentTemplate="{DataTemplate views:ExpensesPage}" Route="ExpensesPage"/>
        </Tab>
        <Tab Title="Time">
            <ShellContent ContentTemplate="{DataTemplate views:TimesheetPage}" Route="TimesheetPage"/>
        </Tab>
        <Tab Title="Customers">
            <ShellContent ContentTemplate="{DataTemplate views:CustomersPage}" Route="CustomersPage"/>
        </Tab>
        <Tab Title="More">
            <ShellContent ContentTemplate="{DataTemplate views:MorePage}" Route="MorePage"/>
        </Tab>
    </TabBar>
</Shell>
"""

write(SHELL, SHELL_TEXT)
print("Done! Build and deploy.")









