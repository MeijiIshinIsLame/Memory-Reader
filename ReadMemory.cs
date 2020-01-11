using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class MemoryReader
{
    const int PROCESS_WM_READ = 0x0010; //specifies that we are reading memory
    private string processName;
    private Process process;
    private IntPtr processHandle;
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    public MemoryReader(string _processName)
    {
        this.processName = _processName;
        this.process = GetProcessFromName(processName);
        this.processHandle = GetProcessHandle(process);
    }

    public static Process GetProcessFromName(string processName)
    {
        try
        {
            Process[] process = Process.GetProcessesByName(processName);
            Console.WriteLine("Opened process: {0}", processName);
            return process[0];
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Process may not be open, or the process name may be wrong.");
            return null;
        }
    }

    public static IntPtr GetProcessHandle(Process process)
    {
        try
        {
            return OpenProcess(PROCESS_WM_READ, false, process.Id); 
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Could not get process handle. Check if the process is still open, or if the name is correct.");
            return IntPtr.Zero; //returns null
        }
    }

    public int ReadAddress(int addr)
    {
        int bytesRead = 0;
        byte[] buffer = new byte[4];

        try
        {
            ReadProcessMemory((int)processHandle, addr, buffer, buffer.Length, ref bytesRead);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Cannot read address");
            return 0;
        }
        return BitConverter.ToInt32(buffer, 0);
    }

    public int ReadMultiLevelPointer(int[] offsets)
    {
        int baseaddr = process.MainModule.BaseAddress.ToInt32() + offsets[0];
        Int32 contents = ReadAddress(baseaddr);

        foreach(int offset in offsets)
        {
            if (offset == offsets[0]) //we already used first index
                continue;

            contents = ReadAddress(contents + offset);
        }

        return contents;
    }
}