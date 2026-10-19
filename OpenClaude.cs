using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

class Launcher
{
    const string Model = "claude-opus-5-5";
    const string CtxCmd = "/set-context-window claude-opus-5-5 1000000";
    const string Title = "OpenClaude";

    static string FindOnPath(string exe)
    {
        var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';');
        foreach (var p in paths)
        {
            try { var f = Path.Combine(p.Trim(), exe); if (File.Exists(f)) return f; } catch { }
        }
        return null;
    }

    static string ExtractIcon(string dir)
    {
        string path = Path.Combine(dir, "OpenClaude.ico");
        try
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenClaude.ico"))
            {
                if (s == null) return null;
                if (!File.Exists(path) || new FileInfo(path).Length != s.Length)
                    using (var f = File.Create(path)) s.CopyTo(f);
            }
            return path;
        }
        catch { return File.Exists(path) ? path : null; }
    }

    [STAThread]
    static void Main()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string oc = FindOnPath("openclaude.cmd") ?? FindOnPath("openclaude.exe");
        if (oc == null && File.Exists(Path.Combine(appData, @"npm\openclaude.cmd")))
            oc = Path.Combine(appData, @"npm\openclaude.cmd");
        if (oc == null)
        {
            MessageBox.Show(
                "OpenClaude não foi encontrado.\n\n" +
                "1. Instale o Node.js 20+:  https://nodejs.org\n" +
                "2. Rode no terminal:\n     npm install -g @twigpine/openclaude\n" +
                "3. Abra este launcher de novo.",
                "OpenClaude Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string dir = Path.Combine(local, "OpenClaudeLauncher");
        Directory.CreateDirectory(dir);
        string icon = ExtractIcon(dir);

        string ps1 = Path.Combine(dir, "launch.ps1");
        string script =
@"$Host.UI.RawUI.WindowTitle = 'OpenClaude'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
Clear-Host
& '" + oc.Replace("'", "''") + @"' --model " + Model + @" --permission-mode bypassPermissions
";
        File.WriteAllText(ps1, script, new UTF8Encoding(true));
        File.Delete(Path.Combine(dir, "banner.ans"));

        string psArgs = "-NoLogo -NoProfile -ExecutionPolicy Bypass -File \"" + ps1 + "\"";

        string wt = FindOnPath("wt.exe");
        string wtApps = Path.Combine(local, @"Microsoft\WindowsApps\wt.exe");
        if (wt == null && File.Exists(wtApps)) wt = wtApps;

        if (wt == null)
        {
            var r = MessageBox.Show(
                "Windows Terminal não está instalado (necessário para acrílico/blur e fonte moderna).\n\n" +
                "Instalar agora via winget?\n\n(Não = abrir no console clássico)",
                "OpenClaude Launcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                try
                {
                    var p = Process.Start(new ProcessStartInfo("winget",
                        "install -e --id Microsoft.WindowsTerminal --accept-source-agreements --accept-package-agreements") { UseShellExecute = true });
                    p.WaitForExit();
                    try
                    {
                        Process.Start(new ProcessStartInfo("winget",
                            "install -e --id DEVCOM.JetBrainsMonoNerdFont --accept-source-agreements --accept-package-agreements") { UseShellExecute = true }).WaitForExit();
                    } catch { }
                }
                catch
                {
                    MessageBox.Show("winget indisponível. Instale o Windows Terminal pela Microsoft Store.", "OpenClaude Launcher");
                }
                if (File.Exists(wtApps)) wt = wtApps; else wt = FindOnPath("wt.exe");
            }
        }

        if (wt != null)
        {
            string fragDir = Path.Combine(local, @"Microsoft\Windows Terminal\Fragments\OpenClaude");
            Directory.CreateDirectory(fragDir);
            Func<string, string> js = s => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
            string cmdline = js("powershell.exe " + psArgs);
            string iconLine = icon != null ? @"""icon"": """ + js(icon) + @""",
      " : "";
            string json = @"{
  ""profiles"": [
    {
      ""guid"": ""{6f1c3a52-9b7e-4c1d-8f2a-0c0de0c1a0de}"",
      ""name"": ""OpenClaude"",
      ""commandline"": """ + cmdline + @""",
      " + iconLine + @"""startingDirectory"": ""%USERPROFILE%"",
      ""hidden"": false,
      ""colorScheme"": ""OpenClaude Aurora"",
      ""useAcrylic"": true,
      ""opacity"": 78,
      ""font"": { ""face"": ""JetBrainsMono Nerd Font, JetBrains Mono, Cascadia Code, Cascadia Mono, Consolas"", ""size"": 12, ""cellHeight"": ""1.25"" },
      ""padding"": ""22, 16, 22, 16"",
      ""cursorShape"": ""bar"",
      ""tabTitle"": ""OpenClaude"",
      ""tabColor"": ""#1B1726"",
      ""suppressApplicationTitle"": true,
      ""scrollbarState"": ""hidden"",
      ""intenseTextStyle"": ""bright"",
      ""antialiasingMode"": ""cleartype""
    }
  ],
  ""schemes"": [
    {
      ""name"": ""OpenClaude Aurora"",
      ""background"": ""#13111C"", ""foreground"": ""#DCDCF0"",
      ""cursorColor"": ""#FF7AC6"", ""selectionBackground"": ""#3D2E66"",
      ""black"": ""#1E1B2B"", ""red"": ""#FF5A7A"", ""green"": ""#8CE99A"", ""yellow"": ""#FFB86B"",
      ""blue"": ""#6E9BFF"", ""purple"": ""#B48CFF"", ""cyan"": ""#7FDBFF"", ""white"": ""#CFD0E8"",
      ""brightBlack"": ""#5E5A7A"", ""brightRed"": ""#FF7A93"", ""brightGreen"": ""#A8F5B4"", ""brightYellow"": ""#FFCB8E"",
      ""brightBlue"": ""#8FB3FF"", ""brightPurple"": ""#C9A8FF"", ""brightCyan"": ""#A3E7FF"", ""brightWhite"": ""#F2F2FF""
    }
  ]
}";
            File.WriteAllText(Path.Combine(fragDir, "openclaude.json"), json, new UTF8Encoding(false));
            Thread.Sleep(400);
            Process.Start(new ProcessStartInfo(wt, "--size 112,36 -p \"OpenClaude\" --title OpenClaude") { UseShellExecute = true });
        }
        else
        {
            Process.Start(new ProcessStartInfo("powershell.exe", psArgs) { UseShellExecute = true });
        }

        // Envia /set-context-window quando a janela estiver pronta
        bool found = false;
        for (int i = 0; i < 60 && !found; i++)
        {
            Thread.Sleep(500);
            try { Microsoft.VisualBasic.Interaction.AppActivate(Title); found = true; } catch { }
        }
        if (!found) return;
        Thread.Sleep(7000); // tempo para o OpenClaude carregar
        try
        {
            Microsoft.VisualBasic.Interaction.AppActivate(Title);
            Thread.Sleep(200);
            SendKeys.SendWait(CtxCmd);
            Thread.Sleep(150);
            SendKeys.SendWait("{ENTER}");
        }
        catch { }
    }
}
