using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.Host
{
    class Qemu : DebugHost
    {
        enum GDBMessageType
        {
            StopReply,
            PlusMinus,
        }

        struct Message
        {
            public GDBMessageType Type
            {get;set;}
            public string Text
            {get;set;}
        }

        TcpClient _client = new TcpClient();
        NetworkStream _stream = null;
        Process _process = new Process();
        bool _attached = false;
        bool _running = false;
        bool _exited = false;
        x86StackContext[] _stack = null;
        Queue<Message> _messages = new Queue<Message>();

        public override x86StackContext[] StackFrames
        {
            get { return _stack; }
        }

        public override Register[] Registers
        {
            get {
                List<Register> registers = new List<Register>();
                x86Registers regs = GetRegisters();
                registers.Add(new Register("EAX", regs.eax));
                registers.Add(new Register("EBX", regs.ebx));
                registers.Add(new Register("ECX", regs.ecx));
                registers.Add(new Register("EDX", regs.edx));
                registers.Add(new Register("EBP", regs.ebp));
                registers.Add(new Register("ESP", regs.esp));
                registers.Add(new Register("EDI", regs.edi));
                registers.Add(new Register("ESI", regs.esi));

                registers.Add(new Register("EFLAGS", regs.EFLAGS));

                registers.Add(new Register("CS", regs.cs));
                registers.Add(new Register("SS", regs.ss));
                registers.Add(new Register("DS", regs.ds));
                registers.Add(new Register("ES", regs.es));
                registers.Add(new Register("FS", regs.fs));
                registers.Add(new Register("GS", regs.gs));
                return registers.ToArray();
            }
        }

        public Qemu()
        {
           
        }

        protected override void DoLaunchSuspended(string strFile)
        {
            _process = new Process();
            _process.StartInfo = new ProcessStartInfo(@"C:\Qemu\qemu-system-i386.exe", "-S -gdb tcp::1234,ipv4 -net nic,model=rtl8139,macaddr=52:54:00:12:34:57 -net user -net dump,file=E:\\Doors\\Qemu-netout.pcap -kernel \"" + strFile + "\"");
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.Start();
            _running = true;
            Task.Factory.StartNew(new Action(() =>
                {
                    try
                    {
                        while (!_process.HasExited)
                        {
                            System.Threading.Thread.Sleep(25);
                            if ((_process.HasExited) && !_exited)
                                OnDisconnected();
                        }
                    }
                    finally
                    {
                        _running = false;
                    }
                }));
        }

        private Object awaitLock = new Object();
        private int awaitCount = 0;
        private void AwaitStop()
        {
            try
            {
                int count = 0;
                lock (awaitLock)
                {
                    count = awaitCount;
                    awaitCount++;
                }
                if (count == 0)
                {
                    lock (_stream)
                    {
                        string rsp = GetResponse(int.MaxValue);
                    }
                }
                else
                {
                    while(true)
                    {
                        lock(awaitLock)
                        {
                            if (awaitCount <= count)
                                break;
                        }
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            finally
            {
                lock(awaitLock)
                {
                    awaitCount--;
                }
            }
            OnSuspended();
        }

        public override void Attach()
        {
            Connect();
            _attached = true;
        }

        public override void Detach()
        {
            _attached = false;
            _client.Close();
        }

        protected override void DoResume()
        {
            Continue();
        }

        protected override void DoSuspend()
        {
            Break();
        }

        private void Break()
        {
            System.Diagnostics.Debug.Print("QEMU Break!");
            bool shouldWait = false;
            lock (awaitLock)
            {
                _stream.WriteByte(0x3);
                if (awaitCount > 0)
                    shouldWait = true;
            }
            if (shouldWait)
            {
                while (true)
                {
                    lock (awaitLock)
                    {
                        if (awaitCount < 1)
                            break;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        protected override void CreateBreakpoint(ulong addr)
        {
            string rsp = SendOKableCommand("Z1," + addr.ToString("X") + ",0");
        }

        protected override void DeleteBreakpoint(ulong addr)
        {
            string rsp = SendOKableCommand("z1," + addr.ToString("X") + ",0");
        }

        private void Connect()
        {
            _client.Connect(System.Net.IPAddress.Loopback, 1234);
            _stream = _client.GetStream();
        }

        public override bool IsAttached
        {
            get { return _attached; }
        }

        public override bool IsRunning
        {
            get { return _running; }
        }

        public override bool WalkStack(AD7.AD7Thread thread)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            List<x86StackContext> frames = new List<x86StackContext>();
            SetCurrentThreadContext(GetCurrentThreadId());
            x86Registers regs = GetRegisters();

            x86StackContext ctx = new x86StackContext();
            ctx.ebp = regs.ebp;
            ctx.esp = regs.esp;
            ctx.eip = regs.eip;

            while (ctx.esp <= ctx.ebp)
            {
                frames.Add(ctx);
                x86StackContext newCtx = new x86StackContext();

                if ((ctx.ebp - ctx.esp) < 0)
                    break;

                uint length = ctx.ebp - ctx.esp + 8;
                byte[] frame = ReadMemory(ctx.esp, ctx.ebp - ctx.esp + 8); // 8 extra to get the prior ebp and eip
                newCtx.ebp = (uint)NumberFromLittleEndianHexString(frame[length - 8].ToString("X2") + frame[length - 7].ToString("X2") + frame[length - 6].ToString("X2") + frame[length - 5].ToString("X2"));
                newCtx.esp = ctx.ebp - 8;
                newCtx.eip = (uint)NumberFromLittleEndianHexString(frame[length - 4].ToString("X2") + frame[length - 3].ToString("X2") + frame[length - 2].ToString("X2") + frame[length - 1].ToString("X2"));
                ctx = newCtx;
            }
            _stack = frames.ToArray();
            sw.Stop();
            System.Diagnostics.Debug.Print("WalkStack time: " + sw.ElapsedMilliseconds.ToString() + " ms");
            return frames.Count > 0;
        }

        protected override void DoTerminate()
        {
            if (!_process.HasExited)
            {
                try { Kill(); }
                catch { }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 1000)
            {
                if (_process.HasExited)
                    break;
            }

            if (!_process.HasExited)
            {
                try
                {
                    _exited = true;
                    _process.Kill();
                }
                catch { }
            }
        }

        private void Continue()
        {
            SendRunableCommand("c");
        }

        private void Kill()
        {
            SendCommand("k");
        }

        private uint GetCurrentThreadId()
        {
            string rsp = SendQueryCommand("qC");
            if (rsp.StartsWith("QC"))
            {
                uint id;
                if (UInt32.TryParse(rsp.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture, out id))
                    return id;
            }
            throw new NotImplementedException();
        }

        private bool SetCurrentThreadContext(uint threadId)
        {
            string rsp = SendQueryCommand("Hg" + threadId.ToString("X"));
            return rsp == "OK";
        }

        private x86Registers GetRegisters()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            string rsp = SendQueryCommand("g");
            x86Registers context = new x86Registers();
            context.eax = (uint)NumberFromLittleEndianHexString(rsp.Substring(0, 8));
            context.ecx = (uint)NumberFromLittleEndianHexString(rsp.Substring(8, 8));
            context.edx = (uint)NumberFromLittleEndianHexString(rsp.Substring(16, 8));
            context.ebx = (uint)NumberFromLittleEndianHexString(rsp.Substring(24, 8));
            context.esp = (uint)NumberFromLittleEndianHexString(rsp.Substring(32, 8));
            context.ebp = (uint)NumberFromLittleEndianHexString(rsp.Substring(40, 8));
            context.esi = (uint)NumberFromLittleEndianHexString(rsp.Substring(48, 8));
            context.edi = (uint)NumberFromLittleEndianHexString(rsp.Substring(56, 8));
            context.eip = (uint)NumberFromLittleEndianHexString(rsp.Substring(64, 8));
            context.EFLAGS = (uint)NumberFromLittleEndianHexString(rsp.Substring(72, 8));
            context.cs = (ushort)NumberFromLittleEndianHexString(rsp.Substring(80, 8));
            context.ss = (ushort)NumberFromLittleEndianHexString(rsp.Substring(88, 8));
            context.ds = (ushort)NumberFromLittleEndianHexString(rsp.Substring(96, 8));
            context.es = (ushort)NumberFromLittleEndianHexString(rsp.Substring(104, 8));
            context.fs = (ushort)NumberFromLittleEndianHexString(rsp.Substring(112, 8));
            context.gs = (ushort)NumberFromLittleEndianHexString(rsp.Substring(120, 8));
            sw.Stop();
            System.Diagnostics.Debug.Print("Get Registers: " + sw.ElapsedMilliseconds.ToString() + " ms");
            return context;
        }

        public override byte[] ReadMemory(ulong address, ulong length)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            System.Diagnostics.Debug.Print("ReadMem 0x" + address.ToString("X") + " - " + length);
            string rsp = SendQueryCommand("m" + address.ToString("X") + "," + length.ToString("X"));
            List<byte> bytes = new List<byte>();

            for (int i = 1; i < rsp.Length; i = i + 2)
            {
                bytes.Add(byte.Parse(rsp.Substring(i - 1, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            sw.Stop();
            System.Diagnostics.Debug.Print("Read Memory (" + length.ToString() + "): " + sw.ElapsedMilliseconds.ToString() + " ms");
            return bytes.ToArray();
        }

        private ulong NumberFromLittleEndianHexString(string inputStr)
        {
            string str = "";
            for (int i = inputStr.Length - 1; i >= 0; i = i - 2)
            {
                str += inputStr[i-1];
                str += inputStr[i];
            }
            return uint.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        private string SendOKableCommand(string cmd)
        {
            string rsp = SendQueryCommand(cmd);
            if (rsp != "OK")
                throw new NotImplementedException();
            return rsp;
        }

        private void SendRunableCommand(string cmd)
        {
            SendCommand(cmd);
            Task.Run(new Action(() => { AwaitStop(); }));
        }

        private string SendQueryCommand(string cmd)
        {
            lock (_stream)
            {
                SendCommand(cmd);
                return GetResponse();
            }
        }

        private void SendCommand(string cmd)
        {
            int tries = 0;

        Retry:
            //System.Diagnostics.Debug.Print("QEMU Write: " + cmd.ToString());
            WriteCommand(cmd.ToArray());

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 500)
            {
                if (_stream.DataAvailable)
                {
                    char c = (char)_stream.ReadByte();

                    if (cmd[0] == 'k')
                    {
                        while (_stream.DataAvailable)
                            _stream.ReadByte();
                        break;
                    }
                    //System.Diagnostics.Debug.Print("QEMU Read: " + c.ToString());
                    if (c == '-')
                    {
                        if (tries > 3)
                            throw new NotImplementedException();
                        tries++;
                        goto Retry;
                    }
                    if (c == '+')
                        return;
                }
                System.Threading.Thread.Sleep(5);
            }
        }

        private void WriteCommand(char[] data)
        {
            long checksum = 0;
            _stream.WriteByte(0x24);
            for (int i = 0; i < data.Length; i++)
            {
                if ((data[i] == 0x23) || (data[i] == 0x24) || (data[i] == 0x7d))
                {
                    checksum += 0x7d;
                    _stream.WriteByte(0x7d);
                    checksum += (data[i] ^ 0x20);
                    _stream.WriteByte((byte)(data[i] ^ 0x20));
                }
                else
                {
                    checksum += data[i];
                    _stream.WriteByte((byte)data[i]);
                }
            }
            _stream.WriteByte(0x23);
            checksum &= 0xFF;
            long first = checksum >> 4;
            long second = checksum & 0xF;
            if (first > 0x9)
                _stream.WriteByte((byte)(0x57 + first));
            else
                _stream.WriteByte((byte)(0x30 + first));
            if (second > 0x9)
                _stream.WriteByte((byte)(0x57 + second));
            else
                _stream.WriteByte((byte)(0x30 + second));
        }

        private string GetResponse(int timeout = 500)
        {
            string rsp = "";
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool escape = false;
            while (sw.ElapsedMilliseconds < timeout)
            {
                if (_stream.DataAvailable)
                {
                    byte b = (byte)_stream.ReadByte();
                    if (escape)
                    {
                        rsp += (char)(b | 0x20);
                        escape = false;
                    }
                    else if (b == 0x7D)
                    {
                        escape = true;
                    }
                    else
                    {
                        rsp += (char)b;
                        if (rsp.StartsWith("$") && rsp.EndsWith("#"))
                        {
                            _stream.ReadByte();
                            _stream.ReadByte();
                            //System.Diagnostics.Debug.Print("QEMU Read: " + rsp);
                            return rsp.Substring(1, rsp.Length - 2);
                        }
                    }
                    sw.Restart();
                }
                //System.Threading.Thread.Sleep(5);
            }
            throw new TimeoutException("Timeout getting response from qemu");
        }
    }
}
