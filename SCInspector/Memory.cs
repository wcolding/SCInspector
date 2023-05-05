using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SCInspector
{
    public static class Memory
    {
        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        const UInt32 WAIT_TIMEOUT = 0x00000102;

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [Flags]
        public enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public struct MODULEENTRY32
        {
            internal uint dwSize;
            internal uint th32ModuleID;
            internal uint th32ProcessID;
            internal uint GlblcntUsage;
            internal uint ProccntUsage;
            internal IntPtr modBaseAddr;
            internal uint modBaseSize;
            internal IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szExePath;
        }

        [DllImport("kernel32.dll")]
        static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        #endregion

        public static ulong baseAddress = 0;
        public static IntPtr handle;

        // Returns specified module's base address or a null value on failure
        public static IntPtr OpenGame(string windowName, string moduleName)
        {
            IntPtr hwnd = FindWindowA(null, windowName);
            if (hwnd == IntPtr.Zero) return hwnd;
            
            GetWindowThreadProcessId(hwnd, out pid);
            Process process = Process.GetProcessById(pid);
            if (process == null) return IntPtr.Zero;

            ProcessHandle = OpenProcess(ProcessAccessFlags.All, false, pid);

            foreach (ProcessModule pModule in process.Modules)
            {
                if (pModule.ModuleName == moduleName)
                {
                    ModuleBase = pModule.BaseAddress;
                    return pModule.BaseAddress;
                }
            }

            return IntPtr.Zero;
        }

        public static int processID { get { return pid; } }
        private static int pid = 0;

        public static IntPtr ProcessHandle = IntPtr.Zero;
        public static IntPtr ModuleBase = IntPtr.Zero;

        public static IntPtr outputPtr = IntPtr.Zero;

        public static UInt32 ReadUInt32(IntPtr offset)
        {
            byte[] buffer = new byte[4];
            Memory.ReadProcessMemory(Memory.ProcessHandle, offset, buffer, buffer.Length, out outputPtr);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static void WriteUInt32(IntPtr offset, uint value)
        {
            Memory.WriteProcessMemory(Memory.ProcessHandle, offset, BitConverter.GetBytes(value), 4, out outputPtr);
        }

        public static UInt32 ReadUInt16(IntPtr offset)
        {
            byte[] buffer = new byte[2];
            Memory.ReadProcessMemory(Memory.ProcessHandle, offset, buffer, buffer.Length, out outputPtr);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public static void WriteUInt16(IntPtr offset, ushort value)
        {
            Memory.WriteProcessMemory(Memory.ProcessHandle, offset, BitConverter.GetBytes(value), 2, out outputPtr);
        }

        public static float ReadFloat(IntPtr offset)
        {
            byte[] buffer = new byte[4];
            Memory.ReadProcessMemory(Memory.ProcessHandle, offset, buffer, buffer.Length, out outputPtr);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static void WriteFloat(IntPtr offset, float value)
        {
            Memory.WriteProcessMemory(Memory.ProcessHandle, offset, BitConverter.GetBytes(value), 4, out outputPtr);
        }

        public static UInt32 ReadUInt8(IntPtr offset)
        {
            byte[] buffer = new byte[1];
            Memory.ReadProcessMemory(Memory.ProcessHandle, offset, buffer, buffer.Length, out outputPtr);
            return buffer[0];
        }

        public static void WriteUInt8(IntPtr offset, byte value)
        {
            byte[] newBuffer = new byte[1];
            newBuffer[0] = value;
            Memory.WriteProcessMemory(Memory.ProcessHandle, offset, newBuffer, 1, out outputPtr);
        }

        public static string ReadString(IntPtr offset, bool isUnicode = false)
        {
            byte[] buffer = new byte[256];
            string temp = string.Empty;
            Memory.ReadProcessMemory(Memory.ProcessHandle, offset, buffer, buffer.Length, out outputPtr);
            if (isUnicode)
                temp = Encoding.Unicode.GetString(buffer);
            else 
                temp = Encoding.UTF8.GetString(buffer);

            int nullIndex = temp.IndexOf('\0');
            if (nullIndex > -1)
                temp = temp.Remove(nullIndex);

            return temp;
        }
    }

}
