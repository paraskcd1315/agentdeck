namespace AgentDeck.Shell.Utils;

public static class Constants
{
    public const string DaemonPipeServer = ".";
    public const string DaemonPipeName = "agentdeck";
    public const int DaemonConnectTimeoutMs = 5000;
    public const string JsonRpcVersion = "2.0";
    public const string StateDirectoryName = ".agentdeck";
    public const string ConfigFileName = "config.json";

    public static class Theme
    {
        public const double DefaultBackgroundGradientOpacity = 0.75;
        public const double DefaultTerminalOpacity = 0.5;
        public const double DefaultTerminalBlur = 24;
        public const double DefaultTerminalSaturation = 1.2;
        public const double DefaultDitherStrength = 0.035;

        public const string GradientAppKey = "AdGradientApp";
        public const string TerminalCanvasKey = "AdTermCanvasBrush";
        public const string TerminalForegroundKey = "AdTermForegroundBrush";
        public const string GlassBlurKey = "AdGlassBlur";
    }

    public static class Method
    {
        public const string PtySpawn = "pty.spawn";
        public const string PtyWrite = "pty.write";
        public const string PtyResize = "pty.resize";
        public const string PtyKill = "pty.kill";
        public const string PanelList = "panel.list";
        public const string PanelRead = "panel.read";
        public const string WorkspaceOpen = "workspace.open";
    }

    public static class Notification
    {
        public const string PtyData = "pty.data";
        public const string PtyExit = "pty.exit";
        public const string HookEvent = "hook.event";
        public const string PanelChanged = "panel.changed";
    }

    public static class Shell
    {
        public const string FallbackProgram = "powershell.exe";

        public static readonly string[] GitBashCandidates =
        [
            @"C:\Program Files\Git\bin\bash.exe",
            @"C:\Program Files (x86)\Git\bin\bash.exe",
        ];

        public static readonly string[] GitBashDefaultArgs = ["--noprofile", "--norc"];
    }
}
