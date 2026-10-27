using System;
using System.Collections.Generic;

namespace PetGemBattle
{
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int X, Y;
        public Cell(int x, int y) { X = x; Y = y; }
        public bool Equals(Cell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Cell other && Equals(other);
        public override int GetHashCode() => X * 397 ^ Y;
    }
    public readonly struct BoardMove
    {
        public readonly Cell A, B;
        public BoardMove(Cell a, Cell b) { A = a; B = b; }
    }
    public sealed class MatchGroup
    {
        public GemKind Kind;
        public readonly HashSet<Cell> Cells = new HashSet<Cell>();
        public int EffectiveCount;
        public int PhysicalCount => Cells.Count;
        public int Units => EffectiveCount > 0 ? EffectiveCount : Cells.Count;
    }
    public readonly struct GemFall
    {
        public readonly Cell Target;
        public readonly int SourceY;
        public GemFall(Cell target, int sourceY) { Target = target; SourceY = sourceY; }
    }

    // Coordinates have y=0 at the bottom. This model never depends on scene objects.
    public sealed class MatchBoard
    {
        public const int Size = BattleRules.BoardSize;
        private readonly int[,] cells = new int[Size, Size];
        private readonly byte[,] bonuses = new byte[Size, Size];
        private readonly Random random;
        private readonly Random bonusRandom;
        public int this[int x, int y] => cells[x, y];
        public int Multiplier(int x,int y) => bonuses[x,y] == 0 ? 1 : bonuses[x,y];
        public MatchBoard(int seed) { random = new Random(seed); bonusRandom=new Random(seed ^ 0x53A91); GeneratePlayable(); }
        private byte RollBonus(){double roll=bonusRandom.NextDouble();return roll<.895 ? (byte)1 : roll<.975 ? (byte)2 : roll<.995 ? (byte)3 : (byte)4;}
        public static bool Inside(Cell c) => c.X >= 0 && c.X < Size && c.Y >= 0 && c.Y < Size;
        public static bool Adjacent(Cell a, Cell b) => Inside(a) && Inside(b) && Math.Abs(a.X-b.X) + Math.Abs(a.Y-b.Y) == 1;
        public void Swap(BoardMove move)
        {
            int old = cells[move.A.X, move.A.Y];
            cells[move.A.X, move.A.Y] = cells[move.B.X, move.B.Y];
            cells[move.B.X, move.B.Y] = old;
            byte boost=bonuses[move.A.X,move.A.Y];bonuses[move.A.X,move.A.Y]=bonuses[move.B.X,move.B.Y];bonuses[move.B.X,move.B.Y]=boost;
        }
        public bool TrySwap(BoardMove move)
        {
            if (!Adjacent(move.A, move.B)) return false;
            Swap(move);
            if (FindMatches().Count > 0) return true;
            Swap(move);
            return false;
        }
        public List<BoardMove> LegalMoves()
        {
            var result = new List<BoardMove>();
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    if (x + 1 < Size) CheckMove(new BoardMove(new Cell(x,y), new Cell(x+1,y)), result);
                    if (y + 1 < Size) CheckMove(new BoardMove(new Cell(x,y), new Cell(x,y+1)), result);
                }
            return result;
        }
        private void CheckMove(BoardMove move, List<BoardMove> result)
        {
            Swap(move);
            if (FindMatches().Count > 0) result.Add(move);
            Swap(move);
        }
        public List<MatchGroup> FindMatches()
        {
            var groups = new List<MatchGroup>();
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size;)
                {
                    int end = x + 1;
                    while (end < Size && cells[end,y] == cells[x,y]) end++;
                    if (cells[x,y] >= 0 && end-x >= 3)
                    {
                        var group = new MatchGroup { Kind = (GemKind)cells[x,y] };
                        for (int i = x; i < end; i++) group.Cells.Add(new Cell(i,y));
                        groups.Add(group);
                    }
                    x = end;
                }
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size;)
                {
                    int end = y + 1;
                    while (end < Size && cells[x,end] == cells[x,y]) end++;
                    if (cells[x,y] >= 0 && end-y >= 3)
                    {
                        var group = new MatchGroup { Kind = (GemKind)cells[x,y] };
                        for (int i = y; i < end; i++) group.Cells.Add(new Cell(x,i));
                        groups.Add(group);
                    }
                    y = end;
                }
            // Union intersecting runs, including transitive T/L/cross shapes, exactly once.
            for (int i = 0; i < groups.Count; i++)
                for (int j = i+1; j < groups.Count; j++)
                    if (groups[i].Kind == groups[j].Kind && Connected(groups[i],groups[j]))
                    {
                        groups[i].Cells.UnionWith(groups[j].Cells);
                        groups.RemoveAt(j);
                        j = i; // Restart because the union may now intersect an earlier run.
                    }
            foreach(var group in groups)foreach(var cell in group.Cells)group.EffectiveCount+=Multiplier(cell.X,cell.Y);
            return groups;
        }
        private static bool Connected(MatchGroup a,MatchGroup b)
        {
            foreach(var cell in a.Cells)
                foreach(var other in b.Cells)
                    if(cell.Equals(other) || Adjacent(cell,other))return true;
            return false;
        }
        public List<GemFall> ClearAndRefill(List<MatchGroup> groups)
        {
            foreach (var group in groups)
                foreach (var c in group.Cells){cells[c.X,c.Y] = -1;bonuses[c.X,c.Y]=0;}
            var falls = new List<GemFall>();
            for (int x = 0; x < Size; x++)
            {
                int target = 0;
                for (int y = 0; y < Size; y++)
                    if (cells[x,y] >= 0)
                    {
                        cells[x,target] = cells[x,y];
                        bonuses[x,target] = bonuses[x,y];
                        falls.Add(new GemFall(new Cell(x,target), y));
                        target++;
                    }
                int spawn = Size;
                while (target < Size)
                {
                    cells[x,target] = random.Next(BattleRules.GemKinds);
                    bonuses[x,target] = RollBonus();
                    falls.Add(new GemFall(new Cell(x,target++), spawn++));
                }
            }
            return falls;
        }
        public void GeneratePlayable()
        {
            do
            {
                for (int y = 0; y < Size; y++)
                    for (int x = 0; x < Size; x++)
                    {
                        int kind;
                        do { kind = random.Next(BattleRules.GemKinds); }
                        while ((x >= 2 && cells[x-1,y] == kind && cells[x-2,y] == kind) ||
                               (y >= 2 && cells[x,y-1] == kind && cells[x,y-2] == kind));
                        cells[x,y] = kind;
                        bonuses[x,y] = RollBonus();
                    }
            } while (LegalMoves().Count == 0);
        }
        public void ShufflePlayable()
        {
            // Preserve the multiset when possible; pathological boards get a fresh deal.
            var values = Snapshot();
            var boosts = MultiplierSnapshot();
            for (int attempt = 0; attempt < 200; attempt++)
            {
                for (int i = values.Length-1; i > 0; i--)
                { int j = random.Next(i+1); int v = values[i]; values[i] = values[j]; values[j] = v; int b=boosts[i];boosts[i]=boosts[j];boosts[j]=b; }
                Load(values,boosts);
                if (FindMatches().Count == 0 && LegalMoves().Count > 0) return;
            }
            GeneratePlayable();
        }
        public int[] Snapshot()
        {
            var values = new int[Size*Size];
            for (int y=0; y<Size; y++) for (int x=0; x<Size; x++) values[y*Size+x]=cells[x,y];
            return values;
        }
        public int[] MultiplierSnapshot()
        {
            var values=new int[Size*Size];
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)values[y*Size+x]=Multiplier(x,y);
            return values;
        }
        public void Load(int[] values)
        {
            var boosts=new int[Size*Size];for(int i=0;i<boosts.Length;i++)boosts[i]=1;
            Load(values,boosts);
        }
        public void Load(int[] values,int[] boosts)
        {
            if (values == null || values.Length != Size*Size) throw new ArgumentException("Board requires 64 cells.");
            if (boosts == null || boosts.Length != Size*Size) throw new ArgumentException("Board requires 64 multipliers.");
            for (int i=0; i<values.Length; i++)
                if (values[i] < 0 || values[i] >= BattleRules.GemKinds) throw new ArgumentException("Unknown gem.");
            for (int i=0; i<boosts.Length; i++)if(boosts[i]<1 || boosts[i]>4)throw new ArgumentException("Unknown multiplier.");
            for (int y=0; y<Size; y++) for (int x=0; x<Size; x++){cells[x,y]=values[y*Size+x];bonuses[x,y]=(byte)boosts[y*Size+x];}
        }
    }
}
