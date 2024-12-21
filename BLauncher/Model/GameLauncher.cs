using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace BLauncher.Model
{
    public class GameLauncher
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_CREATE_THREAD = 0x0002;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_READWRITE = 0x04;

        public static void LaunchGame(string nickName, string ip, int port)
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string gameDirectory = Path.Combine(exePath, "bin", "Game");
            Directory.SetCurrentDirectory(gameDirectory);
            //Directory.SetCurrentDirectory(@"E:\samp\Banderstadt Project\Launcher\BLauncher\BLauncher\bin\Debug\bin\Game");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "gta_sa.exe",
                Arguments = "-c -n Vladyslav -h 192.168.1.1 -p 7777",//$"-c -n {nickName} -h {ip} -p {port}",
                UseShellExecute = false
            };

            Process process = Process.Start(startInfo);
            if (process != null)
            {
                IntPtr processHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, (uint)process.Id);
                if (processHandle != IntPtr.Zero)
                {
                    IntPtr loadLibAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                    IntPtr remoteString = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)("samp.dll".Length + 1), MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

                    WriteProcessMemory(processHandle, remoteString, "samp.dll", (uint)("samp.dll".Length + 1), out _);
                    CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibAddr, remoteString, 0, out _);
                    CloseHandle(processHandle);
                }
            }
        }
    }
}