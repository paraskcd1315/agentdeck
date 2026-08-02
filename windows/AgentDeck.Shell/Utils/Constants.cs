namespace AgentDeck.Shell.Utils;

public static class Constants
{
    public const string DaemonPipeServer = ".";
    public const string DaemonPipeName = "agentdeck";
    public const int DaemonConnectTimeoutMs = 5000;
    public const string JsonRpcVersion = "2.0";

    public static class Method
    {
        public const string PtySpawn = "pty.spawn";
        public const string PtyWrite = "pty.write";
        public const string PtyResize = "pty.resize";
        public const string PtyKill = "pty.kill";
    }

    public static class Notification
    {
        public const string PtyData = "pty.data";
        public const string PtyExit = "pty.exit";
        public const string HookEvent = "hook.event";
    }

    public static class Shell
    {
        public const string DefaultProgram = "powershell.exe";
    }
}
