using System;
using System.Diagnostics;
using System.IO;
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

        // Script de banner + execução
        string dir = Path.Combine(local, "OpenClaudeLauncher");
        Directory.CreateDirectory(dir);
        string ps1 = Path.Combine(dir, "launch.ps1");
        string script =
@"$Host.UI.RawUI.WindowTitle = 'OpenClaude'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$e = [char]27; $d = [char]0x25CF
Clear-Host
Write-Host """"
Write-Host ""  $e[38;2;255;95;87m$d $e[38;2;254;188;46m$d $e[38;2;40;200;64m$d$e[0m""
Write-Host """"
Write-Host ""  $e[1;38;2;230;230;240mOpenClaude$e[0m  $e[38;2;120;125;145m·  " + Model + @"  ·  1M context$e[0m""
Write-Host ""  $e[38;2;80;84;100m" + new string('─', 44) + @"$e[0m""
Write-Host """"
& '" + oc.Replace("'", "''") + @"' --model " + Model + @" --permission-mode bypassPermissions
";
        File.WriteAllText(ps1, script, new UTF8Encoding(true));

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
            string cmdline = ("powershell.exe " + psArgs).Replace("\\", "\\\\").Replace("\"", "\\\"");
            string json = @"{
  ""profiles"": [
    {
      ""guid"": ""{6f1c3a52-9b7e-4c1d-8f2a-0c0de0c1a0de}"",
      ""name"": ""OpenClaude"",
      ""commandline"": """ + cmdline + @""",
      ""hidden"": false,
      ""colorScheme"": ""OpenClaude Soft"",
      ""useAcrylic"": true,
      ""opacity"": 82,
      ""font"": { ""face"": ""JetBrainsMono Nerd Font, JetBrains Mono, Cascadia Code, Cascadia Mono, Consolas"", ""size"": 12, ""cellHeight"": ""1.3"" },
      ""padding"": ""18, 14, 18, 14"",
      ""cursorShape"": ""bar"",
      ""tabTitle"": ""OpenClaude"",
      ""suppressApplicationTitle"": true,
      ""scrollbarState"": ""hidden"",
      ""antialiasingMode"": ""cleartype""
    }
  ],
  ""schemes"": [
    {
      ""name"": ""OpenClaude Soft"",
      ""background"": ""#16161E"", ""foreground"": ""#D4D6E4"",
      ""cursorColor"": ""#C0CAF5"", ""selectionBackground"": ""#33467C"",
      ""black"": ""#1F2030"", ""red"": ""#F7768E"", ""green"": ""#9ECE6A"", ""yellow"": ""#E0AF68"",
      ""blue"": ""#7AA2F7"", ""purple"": ""#BB9AF7"", ""cyan"": ""#7DCFFF"", ""white"": ""#C0CAF5"",
      ""brightBlack"": ""#565F89"", ""brightRed"": ""#FF899D"", ""brightGreen"": ""#B9F27C"", ""brightYellow"": ""#FFC777"",
      ""brightBlue"": ""#8DB0FF"", ""brightPurple"": ""#C7A9FF"", ""brightCyan"": ""#A4DAFF"", ""brightWhite"": ""#E6E8F5""
    }
  ]
}";
            File.WriteAllText(Path.Combine(fragDir, "openclaude.json"), json, new UTF8Encoding(false));
            Thread.Sleep(400);
            // usa -p com fallback para o commandline direto, caso o fragmento ainda não tenha sido carregado
            Process.Start(new ProcessStartInfo(wt, "-p \"OpenClaude\" --title OpenClaude") { UseShellExecute = true });
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
