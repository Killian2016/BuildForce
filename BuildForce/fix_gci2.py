import os

ROOT  = r"C:\Users\mezan\source\repos\BuildForce\BuildForce"
VIEWS = os.path.join(ROOT, "Views")

def write(path, text):
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    print("OK  " + path)

GCI_CS = os.path.join(VIEWS, "GroupClockInPage.xaml.cs")
t = open(GCI_CS, encoding="utf-8").read()
t = t.replace("emp.EmployeeId", "emp.Id")
write(GCI_CS, t)
print("Done! Build and deploy.")