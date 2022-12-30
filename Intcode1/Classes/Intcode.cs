using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intcode1
{
    public class Intcode
    {
        Dictionary<int, int> _dic=new Dictionary<int, int>();

        public Intcode(IEnumerable<int> collection)
        {
            int pos=0;
            foreach (var value in collection)
            {
                _dic.Add(pos, value);
                pos++;
            }
        }

        public Intcode(Intcode intcode)
        {
            foreach (var i in intcode.GetItems())
            {
                _dic.Add(i.Key, i.Value);
            }
        }

        public bool Contains(int pos) => _dic.ContainsKey(pos);
         
        public int Get(int pos) => _dic[pos];

        public void Set(int pos,int value)
        {
            if(_dic.ContainsKey(pos)) 
                _dic[pos] = value;
            else 
                _dic.Add(pos, value);
        }

        public IEnumerable<KeyValuePair<int,int>> GetItems() 
        {
            foreach (var item in _dic)
            {
                yield return item;
            }
        }

        public IEnumerable<KeyValuePair<int, int>> Items
        {
            get { return GetItems();}
        }

        public IEnumerable<int> GetState()
        {
            var res = new List<int>();
            int max= Items.Max(i => i.Key);
            int def= -9999; 
            for(int i = 0; i <= max;i++)
            {
                if (_dic.ContainsKey(i)) 
                    res.Add(_dic[i]);
                else
                    res.Add(def);
            }
            return res;
        }

        // Pseudocode
        readonly List<string> _Pseudocode = new List<string>();
        public void AddPseudocode(string line) { _Pseudocode.Add(line); }
        public List<string> Pseudocode { get => _Pseudocode; }

        // Engine
        int pointer = 0;
        Opcode? _LastInstruction = null;
        int _ExitCode = 0;
        public int Execute()
        {
            pointer = 0;
            Opcode opcode;
            while (true)
            {
                opcode = NextInstruction(pointer);

                if (opcode.Halted) { _ExitCode = 0; break; } // 99
                if (opcode.IsUknown) { _ExitCode = 1; break; } // opcode desconocido
                
                opcode.Execute();

                if (opcode.Failed) { _ExitCode = 2; break; } // Error critico

                pointer +=opcode.Length;
            }
            _LastInstruction=opcode; 
            return _ExitCode;
        }

        private Opcode NextInstruction(int pointer)
        {
            var p=Get(pointer);
            return p switch
            {
                1 => new OpcodeSum(pointer, this),
                2 => new OpcodeMul(pointer, this),
                99 => new Opcode(99, 0, pointer,this),
                _ => new Opcode(0, 0, pointer,this),
            };
        }
        
    }

    public class Opcode
    {
        public int Code { get; private set;}
        public int Length { get; private set; }
        public int Offset { get; private set; }

        internal Intcode mem;

        public Opcode(int code, int length, int offset, Intcode _mem)
        {
            Code = code;
            Length = length;    
            Offset = offset;
            mem = _mem;
        }

        public virtual void Execute()
        {
            
        }

        public bool IsUknown => Code == 0;
        public bool Halted => Code == 99;

        public bool Failed => CriticalPos != -1;
        public int CriticalPos = -1;

        public bool TryGetParam(int pos, out int param)
        {
            param = 0;
            if (!mem.Contains(pos)) { CriticalPos = pos; return false; }
            var pos1 = mem.Get(pos);
            if (!mem.Contains(pos1)) { CriticalPos = pos1; return false; }
            param = mem.Get(pos1);
            return true;
        }

        public void SetParam(int pos, int param)
        {
            if (!mem.Contains(pos)) { CriticalPos = pos; return; }
            var pos1 = mem.Get(pos);
            mem.Set(pos1, param);
        }

        public bool TryGetParam(int pos, out int param, out int index)
        {
            param = 0;
            index = -1;
            if (!mem.Contains(pos)) { CriticalPos = pos; return false; }
            var pos1 = mem.Get(pos);
            index=pos1;
            if (!mem.Contains(pos1)) { CriticalPos = pos1; return false; }
            param = mem.Get(pos1);
            return true;
        }
        public void SetParam(int pos, int param, out int index)
        {
            index = -1;
            if (!mem.Contains(pos)) { CriticalPos = pos; return; }
            var pos1 = mem.Get(pos);
            index = pos1;
            mem.Set(pos1, param);
        }
    }

    public class OpcodeSum : Opcode 
    {
        public OpcodeSum(int offset, Intcode _mem) :base(2,4,offset, _mem)  {}

        public override void Execute()
        {
            if (!TryGetParam(Offset + 1, out int s1, out int idx1)) return;
            if (!TryGetParam(Offset + 2, out int s2, out int idx2)) return;

            var res = s1 + s2;
            
            SetParam(Offset + 3, res, out int idx3);
            string str = $"X[{idx3}] = X[{idx1}] + X[{idx2}]";
            mem.AddPseudocode(str);
        }
    }

    public class OpcodeMul : Opcode
    {
        public OpcodeMul(int offset, Intcode _mem) : base(1, 4, offset,_mem) { }

        public override void Execute()
        {
            if (!TryGetParam(Offset + 1, out int m1, out int idx1)) return;
            if (!TryGetParam(Offset + 2, out int m2, out int idx2)) return;

            var res = m1 * m2;

            SetParam(Offset + 3, res, out int idx3);
            string str = $"X[{idx3}] = X[{idx1}] * X[{idx2}]";
            mem.AddPseudocode(str);
        }
    }

    public static class CreateIntCode
    {
        public static Intcode FromFile(string file)
        {
            var result = FromString(File.ReadAllText(file));
            return result;
        }

        public static Intcode FromString(string str)
        {
            var ints = str.Split(',', StringSplitOptions.TrimEntries).Select(x => int.Parse(x));
            var result = new Intcode(ints);
            return result;
        }

        public static Intcode FromIntcode(Intcode intcode)
        {
            var result = new Intcode(intcode);
            return result;
        }
    }

    public class Benchmark 
    {
        private readonly Stopwatch timer = new Stopwatch();
        public readonly string Name;

        public Benchmark(string benchmarkName)
        {
            Name = benchmarkName;
        }
         
        public void Start()
        {
            timer.Start();
        }
        public void Restart()
        {
            timer.Restart();
        }
        public void Stop()
        {
            timer.Stop();
        }

        public override string ToString()
        {
            return $"{timer.Elapsed}"; 
        }
    }

    public class TExecutionResult
    {
        public int ExitCode { get; private set; }
        public int Result { get; private set; }

        public TExecutionResult(int exitCode, int result)
        {
            ExitCode = exitCode;
            Result = result;
        }

        public bool IsOk
        {
            get { return ExitCode == 0;}
        }

        public bool IsUnknownOpcode
        {
            get { return ExitCode == 1; }
        }

        public bool IsInvalidMemoryIndex
        {
            get { return ExitCode == 2; }
        }
    }

}
