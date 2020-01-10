using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class MemoryReader
{
    const int PROCESS_WM_READ = 0x0010; //specifies that we are reading memory
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    public static Process OpenProcess(string processName)
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
            return null; //stop program
        }
    }

    public static int ReadAddress(Process process, int addr)
    {
        IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id); 

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
            return 0; //stop program
        }
        return BitConverter.ToInt32(buffer, 0);
    }

    public static int ReadMultiLevelPointer(Process process, int[] offsets)
    {
        int baseaddr = process.MainModule.BaseAddress.ToInt32() + offsets[0];
        Int32 contents = contents = ReadAddress(process, baseaddr);

        foreach(int offset in offsets)
        {
            if (offset == offsets[0]) //we already used first index
                continue;

            contents = ReadAddress(process, contents + offset);
        }

        return contents;
    }
}

